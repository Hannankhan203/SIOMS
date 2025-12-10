#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class SalesOrderItem
    {
        [Key]
        public int SalesOrderItemId { get; set; }
        
        [ForeignKey("SalesOrder")]
        public int SalesOrderId { get; set; }
        
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        // Navigation properties
        public virtual SalesOrder SalesOrder { get; set; }
        public virtual Product Product { get; set; }
    }
}