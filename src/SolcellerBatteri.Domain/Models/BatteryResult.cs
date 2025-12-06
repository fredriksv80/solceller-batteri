// src/SolcellerBatteri.Domain/Models/BatteryResult.cs
namespace SolcellerBatteri.Domain.Models;

public class BatteryResult
{
    /// <summary>
    /// Kapacitet som användes i simuleringen (kWh)
    /// </summary>
    public double BatteryCapacityKWh { get; set; }

    /// <summary>
    /// Antal timmar som simulerats
    /// </summary>
    public int TotalHoursSimulated { get; set; }

    /// <summary>
    /// Minskad köpt el (kWh) tack vare batteriet
    /// </summary>
    public double ReducedGridImportKWh { get; set; }

    /// <summary>
    /// Ökad egenanvändning av solel (kWh)
    /// </summary>
    public double IncreasedSelfConsumptionKWh { get; set; }

    /// <summary>
    /// Uppskattad årlig besparing i kronor (fyller vi på senare)
    /// </summary>
    public double EstimatedAnnualSavingsSek { get; set; }
}
