using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace LabInventory.Api.Tests;

/// <summary>
/// Levanta la API completa contra una base SQLite en memoria y con el reloj fijo,
/// así los tests de vencimientos dan siempre el mismo resultado.
/// </summary>
public class InventoryApiFactory : WebApplicationFactory<Program>
{
    public static readonly DateTimeOffset FixedNow = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    public static DateOnly Today => DateOnly.FromDateTime(FixedNow.UtcDateTime);

    private readonly string _connectionString = $"Data Source=file:inventory-{Guid.NewGuid():N}?mode=memory&cache=shared";

    // Una base SQLite en memoria vive mientras haya al menos una conexión abierta.
    private readonly SqliteConnection _keepAlive;

    public InventoryApiFactory()
    {
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }

    public FakeTimeProvider Time { get; } = new(FixedNow);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Inventory", _connectionString);
        builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(Time));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAlive.Dispose();
        }
    }
}
