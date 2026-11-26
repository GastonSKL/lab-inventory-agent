using LabInventory.Api.Domain;

namespace LabInventory.Api.Endpoints;

public record ProductSummary(int Id, string Sku, string Name, string Category, string Unit);

public record ProductDetail(
    int Id,
    string Sku,
    string Name,
    string Category,
    string Unit,
    decimal ReorderPoint,
    string? StorageConditions);

public record LotStock(string LotNumber, decimal Quantity, DateOnly? ExpirationDate, bool IsExpired);

public record ProductStock(
    int ProductId,
    string Sku,
    string Name,
    string Unit,
    decimal ReorderPoint,
    decimal AvailableQuantity,
    decimal ExpiredQuantity,
    bool BelowReorderPoint,
    IReadOnlyList<LotStock> Lots);

public record ExpiringLot(
    int LotId,
    string LotNumber,
    int ProductId,
    string Sku,
    string ProductName,
    decimal Quantity,
    string Unit,
    DateOnly ExpirationDate,
    int DaysUntilExpiration);

public record RestockOrderView(
    int Id,
    int ProductId,
    string Sku,
    string ProductName,
    decimal Quantity,
    string Unit,
    string Status,
    DateTime CreatedAtUtc,
    string RequestedBy,
    string? Notes)
{
    public static RestockOrderView From(RestockOrder o) => new(
        o.Id, o.ProductId, o.Product.Sku, o.Product.Name, o.Quantity, o.Product.Unit,
        o.Status.ToString(), o.CreatedAtUtc, o.RequestedBy, o.Notes);
}
