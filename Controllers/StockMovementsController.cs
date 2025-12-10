using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;  // ADD THIS
using SIOMS.Models;
using SIOMS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Controllers
{
    public class StockMovementsController : Controller
    {
        private readonly IStockMovementService _stockMovementService;
        private readonly ApplicationDbContext _context;

        public StockMovementsController(IStockMovementService stockMovementService, ApplicationDbContext context)
        {
            _stockMovementService = stockMovementService;
            _context = context;
        }

        // GET: StockMovements
        public async Task<IActionResult> Index(string search = "", string movementType = "", 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            ViewBag.SearchTerm = search;
            ViewBag.MovementType = movementType;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            var movements = await _stockMovementService.SearchMovementsAsync(search, startDate, endDate);

            if (!string.IsNullOrEmpty(movementType))
            {
                movements = movements.Where(m => m.MovementType == movementType).ToList();
            }

            // Get summary for the filter
            ViewBag.Summary = await _stockMovementService.GetMovementSummaryAsync(startDate, endDate);

            return View(movements);
        }

        // GET: StockMovements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockMovement = await _stockMovementService.GetMovementByIdAsync(id.Value);
            if (stockMovement == null)
            {
                return NotFound();
            }

            return View(stockMovement);
        }

        // GET: StockMovements/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.MovementTypes = new List<string>
            {
                "IN", "OUT", "TRANSFER", "ADJUSTMENT"
            };

            return View();
        }

        // POST: StockMovements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StockMovementId,ProductId,MovementType,Quantity,UnitPrice,ReferenceNumber,Notes,MovementDate,SourceLocation,DestinationLocation")] StockMovement stockMovement)
        {
            if (ModelState.IsValid)
            {
                await _stockMovementService.CreateMovementAsync(stockMovement);
                TempData["SuccessMessage"] = "Stock movement recorded successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.MovementTypes = new List<string>
            {
                "IN", "OUT", "TRANSFER", "ADJUSTMENT"
            };

            return View(stockMovement);
        }

        // GET: StockMovements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockMovement = await _stockMovementService.GetMovementByIdAsync(id.Value);
            if (stockMovement == null)
            {
                return NotFound();
            }

            ViewBag.Products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.MovementTypes = new List<string>
            {
                "IN", "OUT", "TRANSFER", "ADJUSTMENT"
            };

            return View(stockMovement);
        }

        // POST: StockMovements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StockMovementId,ProductId,MovementType,Quantity,UnitPrice,ReferenceNumber,Notes,MovementDate,SourceLocation,DestinationLocation")] StockMovement stockMovement)
        {
            if (id != stockMovement.StockMovementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get original movement to reverse stock
                    var originalMovement = await _stockMovementService.GetMovementByIdAsync(id);
                    if (originalMovement != null)
                    {
                        // Reverse original stock adjustment
                        var product = await _context.Products.FindAsync(originalMovement.ProductId);
                        if (product != null)
                        {
                            if (originalMovement.MovementType.ToUpper() == "IN" || 
                                originalMovement.MovementType.ToUpper() == "ADJUSTMENT" ||
                                originalMovement.MovementType.ToUpper() == "TRANSFER" && originalMovement.DestinationLocation != null)
                            {
                                product.StockQuantity -= originalMovement.Quantity;
                            }
                            else if (originalMovement.MovementType.ToUpper() == "OUT" ||
                                     originalMovement.MovementType.ToUpper() == "TRANSFER" && originalMovement.SourceLocation != null)
                            {
                                product.StockQuantity += originalMovement.Quantity;
                            }
                        }
                    }

                    // Update the movement
                    _context.Update(stockMovement);
                    await _context.SaveChangesAsync();

                    // Apply new stock adjustment
                    var updatedProduct = await _context.Products.FindAsync(stockMovement.ProductId);
                    if (updatedProduct != null)
                    {
                        if (stockMovement.MovementType.ToUpper() == "IN" || 
                            stockMovement.MovementType.ToUpper() == "ADJUSTMENT" ||
                            stockMovement.MovementType.ToUpper() == "TRANSFER" && stockMovement.DestinationLocation != null)
                        {
                            updatedProduct.StockQuantity += stockMovement.Quantity;
                        }
                        else if (stockMovement.MovementType.ToUpper() == "OUT" ||
                                 stockMovement.MovementType.ToUpper() == "TRANSFER" && stockMovement.SourceLocation != null)
                        {
                            updatedProduct.StockQuantity -= stockMovement.Quantity;
                        }

                        updatedProduct.UpdatedDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Stock movement updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await StockMovementExists(stockMovement.StockMovementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.MovementTypes = new List<string>
            {
                "IN", "OUT", "TRANSFER", "ADJUSTMENT"
            };

            return View(stockMovement);
        }

        // GET: StockMovements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockMovement = await _stockMovementService.GetMovementByIdAsync(id.Value);
            if (stockMovement == null)
            {
                return NotFound();
            }

            return View(stockMovement);
        }

        // POST: StockMovements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _stockMovementService.DeleteMovementAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Stock movement deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete stock movement.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: StockMovements/ByProduct/5
        public async Task<IActionResult> ByProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Product = product;
            var movements = await _stockMovementService.GetMovementsByProductAsync(productId);
            return View(movements);
        }

        // GET: StockMovements/Report
        public async Task<IActionResult> Report(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            var movements = await _stockMovementService.GetMovementsByDateRangeAsync(startDate.Value, endDate.Value);
            var summary = await _stockMovementService.GetMovementSummaryAsync(startDate, endDate);

            ViewBag.Summary = summary;

            return View(movements);
        }

        private async Task<bool> StockMovementExists(int id)
        {
            return await _context.StockMovements.AnyAsync(e => e.StockMovementId == id);
        }
    }
}