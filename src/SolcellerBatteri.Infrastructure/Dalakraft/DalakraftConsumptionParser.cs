using System.Globalization;
using SolcellerBatteri.Domain.Models;

namespace SolcellerBatteri.Infrastructure.Dalakraft;

public static class DalakraftConsumptionParser
{
    private static readonly CultureInfo Sv = CultureInfo.GetCultureInfo("sv-SE");

    public sealed class HourlyConsumption
    {
        public List<EnergyRecord> Records { get; } = new();
        public int SkippedRows { get; init; }
    }

    /// <summary>
    /// Parser för Dalakraft-fil med kvartsvärden (15 min) och pris i öre/kWh exkl moms.
    /// Förväntar kolumner:
    /// Tidpunkt, Dala Elfond (öre/kWh) exkl moms, Din förbrukning, Dala Elfond kvartskostnad exkl moms
    ///
    /// Separator: tab (TSV) eller semikolon/komma (vi försöker).
    /// Decimal: svensk (komma).
    /// </summary>
    public static HourlyConsumption ParseQuarterlyToHourly(string fileContent)
    {
        if (string.IsNullOrWhiteSpace(fileContent))
            throw new ArgumentException("File content is empty.", nameof(fileContent));

        var lines = fileContent
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (lines.Count < 2)
            throw new InvalidOperationException("Not enough rows.");

        // Hitta separator från header-raden
        var header = lines[0];
        char sep = DetectSeparator(header);

        // Index på kolumner
        var headers = header.Split(sep).Select(h => h.Trim()).ToArray();

        int idxTime = Find(headers, "Tidpunkt");
        int idxPriceOre = Find(headers, "Dala Elfond (öre/kWh) exkl moms");
        int idxKwh = Find(headers, "Din förbrukning");
        // idxCostOre är optional
        int idxCostOre = TryFind(headers, "Dala Elfond kvartskostnad exkl moms");

        var quarterRows = new List<(DateTime ts, double kwh, double priceOrePerKwh)>();
        int skipped = 0;

        for (int i = 1; i < lines.Count; i++)
        {
            var parts = lines[i].Split(sep);
            if (parts.Length < headers.Length)
            {
                skipped++;
                continue;
            }

            var timeRaw = parts[idxTime].Trim();

            // // Skippa rader utan klockslag (t.ex. "2025-10-01")
            // if (!timeRaw.Contains(':'))
            // {
            //     skipped++;
            //     continue;
            // }

            if (!TryParseDalakraftDateTime(timeRaw, out var ts))
            {
                skipped++;
                continue;
            }

            if (!TryParseDouble(parts[idxKwh], out var kwh))
            {
                skipped++;
                continue;
            }

            if (!TryParseDouble(parts[idxPriceOre], out var priceOre))
            {
                skipped++;
                continue;
            }

            // Sanity: negativa kWh borde inte finnas här
            if (kwh < 0)
            {
                skipped++;
                continue;
            }

            quarterRows.Add((ts, kwh, priceOre));
        }

        // Grupp per timme
        var hourly = quarterRows
            .GroupBy(r => new DateTime(r.ts.Year, r.ts.Month, r.ts.Day, r.ts.Hour, 0, 0, DateTimeKind.Unspecified))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var importKwh = g.Sum(x => x.kwh);

                // Viktat snittpris i öre/kWh (viktat på kWh)
                // Om importKwh=0: sätt null eller 0. Vi sätter null i pris då.
                double? sekPerKwh = null;
                if (importKwh > 0)
                {
                    var weightedOre = g.Sum(x => x.kwh * x.priceOrePerKwh) / importKwh;
                    sekPerKwh = weightedOre / 100.0; // öre/kWh -> SEK/kWh (exkl moms)
                }

                return new EnergyRecord
                {
                    Timestamp = g.Key,          // svensk lokal tid (okind)
                    ImportKWh = importKwh,
                    ExportKWh = 0.0,            // fylls från export-fil senare
                    SpotPriceSekPerKWh = sekPerKwh
                };
            })
            .ToList();

        return new HourlyConsumption
        {
            Records = hourly,
            SkippedRows = skipped
        };
    }

    private static char DetectSeparator(string headerLine)
    {
        // Dalakraft verkar ofta vara tab-separerad. Men vi gissar robust.
        if (headerLine.Contains('\t')) return '\t';
        if (headerLine.Contains(';')) return ';';
        if (headerLine.Contains(',')) return ',';
        return '\t';
    }

    private static int Find(string[] headers, string name)
    {
        var idx = Array.FindIndex(headers, h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) throw new InvalidOperationException($"Missing column: {name}");
        return idx;
    }

    private static int TryFind(string[] headers, string name)
        => Array.FindIndex(headers, h => string.Equals(h, name, StringComparison.OrdinalIgnoreCase));

    private static bool TryParseDouble(string raw, out double value)
    {
        raw = raw.Trim();

        // Hantera "0,31" osv
        return double.TryParse(raw, NumberStyles.Float, Sv, out value);
    }

   
private static bool TryParseDalakraftDateTime(string raw, out DateTime dt)
{
    raw = raw.Trim();

    // Om bara datum finns: tolka som 00:00
    if (!raw.Contains(':'))
    {
        // Ex: "2025-10-01" → "2025-10-01 00:00"
        raw = raw + " 00:00";
    }

    string[] formats =
    {
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd H:mm",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd H:mm:ss"
    };

    return DateTime.TryParseExact(
        raw,
        formats,
        Sv,
        DateTimeStyles.None,
        out dt
    );
}

}
