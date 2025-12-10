using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class LowStockAlert
    {
        [Key]
        public int AlertId { get; set; }
        
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;
        
        public int CurrentStock { get; set; }
        
        public int MinimumStockLevel { get; set; }
        
        public DateTime AlertDate { get; set; } = DateTime.Now;
        
        public bool IsResolved { get; set; } = false;
        
        public DateTime? ResolvedDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Navigation property
        public virtual Product Product { get; set; } = null!;
    }
}