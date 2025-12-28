using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using SolcellerBatteri.Domain.Models;
using SolcellerBatteri.Domain.Interfaces;
// justera namespace till ditt projekt
namespace SolcellerBatteri.Infrastructure.Services;



public class EntsoeSpotPriceClient : IEntsoeSpotPriceClient
{
    private const string BaseUrl = "https://web-api.tp.entsoe.eu/api";
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;

    // 🔹 Nu tar vi in token som parameter, ingen IConfiguration här inne längre
    public EntsoeSpotPriceClient(HttpClient httpClient, string apiToken)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);

        _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
        if (string.IsNullOrWhiteSpace(_apiToken))
            throw new ArgumentException("API token must not be empty.", nameof(apiToken));
    }

    public async Task<IReadOnlyList<SpotPricePoint>> GetSpotPricesAsync(
        string biddingZone,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var startUtc = DateTime.SpecifyKind(start, DateTimeKind.Local).ToUniversalTime();
        var endUtc   = DateTime.SpecifyKind(end,   DateTimeKind.Local).ToUniversalTime();

        const string format = "yyyyMMddHHmm";
        string periodStart = startUtc.ToString(format, CultureInfo.InvariantCulture);
        string periodEnd   = endUtc.ToString(format, CultureInfo.InvariantCulture);

        var query = new StringBuilder();
        query.Append("api?");
        query.Append("securityToken=").Append(Uri.EscapeDataString(_apiToken));
        query.Append("&documentType=A44");
        query.Append("&processType=A01");
        query.Append("&in_Domain=").Append(Uri.EscapeDataString(biddingZone));
        query.Append("&out_Domain=").Append(Uri.EscapeDataString(biddingZone));
        query.Append("&periodStart=").Append(periodStart);
        query.Append("&periodEnd=").Append(periodEnd);

        using var request = new HttpRequestMessage(HttpMethod.Get, query.ToString());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var xmlString = await response.Content.ReadAsStringAsync(cancellationToken);

        return ParsePricesFromXml(xmlString);
    }

    private static IReadOnlyList<SpotPricePoint> ParsePricesFromXml(string xml)
    {
        var doc = XDocument.Parse(xml);

        XNamespace ns = doc.Root?.Name.Namespace ?? XNamespace.None;
        var results = new List<SpotPricePoint>();

        var timeSeries = doc.Descendants(ns + "TimeSeries");

        foreach (var ts in timeSeries)
        {
            var period = ts.Element(ns + "Period");
            if (period == null) continue;

            var timeInterval = period.Element(ns + "timeInterval");
            if (timeInterval == null) continue;

            var startStr = timeInterval.Element(ns + "start")?.Value;
            var endStr   = timeInterval.Element(ns + "end")?.Value;

            if (!DateTimeOffset.TryParse(startStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var periodStart))
                continue;
            if (!DateTimeOffset.TryParse(endStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var periodEnd))
                continue;

            var points = period.Elements(ns + "Point").ToList();

            foreach (var p in points)
            {
                var posStr   = p.Element(ns + "position")?.Value;
                var priceStr = p.Element(ns + "price.amount")?.Value;

                if (!int.TryParse(posStr, out var position)) continue;
                if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) continue;

                var time = periodStart.AddHours(position - 1);

                results.Add(new SpotPricePoint
                {
                    Time = time,
                    PriceEurPerMWh = price
                });
            }
        }

        return results
            .OrderBy(r => r.Time)
            .ToList();
    }
}
