using System.ComponentModel.DataAnnotations;

namespace SIOMS.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        // Statistics
        [Display(Name = "Product Count")]
        public int ProductCount { get; set; }
        
        [Display(Name = "Total Stock")]
        public int TotalStock { get; set; }
        
        [Display(Name = "Stock Value")]
        [DataType(DataType.Currency)]
        public decimal StockValue { get; set; }
    }
}