using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIOMS.ViewModels
{
    public class PurchaseOrderViewModel
    {
        public int PurchaseOrderId { get; set; }
        
        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000")]
        public int Quantity { get; set; } = 1;
        
        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Unit price must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }
        
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; } // Remove the computed property
        
        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }
        
        [DataType(DataType.Date)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [DataType(DataType.Date)]
        [Display(Name = "Expected Delivery")]
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public List<SelectListItem>? Products { get; set; }
        public List<SelectListItem>? Suppliers { get; set; }
        
        public string? ProductName { get; set; }
        public string? ProductSKU { get; set; }
        public decimal CurrentBuyingPrice { get; set; }
        public int CurrentStock { get; set; }
    }
}