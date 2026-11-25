namespace LabInventory.Api.Domain;

/// <summary>
/// Registro de auditoría de cada ajuste manual de stock sobre un lote.
/// Delta positivo suma, negativo resta (rotura, consumo no registrado, conteo físico).
/// </summary>
public class StockAdjustment
{
    public int Id { get; set; }

    public int LotId { get; set; }

    public Lot Lot { get; set; } = null!;

    public decimal Delta { get; set; }

    public required string Reason { get; set; }

    public required string AdjustedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
