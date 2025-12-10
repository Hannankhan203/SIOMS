using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIOMS.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
[StringLength(100)]
public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}