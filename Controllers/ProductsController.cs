#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIOMS.Data;
using SIOMS.Models;
using SIOMS.Services;
using SIOMS.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SIOMS.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, IInventoryService inventoryService, ILogger<ProductsController> logger)
        {
            _context = context;
            _inventoryService = inventoryService;
            _logger = logger;
        }

       // GET: Products
public async Task<IActionResult> Index()
{
    var products = await _inventoryService.GetAllProductsAsync();
    
    // Add this line to populate ViewBag.Categories
    ViewBag.Categories = await _context.Categories.ToListAsync();
    
    return View(products);
}

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _inventoryService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            // Get stock movements for this product
            var movements = await _inventoryService.GetStockMovementsAsync(id.Value);
            ViewBag.StockMovements = movements;

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ProductViewModel
            {
                Categories = await _context.Categories.ToListAsync(),
                Suppliers = await _context.Suppliers.ToListAsync()
            };
            return View(viewModel);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _inventoryService.AddProductAsync(model);
                _logger.LogInformation($"Product created: {model.Name}");
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown data if validation fails
            model.Categories = await _context.Categories.ToListAsync();
            model.Suppliers = await _context.Suppliers.ToListAsync();
            return View(model);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return NotFound();
                }

                var viewModel = new ProductViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description ?? string.Empty,
                    SKU = product.SKU ?? string.Empty,
                    CategoryId = product.CategoryId,
                    SupplierId = product.SupplierId,
                    BuyingPrice = product.BuyingPrice,
                    SellingPrice = product.SellingPrice,
                    CurrentStock = product.StockQuantity,
                    MinimumStockLevel = product.MinimumStockLevel,
                    Categories = await _context.Categories.ToListAsync(),
                    Suppliers = await _context.Suppliers.ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error loading product for edit: {ProductId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            // Reload dropdown data for validation errors
            model.Categories = await _context.Categories.ToListAsync();
            model.Suppliers = await _context.Suppliers.ToListAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    await _inventoryService.UpdateProductAsync(model);

                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = model.ProductId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExists(model.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _inventoryService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _inventoryService.DeleteProductAsync(id);
            _logger.LogInformation($"Product deleted: ID {id}");
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/UpdateStock/5
        public async Task<IActionResult> UpdateStock(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _inventoryService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            // Make sure to pass product to the view
            ViewBag.Product = product;

            // Initialize the model with default values
            var model = new SIOMS.ViewModels.StockUpdateModel
            {
                Quantity = 1,
                MovementType = "Adjustment"
            };

            return View(model);
        }

        // POST: Products/UpdateStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int id,
        [Bind("Quantity,MovementType,Notes")] SIOMS.ViewModels.StockUpdateModel model)
        {
            var product = await _inventoryService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _inventoryService.UpdateStockAsync(
                        id,
                        model.Quantity,
                        model.MovementType,
                        "Manual Adjustment",
                        model.Notes ?? string.Empty,
                        User.Identity?.Name ?? "System");

                    TempData["SuccessMessage"] = $"Stock updated successfully! {model.Quantity} units {GetMovementVerb(model.MovementType)}.";

                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating stock: {ex.Message}");
                }
            }

            // If we got here, something went wrong
            ViewBag.Product = product;
            return View(model);
        }

        private string GetMovementVerb(string movementType)
        {
            return movementType switch
            {
                "Purchase" or "Adjustment-In" or "Return" => "added to stock",
                "Sale" or "Adjustment-Out" or "Damaged" or "Expired" => "removed from stock",
                _ => "adjusted"
            };
        }

        // GET: Products/StockHistory/5
        public async Task<IActionResult> StockHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _inventoryService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            // Get all stock movements for this product
            var movements = await _inventoryService.GetStockMovementsAsync(id.Value);

            ViewBag.Product = product;
            return View(movements);
        }

        // POST: Products/UpdateMinimumStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMinimumStock(int id, [FromForm] int minimumStockLevel)
        {
            var product = await _inventoryService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Update minimum stock level
            product.MinimumStockLevel = minimumStockLevel;
            product.UpdatedDate = DateTime.Now;

            _context.Update(product);
            await _context.SaveChangesAsync();

            // Check if now low stock
            product.CheckLowStock();

            TempData["SuccessMessage"] = "Minimum stock level updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _context.Products.AnyAsync(e => e.ProductId == id);
        }
    }
}