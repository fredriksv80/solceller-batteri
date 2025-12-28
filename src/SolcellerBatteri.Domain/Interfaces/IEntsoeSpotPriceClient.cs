using SolcellerBatteri.Domain.Models;

namespace SolcellerBatteri.Domain.Interfaces;

public interface IEntsoeSpotPriceClient
{
    Task<IReadOnlyList<SolcellerBatteri.Domain.Models.SpotPricePoint>> GetSpotPricesAsync(
        string biddingZone,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);
}
