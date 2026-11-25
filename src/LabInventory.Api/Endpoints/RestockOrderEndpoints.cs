using LabInventory.Api.Data;
using LabInventory.Api.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Api.Endpoints;

public static class RestockOrderEndpoints
{
    public static RouteGroupBuilder MapRestockOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Restock orders");

        group.MapGet("/", ListOrders)
            .WithSummary("Pedidos de reposición, opcionalmente filtrados por estado (Pending, Received, Cancelled).");

        group.MapGet("/{id:int}", GetOrder)
            .WithSummary("Detalle de un pedido de reposición.");

        // TODO (Gastón): POST /api/orders y POST /api/lots/{id}/adjustments. Ver docs/phase-1-write-endpoints.md.

        return group;
    }

    private static async Task<Ok<List<RestockOrderView>>> ListOrders(
        InventoryDbContext db, CancellationToken ct, RestockOrderStatus? status = null)
    {
        var query = db.RestockOrders.AsNoTracking().Include(o => o.Product).AsQueryable();

        if (status is not null)
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAtUtc).ToListAsync(ct);

        return TypedResults.Ok(orders.Select(RestockOrderView.From).ToList());
    }

    private static async Task<Results<Ok<RestockOrderView>, NotFound>> GetOrder(
        int id, InventoryDbContext db, CancellationToken ct)
    {
        var order = await db.RestockOrders.AsNoTracking()
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return order is null ? TypedResults.NotFound() : TypedResults.Ok(RestockOrderView.From(order));
    }
}
