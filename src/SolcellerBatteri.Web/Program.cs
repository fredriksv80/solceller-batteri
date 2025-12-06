using SolcellerBatteri.Domain;
using SolcellerBatteri.Domain.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Lägg till basic API-grejer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrera BatterySimulator så vi kan få in den via dependency injection
builder.Services.AddSingleton<BatterySimulator>();

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

app.Run();
