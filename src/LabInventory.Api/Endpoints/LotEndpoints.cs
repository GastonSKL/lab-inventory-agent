using System.ComponentModel.DataAnnotations;
using LabInventory.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Api.Endpoints;

public static class LotEndpoints
{
    public static RouteGroupBuilder MapLotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lots").WithTags("Lots");

        group.MapGet("/expiring", GetExpiringLots)
            .WithSummary("Lotes con stock que vencen entre hoy y los próximos N días (1 a 365, por defecto 30).");

        return group;
    }

    private static async Task<Ok<List<ExpiringLot>>> GetExpiringLots(
        InventoryDbContext db,
        TimeProvider time,
        CancellationToken ct,
        [Range(1, 365)] int days = 30)
    {
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var limit = today.AddDays(days);

        var lots = await db.Lots.AsNoTracking()
            .Include(l => l.Product)
            .Where(l => l.ExpirationDate != null && l.ExpirationDate >= today && l.ExpirationDate <= limit)
            .OrderBy(l => l.ExpirationDate)
            .ToListAsync(ct);

        var result = lots
            .Where(l => l.Quantity > 0)
            .Select(l => new ExpiringLot(
                l.Id, l.LotNumber, l.ProductId, l.Product.Sku, l.Product.Name, l.Quantity, l.Product.Unit,
                l.ExpirationDate!.Value, l.ExpirationDate.Value.DayNumber - today.DayNumber))
            .ToList();

        return TypedResults.Ok(result);
    }
}
