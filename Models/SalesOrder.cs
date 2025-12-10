using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class SalesOrder
    {
        [Key]
        public int SalesOrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? CustomerId { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        [Range(1, 10000)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CustomerPhone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? DeliveryDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        // Calculated properties (not stored in database)
        [NotMapped]
        public decimal Profit => UnitPrice - (Product?.BuyingPrice ?? 0);

        [NotMapped]
        public decimal ProfitPercentage => Product?.BuyingPrice > 0 ?
            ((UnitPrice - Product.BuyingPrice) / Product.BuyingPrice * 100) : 0;
    }
}