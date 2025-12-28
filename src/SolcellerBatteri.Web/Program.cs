using SolcellerBatteri.Domain;
using SolcellerBatteri.Domain.Models;
using SolcellerBatteri.Domain.Interfaces;
using SolcellerBatteri.Domain.Mappers;
using SolcellerBatteri.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Lägg till basic API-grejer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrera BatterySimulator så vi kan få in den via dependency injection
builder.Services.AddSingleton<BatterySimulator>();

//Registrerar tjänst för att hämta pricer
builder.Services.AddHttpClient<IEntsoeSpotPriceClient, EntsoeSpotPriceClient>(
    (httpClient, sp) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var token = configuration["Entsoe:ApiToken"]
                    ?? throw new InvalidOperationException("Entsoe:ApiToken is not configured.");

        return new EntsoeSpotPriceClient(httpClient, token);
    });


var app = builder.Build();

// Swagger för enkel testning av /simulate
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enkel "ping" på /
app.MapGet("/", () => "Solceller + batteri API är igång 🚀");

// Första versionen av /simulate.
// Just nu använder vi fejkade energidata tills vi kopplat på CSV/spotpris.
app.MapPost("/simulate", (BatterySettings settings, BatterySimulator simulator) =>
{
    // TODO: ersätt med riktiga energidata från CSV + spotpris.
    var dummyEnergyData = new List<EnergyRecord>
    {
        new()
        {
            Timestamp = DateTime.SpecifyKind(new DateTime(2024, 6, 1, 12, 0, 0), DateTimeKind.Local),
            ImportKWh = 0.5,
            ExportKWh = 1.2,
            SpotPriceSekPerKWh = 0.80
        },
        new()
        {
            Timestamp = DateTime.SpecifyKind(new DateTime(2024, 6, 1, 13, 0, 0), DateTimeKind.Local),
            ImportKWh = 0.3,
            ExportKWh = 0.9,
            SpotPriceSekPerKWh = 0.75
        }
    };

    var result = simulator.Simulate(dummyEnergyData, settings);
    return Results.Ok(result);
})
.WithName("SimulateBattery")
.WithOpenApi();

app.MapGet("/api/spotprices", async (
    string area,        // t.ex. SE1, SE2, SE3, SE4
    DateTime start,
    DateTime end,
    IEntsoeSpotPriceClient entsoeClient,
    CancellationToken ct) =>
{
    if (end <= start)
    {
        return Results.BadRequest("End must be later than start.");
    }

    string biddingZone;
    try
    {
        biddingZone = EntsoeBiddingZoneMapper.ToEntsoeCode(area);
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(ex.Message);
    }

    var prices = await entsoeClient.GetSpotPricesAsync(biddingZone, start, end, ct);
    return Results.Ok(prices);
})
.WithName("GetSpotPricesByArea")
.WithOpenApi();

app.Run();
