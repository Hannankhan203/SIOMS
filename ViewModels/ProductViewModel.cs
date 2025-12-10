using SIOMS.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIOMS.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        
        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }
        
        [Required]
        [Range(0.01, 1000000)]
        [Display(Name = "Buying Price")]
        public decimal BuyingPrice { get; set; }
        
        [Required]
        [Range(0.01, 1000000)]
        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }
        
        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }
        
        [Required]
        [Range(0, 1000000)]
        [Display(Name = "Minimum Stock Level")]
        public int MinimumStockLevel { get; set; }
        
        // For dropdown lists
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}