using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIOMS.ViewModels
{
    public class SalesOrderViewModel
    {
        public int SalesOrderId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Unit price must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Selling Price")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
        [Display(Name = "Phone Number")]
        public string? CustomerPhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string? CustomerEmail { get; set; }

        public int? CustomerId { get; set; }

        public List<SelectListItem>? Customers { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        [Display(Name = "Delivery Date")]
        public DateTime? DeliveryDate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        // For dropdown
        public List<SelectListItem>? Products { get; set; }

        // Product details for display
        public string? ProductName { get; set; }
        public string? ProductSKU { get; set; }
        public decimal CurrentSellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public decimal BuyingPrice { get; set; }

        // Calculated properties
        [Display(Name = "Estimated Profit")]
        [DataType(DataType.Currency)]
        public decimal EstimatedProfit => UnitPrice - BuyingPrice;

        [Display(Name = "Profit Margin")]
        public decimal ProfitPercentage => BuyingPrice > 0 ?
            ((UnitPrice - BuyingPrice) / BuyingPrice * 100) : 0;
    }
}