// src/SolcellerBatteri.Domain/Models/EnergyRecord.cs
namespace SolcellerBatteri.Domain.Models;

public class EnergyRecord
{
    /// <summary>
    /// Lokal svensk tid (t.ex. 2024-06-01 12:00)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Köpt el från nätet under denna timme (kWh)
    /// </summary>
    public double ImportKWh { get; set; }

    /// <summary>
    /// Såld/Exporterad el till nätet under denna timme (kWh)
    /// </summary>
    public double ExportKWh { get; set; }

    /// <summary>
    /// Spotpris i SEK/kWh för denna timme (kan vara null om okänt)
    /// </summary>
    public double? SpotPriceSekPerKWh { get; set; }
}
