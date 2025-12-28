namespace SolcellerBatteri.Domain.Models;

public class SpotPricePoint
{
    public DateTimeOffset Time { get; set; }
    public decimal PriceEurPerMWh { get; set; }
}
