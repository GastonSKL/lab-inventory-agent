using LabInventory.Api.Domain;

namespace LabInventory.Api.Data;

/// <summary>
/// Datos 100% sintéticos. Las fechas se calculan relativas a "hoy" para que siempre haya
/// lotes vencidos, por vencer y sanos, sin importar cuándo se levanta la base.
/// </summary>
public static class SeedData
{
    public static void Seed(InventoryDbContext db, TimeProvider time)
    {
        if (db.Products.Any())
        {
            return;
        }

        var now = time.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);
        DateOnly In(int days) => today.AddDays(days);

        var products = new List<Product>
        {
            Reagent("REA-001", "Etanol 96%", "L", 20, "Inflamable. Almacenar entre 15 y 25 °C, lejos de fuentes de ignición.",
                ("ET-2401", 12, In(-10)), ("ET-2405", 18, In(25)), ("ET-2410", 25, In(400))),
            Reagent("REA-002", "Ácido sulfúrico 98%", "L", 5, "Corrosivo. Armario de ácidos ventilado, separado de bases.",
                ("AS-2311", 3.5m, In(200))),
            Reagent("REA-003", "Hidróxido de sodio en lentejas", "kg", 3, "Corrosivo e higroscópico. Envase bien cerrado.",
                ("HS-2402", 2, In(5)), ("HS-2409", 6, In(700))),
            Reagent("REA-004", "Acetonitrilo grado HPLC", "L", 8, "Inflamable y tóxico. Gabinete para inflamables.",
                ("AC-2406", 4, In(12)), ("AC-2408", 8, In(90))),
            Reagent("REA-005", "Metanol grado HPLC", "L", 10, "Inflamable y tóxico. Gabinete para inflamables.",
                ("ME-2407", 16, In(300))),
            Reagent("REA-006", "Solución buffer pH 7,00", "L", 2, "Refrigerar entre 2 y 8 °C. No congelar.",
                ("BF-2409", 0.5m, In(3)), ("BF-2410", 1, In(60))),
            Reagent("REA-007", "Agar nutritivo", "kg", 1, "Lugar fresco y seco, protegido de la luz.",
                ("AG-2312", 0.8m, In(-30))),
            Reagent("REA-008", "Cloruro de sodio p.a.", "kg", 2, "Lugar seco.",
                ("CS-2404", 5, In(900))),

            Consumable("CON-001", "Guantes de nitrilo talle M", "caja x100", 10, null,
                ("GN-2402", 4, In(500)), ("GN-2408", 3, In(800))),
            Consumable("CON-002", "Puntas de micropipeta 200 µL", "rack x96", 15, null,
                ("PM-2405", 40, null)),
            Consumable("CON-003", "Papel de filtro cualitativo 125 mm", "paquete x100", 5, null,
                ("PF-2403", 2, null)),
            Consumable("CON-004", "Tubos cónicos 50 mL", "bolsa x25", 8, null,
                ("TC-2406", 12, null)),

            new()
            {
                Sku = "GLA-001", Name = "Matraz aforado 100 mL", Category = ProductCategory.Glassware,
                Unit = "unidad", ReorderPoint = 4,
                Lots = [new() { LotNumber = "MA-2401", Quantity = 6, ReceivedOn = In(-300) }],
            },
            new()
            {
                Sku = "GLA-002", Name = "Vaso de precipitado 250 mL", Category = ProductCategory.Glassware,
                Unit = "unidad", ReorderPoint = 10,
                Lots = [new() { LotNumber = "VP-2402", Quantity = 7, ReceivedOn = In(-250) }],
            },

            new()
            {
                Sku = "SEG-001", Name = "Antiparras de seguridad", Category = ProductCategory.SafetyEquipment,
                Unit = "unidad", ReorderPoint = 5,
                Lots = [new() { LotNumber = "AP-2403", Quantity = 9, ReceivedOn = In(-200) }],
            },
            new()
            {
                Sku = "SEG-002", Name = "Respirador N95", Category = ProductCategory.SafetyEquipment,
                Unit = "caja x20", ReorderPoint = 3,
                Lots = [new() { LotNumber = "RN-2312", Quantity = 2, ExpirationDate = In(20), ReceivedOn = In(-340) }],
            },
        };

        db.Products.AddRange(products);
        db.SaveChanges();

        var bySku = products.ToDictionary(p => p.Sku);
        db.RestockOrders.AddRange(
            new RestockOrder
            {
                ProductId = bySku["REA-002"].Id, Quantity = 10, Status = RestockOrderStatus.Pending,
                CreatedAtUtc = now.AddDays(-2), RequestedBy = "lab.analista1", Notes = "Stock bajo el punto de pedido.",
            },
            new RestockOrder
            {
                ProductId = bySku["CON-002"].Id, Quantity = 20, Status = RestockOrderStatus.Received,
                CreatedAtUtc = now.AddDays(-20), RequestedBy = "lab.jefe",
            });
        db.SaveChanges();

        // Helpers locales para que la lista de arriba se lea como una tabla.
        Product Reagent(string sku, string name, string unit, decimal reorderPoint, string storage,
            params (string LotNumber, decimal Quantity, DateOnly Expiration)[] lots) => new()
        {
            Sku = sku, Name = name, Category = ProductCategory.Reagent, Unit = unit,
            ReorderPoint = reorderPoint, StorageConditions = storage,
            Lots = lots.Select(l => new Lot
            {
                LotNumber = l.LotNumber, Quantity = l.Quantity, ExpirationDate = l.Expiration,
                ReceivedOn = l.Expiration.AddDays(-365),
            }).ToList(),
        };

        Product Consumable(string sku, string name, string unit, decimal reorderPoint, string? storage,
            params (string LotNumber, decimal Quantity, DateOnly? Expiration)[] lots) => new()
        {
            Sku = sku, Name = name, Category = ProductCategory.Consumable, Unit = unit,
            ReorderPoint = reorderPoint, StorageConditions = storage,
            Lots = lots.Select(l => new Lot
            {
                LotNumber = l.LotNumber, Quantity = l.Quantity, ExpirationDate = l.Expiration,
                ReceivedOn = today.AddDays(-120),
            }).ToList(),
        };
    }
}
