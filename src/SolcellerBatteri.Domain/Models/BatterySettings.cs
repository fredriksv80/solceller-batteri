// src/SolcellerBatteri.Domain/Models/BatterySettings.cs
namespace SolcellerBatteri.Domain.Models;

public class BatterySettings
{
    /// <summary>
    /// Batteriets användbara kapacitet i kWh (t.ex. 5, 7, 10)
    /// </summary>
    public double BatteryCapacityKWh { get; set; }

    /// <summary>
    /// Max ladd-/urladdningseffekt i kW.
    /// Första versionen: antag samma för båda riktningarna.
    /// </summary>
    public double MaxPowerKW { get; set; } = 3.0;

    /// <summary>
    /// Total verkningsgrad (round-trip), t.ex. 0.9 för 90 %
    /// </summary>
    public double RoundTripEfficiency { get; set; } = 0.90;

    /// <summary>
    /// Startnivå i batteriet i kWh (standard 0)
    /// </summary>
    public double InitialStateOfChargeKWh { get; set; } = 0.0;
}
