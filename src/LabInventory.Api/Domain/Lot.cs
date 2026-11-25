namespace LabInventory.Api.Domain;

/// <summary>
/// Un lote recibido de un producto. El stock de un producto es la suma de sus lotes.
/// </summary>
public class Lot
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public required string LotNumber { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>Null para productos que no vencen (vidriería, algunos consumibles).</summary>
    public DateOnly? ExpirationDate { get; set; }

    public DateOnly ReceivedOn { get; set; }

    public bool IsExpired(DateOnly today) => ExpirationDate is { } date && date < today;
}
