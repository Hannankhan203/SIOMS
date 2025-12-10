using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SIOMS.Data;
using Microsoft.AspNetCore.Identity;
using SIOMS.Models;

namespace SIOMS.Models
{
    public class StockMovement
    {
        [Key]
        public int StockMovementId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string MovementType { get; set; } = string.Empty; // "IN", "OUT", "TRANSFER", "ADJUSTMENT"

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; } // PO Number, SO Number, etc.

        [StringLength(200)]
        public string? Notes { get; set; }

        [Required]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        // Fix: Choose ONE of these approaches:

        // APPROACH 1: Standard foreign key with navigation property (RECOMMENDED)
        [ForeignKey("User")] // This links to the navigation property below
        public string? UserId { get; set; } // This should be string (if using ASP.NET Identity)

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; } = null!;

        // OR APPROACH 2: If you want to keep CreatedByUserId as the foreign key
        // [ForeignKey("User")]
        // public string CreatedByUserId { get; set; }
        // 
        // [ForeignKey("CreatedByUserId")]
        // public virtual ApplicationUser User { get; set; } = null!;

        [StringLength(100)]
        public string? SourceLocation { get; set; }

        [StringLength(100)]
        public string? DestinationLocation { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }

    public enum StockMovementType
    {
        In,        // Stock coming in (Purchase, Return)
        Out,       // Stock going out (Sale, Damage)
        Transfer,  // Transfer between locations
        Adjustment // Manual adjustment
    }
}