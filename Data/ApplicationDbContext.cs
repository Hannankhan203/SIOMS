using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIOMS.Models;

namespace SIOMS.Data
{
    // CHANGE: Inherit from IdentityDbContext<ApplicationUser> not DbContext
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for Users is automatically provided by IdentityDbContext<ApplicationUser>
        // public DbSet<ApplicationUser> Users { get; set; } // ← This is inherited
        
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<LowStockAlert> LowStockAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                
                // Navigation property
                entity.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Product - FIXED WITH IsActive
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                
                // String properties
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(e => e.Description)
                    .HasMaxLength(1000);
                    
                entity.Property(e => e.SKU)
                    .IsRequired()
                    .HasMaxLength(50);
                
                // Integer properties
                entity.Property(p => p.StockQuantity)
                    .IsRequired()
                    .HasDefaultValue(0);
                    
                entity.Property(p => p.MinimumStockLevel)
                    .IsRequired()
                    .HasDefaultValue(0);
                    
                entity.Property(p => p.ReorderLevel)
                    .IsRequired()
                    .HasDefaultValue(0);
                
                // Boolean property - ADDED THIS
                entity.Property(p => p.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                
                // Decimal properties
                entity.Property(p => p.BuyingPrice)
                    .IsRequired()
                    .HasPrecision(18, 2)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.SellingPrice)
                    .IsRequired()
                    .HasPrecision(18, 2)
                    .HasColumnType("decimal(18,2)");
                
                // DateTime properties
                entity.Property(p => p.CreatedDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");
                    
                entity.Property(p => p.UpdatedDate);
                
                // Foreign keys - ALL CHANGED TO DeleteBehavior.NoAction
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction); // ← Changed from Restrict to NoAction
                    
                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.NoAction); // ← Changed from SetNull to NoAction
            });

            // Configure Supplier
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.SupplierId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
            });

            // Configure Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
            });

            // Configure PurchaseOrder - FIXED
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(e => e.PurchaseOrderId);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                
                // Relationships - ALL CHANGED TO NoAction
                entity.HasOne(po => po.Supplier)
                    .WithMany()
                    .HasForeignKey(po => po.SupplierId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(po => po.Product)
                    .WithMany()
                    .HasForeignKey(po => po.ProductId)
                    .OnDelete(DeleteBehavior.NoAction); // ← ADD THIS
            });

            // Configure PurchaseOrderItem
            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(p => p.TotalPrice)
                .HasPrecision(18, 2);

            // Configure SalesOrder - FIXED
            modelBuilder.Entity<SalesOrder>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);
                
            // Relationships - ALL CHANGED TO NoAction
            modelBuilder.Entity<SalesOrder>()
                .HasOne(so => so.Customer)
                .WithMany()
                .HasForeignKey(so => so.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
                
            modelBuilder.Entity<SalesOrder>()
                .HasOne(so => so.Product)
                .WithMany()
                .HasForeignKey(so => so.ProductId)
                .OnDelete(DeleteBehavior.NoAction); // ← ADD THIS

            // Configure SalesOrderItem
            modelBuilder.Entity<SalesOrderItem>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesOrderItem>()
                .Property(p => p.TotalPrice)
                .HasPrecision(18, 2);

            // Configure StockMovement - FIXED
            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.HasKey(e => e.StockMovementId);
                entity.Property(sm => sm.MovementType).IsRequired().HasMaxLength(50);
                entity.Property(sm => sm.ReferenceNumber).HasMaxLength(100);
                entity.Property(sm => sm.Notes).HasMaxLength(200);
                entity.Property(sm => sm.SourceLocation).HasMaxLength(100);
                entity.Property(sm => sm.DestinationLocation).HasMaxLength(100);
                entity.Property(p => p.UnitPrice)
                    .HasPrecision(18, 2)
                    .HasColumnType("decimal(18,2)");
                    
                // Relationships
                entity.HasOne(sm => sm.Product)
                    .WithMany(p => p.StockMovements)
                    .HasForeignKey(sm => sm.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(sm => sm.User) // ← ADD THIS if you have User navigation property
                    .WithMany()
                    .HasForeignKey(sm => sm.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure LowStockAlert
            modelBuilder.Entity<LowStockAlert>(entity =>
            {
                entity.HasKey(e => e.AlertId);
                entity.Property(a => a.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Notes).HasMaxLength(500);
                
                // Relationships
                entity.HasOne(lsa => lsa.Product)
                    .WithMany()
                    .HasForeignKey(lsa => lsa.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}