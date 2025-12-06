// src/SolcellerBatteri.Domain/BatterySimulator.cs
using SolcellerBatteri.Domain.Models;

namespace SolcellerBatteri.Domain;

public class BatterySimulator
{
    /// <summary>
    /// Enkel första version som bara räknar lite statistik,
    /// så vi får nåt vettigt svar från /simulate.
    /// Själva batterilogiken bygger vi ut i nästa steg.
    /// </summary>
    public BatteryResult Simulate(IEnumerable<EnergyRecord> energyData, BatterySettings settings)
    {
        if (energyData is null)
            throw new ArgumentNullException(nameof(energyData));
        if (settings is null)
            throw new ArgumentNullException(nameof(settings));

        var list = energyData.ToList();

        // Placeholder-logik:
        // Just nu räknar vi bara antal timmar och summerar lite.
        // Sen byter vi detta mot "riktig" batterisimulering.
        double totalImport = list.Sum(e => e.ImportKWh);
        double totalExport = list.Sum(e => e.ExportKWh);

        return new BatteryResult
        {
            BatteryCapacityKWh = settings.BatteryCapacityKWh,
            TotalHoursSimulated = list.Count,
            ReducedGridImportKWh = 0.0,            // TODO: räkna "på riktigt"
            IncreasedSelfConsumptionKWh = 0.0,     // TODO: räkna "på riktigt"
            EstimatedAnnualSavingsSek = 0.0        // TODO: räkna "på riktigt"
        };
    }
}
