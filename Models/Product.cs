using System;
using System.Collections.Generic;   
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

            [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }  // This must exist
    
    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; set; }  // This must exist
        
        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyingPrice { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }
        
        // REMOVE OR COMMENT OUT CurrentStock since we're using StockQuantity
        // public int CurrentStock { get; set; }
        
        public int MinimumStockLevel { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        
        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
        public virtual ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
        
        // Event for low stock - UPDATED to use StockQuantity
        public event EventHandler<LowStockEventArgs>? LowStockEvent;
        
        public void CheckLowStock()
        {
            if (StockQuantity <= MinimumStockLevel)  // Use StockQuantity
            {
                LowStockEvent?.Invoke(this, new LowStockEventArgs
                {
                    ProductId = ProductId,
                    ProductName = Name,
                    MinimumStock = MinimumStockLevel,
                    EventDate = DateTime.Now
                });
            }
        }
    }
    
    public class LowStockEventArgs : EventArgs
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        // public int CurrentStock { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public DateTime EventDate { get; set; }
    }
}