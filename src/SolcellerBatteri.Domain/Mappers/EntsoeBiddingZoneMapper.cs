namespace SolcellerBatteri.Domain.Mappers;

public static class EntsoeBiddingZoneMapper
{
    public static string ToEntsoeCode(string area)
    {
        if (string.IsNullOrWhiteSpace(area))
            throw new ArgumentException("Area must be provided.", nameof(area));

        // Normalisera input, t.ex. "se3", " SE3 " etc.
        area = area.Trim().ToUpperInvariant();

       return area switch
        {
            "SE1" => "10Y1001A1001A44P",
            "SE2" => "10Y1001A1001A45N",
            "SE3" => "10Y1001A1001A46L",
            "SE4" => "10Y1001A1001A47J",
            _     => throw new ArgumentOutOfRangeException(nameof(area),
                        area,
                        "Unsupported bidding area. Supported values are SE1, SE2, SE3, SE4.")
        };
    }
}
