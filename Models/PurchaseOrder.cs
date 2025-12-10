using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class PurchaseOrder
    {

        public string OrderNumber { get; set; } = string.Empty;

        [Key]
        public int PurchaseOrderId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
        
        [Required]
        public int SupplierId { get; set; }
        
        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }
        
        [Required]
        [Range(1, 10000)]
        public int Quantity { get; set; }
        
        [Required]
        [Range(0.01, 1000000)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        // FIX: Make this a regular property with a backing field
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [DataType(DataType.Date)]
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? ActualDeliveryDate { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        
        [StringLength(50)]
        public string? CreatedBy { get; set; }
    }
}