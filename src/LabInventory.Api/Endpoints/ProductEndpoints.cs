using System.ComponentModel.DataAnnotations;
using LabInventory.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Api.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", SearchProducts)
            .WithSummary("Busca productos por nombre o SKU (contiene, sin distinguir mayúsculas).");

        group.MapGet("/{id:int}", GetProduct)
            .WithSummary("Detalle de un producto.");

        group.MapGet("/{id:int}/stock", GetProductStock)
            .WithSummary("Stock disponible y vencido de un producto, desglosado por lote.");

        return group;
    }

    private static async Task<Ok<List<ProductSummary>>> SearchProducts(
        InventoryDbContext db,
        CancellationToken ct,
        [MaxLength(100)] string? search = null)
    {
        var query = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // LIKE de SQLite: case-insensitive solo para ASCII, y no ignora tildes ("acido" no encuentra "Ácido").
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.Like(p.Name, pattern) || EF.Functions.Like(p.Sku, pattern));
        }

        var products = await query
            .OrderBy(p => p.Sku)
            .Select(p => new ProductSummary(p.Id, p.Sku, p.Name, p.Category.ToString(), p.Unit))
            .ToListAsync(ct);

        return TypedResults.Ok(products);
    }

    private static async Task<Results<Ok<ProductDetail>, NotFound>> GetProduct(
        int id, InventoryDbContext db, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDetail(
                p.Id, p.Sku, p.Name, p.Category.ToString(), p.Unit, p.ReorderPoint, p.StorageConditions))
            .FirstOrDefaultAsync(ct);

        return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
    }

    private static async Task<Results<Ok<ProductStock>, NotFound>> GetProductStock(
        int id, InventoryDbContext db, TimeProvider time, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking()
            .Include(p => p.Lots)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
        {
            return TypedResults.NotFound();
        }

        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);

        // Las sumas se hacen en memoria: SQLite no soporta agregaciones sobre decimal.
        var lots = product.Lots
            .Where(l => l.Quantity > 0)
            .OrderBy(l => l.ExpirationDate ?? DateOnly.MaxValue)
            .Select(l => new LotStock(l.LotNumber, l.Quantity, l.ExpirationDate, l.IsExpired(today)))
            .ToList();

        var available = lots.Where(l => !l.IsExpired).Sum(l => l.Quantity);
        var expired = lots.Where(l => l.IsExpired).Sum(l => l.Quantity);

        return TypedResults.Ok(new ProductStock(
            product.Id, product.Sku, product.Name, product.Unit, product.ReorderPoint,
            available, expired, available < product.ReorderPoint, lots));
    }
}
