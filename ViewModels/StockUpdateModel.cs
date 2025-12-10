// ViewModels/StockUpdateModel.cs
using System.ComponentModel.DataAnnotations;

namespace SIOMS.ViewModels
{
    public class StockUpdateModel
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10000, ErrorMessage = "Quantity must be between 1 and 10000")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "Movement type is required")]
        public string MovementType { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}