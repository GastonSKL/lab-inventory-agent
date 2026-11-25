using LabInventory.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LabInventory.Api.Data;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<RestockOrder> RestockOrders => Set<RestockOrder>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.HasIndex(p => p.Sku).IsUnique();
            product.Property(p => p.Sku).HasMaxLength(20);
            product.Property(p => p.Name).HasMaxLength(200);
            product.Property(p => p.Unit).HasMaxLength(30);
            product.Property(p => p.Category).HasConversion<string>().HasMaxLength(30);
            product.Property(p => p.ReorderPoint).HasPrecision(18, 3);
        });

        modelBuilder.Entity<Lot>(lot =>
        {
            lot.HasIndex(l => new { l.ProductId, l.LotNumber }).IsUnique();
            lot.Property(l => l.LotNumber).HasMaxLength(50);
            lot.Property(l => l.Quantity).HasPrecision(18, 3);
            lot.HasIndex(l => l.ExpirationDate);
        });

        modelBuilder.Entity<RestockOrder>(order =>
        {
            order.Property(o => o.Quantity).HasPrecision(18, 3);
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            order.Property(o => o.RequestedBy).HasMaxLength(100);
            order.Property(o => o.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<StockAdjustment>(adjustment =>
        {
            adjustment.Property(a => a.Delta).HasPrecision(18, 3);
            adjustment.Property(a => a.Reason).HasMaxLength(500);
            adjustment.Property(a => a.AdjustedBy).HasMaxLength(100);
        });
    }
}
