namespace LabInventory.Api.Domain;

public enum RestockOrderStatus
{
    Pending,
    Received,
    Cancelled,
}

public class RestockOrder
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public decimal Quantity { get; set; }

    public RestockOrderStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public required string RequestedBy { get; set; }

    public string? Notes { get; set; }
}
