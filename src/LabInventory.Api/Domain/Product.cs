namespace LabInventory.Api.Domain;

public enum ProductCategory
{
    Reagent,
    Consumable,
    Glassware,
    SafetyEquipment,
}

public class Product
{
    public int Id { get; set; }

    /// <summary>Código interno único, por ejemplo "REA-001".</summary>
    public required string Sku { get; set; }

    public required string Name { get; set; }

    public ProductCategory Category { get; set; }

    /// <summary>Unidad en la que se mide el stock: "L", "kg", "caja x100", etc.</summary>
    public required string Unit { get; set; }

    /// <summary>Por debajo de esta cantidad disponible hay que reponer.</summary>
    public decimal ReorderPoint { get; set; }

    public string? StorageConditions { get; set; }

    public List<Lot> Lots { get; set; } = [];
}
