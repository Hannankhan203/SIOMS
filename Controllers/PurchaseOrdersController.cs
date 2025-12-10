#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; // ADD THIS
using SIOMS.Data;
using SIOMS.Models;
using SIOMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SIOMS.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseOrdersController> _logger;

        public PurchaseOrdersController(ApplicationDbContext context, ILogger<PurchaseOrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PurchaseOrders
        public async Task<IActionResult> Index()
        {
            // Check if PurchaseOrders table exists
            try
            {
                var purchaseOrders = await _context.PurchaseOrders
                    .Include(po => po.Product)
                    .Include(po => po.Supplier)
                    .OrderByDescending(po => po.OrderDate)
                    .ToListAsync();

                return View(purchaseOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading purchase orders");
                TempData["ErrorMessage"] = "Purchase orders table not found. Please run migrations.";
                return View(new List<PurchaseOrder>());
            }
        }

        // GET: PurchaseOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Product)
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(m => m.PurchaseOrderId == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // GET: PurchaseOrders/Create
        public async Task<IActionResult> Create(int? productId)
        {
            try
            {
                var viewModel = new PurchaseOrderViewModel
                {
                    OrderDate = DateTime.Now,
                    ExpectedDeliveryDate = DateTime.Now.AddDays(7)
                };

                // Load products and suppliers for dropdowns
                viewModel.Products = await _context.Products
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProductId.ToString(),
                        Text = $"{p.Name} ({p.SKU}) - Stock: {p.StockQuantity}"
                    })
                    .ToListAsync();

                viewModel.Suppliers = await _context.Suppliers
                    .Select(s => new SelectListItem
                    {
                        Value = s.SupplierId.ToString(),
                        Text = s.Name
                    })
                    .ToListAsync();

                // If productId is provided, pre-select it
                if (productId.HasValue)
                {
                    viewModel.ProductId = productId.Value;
                    var product = await _context.Products.FindAsync(productId.Value);
                    if (product != null)
                    {
                        viewModel.UnitPrice = product.BuyingPrice;
                        viewModel.ProductName = product.Name;
                        viewModel.ProductSKU = product.SKU;
                        viewModel.CurrentBuyingPrice = product.BuyingPrice;
                        viewModel.CurrentStock = product.StockQuantity;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create view");
                TempData["ErrorMessage"] = "Error loading data. Please check if database tables exist.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Json(new
            {
                name = product.Name,
                sku = product.SKU,
                currentStock = product.StockQuantity,
                buyingPrice = product.BuyingPrice
            });
        }

        // POST: PurchaseOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Calculate TotalAmount
                model.TotalAmount = model.Quantity * model.UnitPrice;

                var purchaseOrder = new PurchaseOrder
                {
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    UnitPrice = model.UnitPrice,
                    TotalAmount = model.TotalAmount, // Add this line
                    SupplierId = model.SupplierId,
                    OrderDate = model.OrderDate,
                    ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                    Notes = model.Notes,
                    Status = "Pending",
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System"
                };

                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Purchase order created: ID {purchaseOrder.PurchaseOrderId}, Product: {model.ProductId}, Quantity: {model.Quantity}");

                TempData["SuccessMessage"] = "Purchase order created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            try
            {
                model.Products = await _context.Products
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProductId.ToString(),
                        Text = $"{p.Name} ({p.SKU})"
                    })
                    .ToListAsync();

                model.Suppliers = await _context.Suppliers
                    .Select(s => new SelectListItem
                    {
                        Value = s.SupplierId.ToString(),
                        Text = s.Name
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading dropdowns");
            }

            return View(model);
        }

        // GET: PurchaseOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            var viewModel = new PurchaseOrderViewModel
            {
                PurchaseOrderId = purchaseOrder.PurchaseOrderId,
                ProductId = purchaseOrder.ProductId,
                Quantity = purchaseOrder.Quantity,
                UnitPrice = purchaseOrder.UnitPrice,
                SupplierId = purchaseOrder.SupplierId,
                OrderDate = purchaseOrder.OrderDate,
                ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                Notes = purchaseOrder.Notes
            };

            // Load dropdowns
            viewModel.Products = await _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} ({p.SKU})"
                })
                .ToListAsync();

            viewModel.Suppliers = await _context.Suppliers
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            return View(viewModel);
        }

        // POST: PurchaseOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseOrderViewModel model)
        {
            if (id != model.PurchaseOrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Calculate TotalAmount
                    model.TotalAmount = model.Quantity * model.UnitPrice;

                    var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
                    if (purchaseOrder == null)
                    {
                        return NotFound();
                    }

                    purchaseOrder.ProductId = model.ProductId;
                    purchaseOrder.Quantity = model.Quantity;
                    purchaseOrder.UnitPrice = model.UnitPrice;
                    purchaseOrder.TotalAmount = model.TotalAmount; // Add this line
                    purchaseOrder.SupplierId = model.SupplierId;
                    purchaseOrder.OrderDate = model.OrderDate;
                    purchaseOrder.ExpectedDeliveryDate = model.ExpectedDeliveryDate;
                    purchaseOrder.Notes = model.Notes;
                    purchaseOrder.UpdatedDate = DateTime.Now;

                    _context.Update(purchaseOrder);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Purchase order updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(id))
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
            
            // Reload dropdowns if validation fails
            model.Products = await _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} ({p.SKU})"
                })
                .ToListAsync();

            model.Suppliers = await _context.Suppliers
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            return View(model);
        }

        // GET: PurchaseOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Product)
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(m => m.PurchaseOrderId == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
                if (purchaseOrder != null)
                {
                    _context.PurchaseOrders.Remove(purchaseOrder);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Purchase order deleted: ID {id}");
                    TempData["SuccessMessage"] = "Purchase order deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase order");
                TempData["ErrorMessage"] = $"Error deleting purchase order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: PurchaseOrders/Receive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id)
        {
            try
            {
                var purchaseOrder = await _context.PurchaseOrders
                    .Include(po => po.Product)
                    .FirstOrDefaultAsync(po => po.PurchaseOrderId == id);

                if (purchaseOrder == null)
                {
                    return NotFound();
                }

                // Update purchase order status
                purchaseOrder.Status = "Received";
                purchaseOrder.ActualDeliveryDate = DateTime.Now;
                purchaseOrder.UpdatedDate = DateTime.Now;

                // Update product stock
                if (purchaseOrder.Product != null)
                {
                    purchaseOrder.Product.StockQuantity += purchaseOrder.Quantity;
                    purchaseOrder.Product.UpdatedDate = DateTime.Now;

                    _logger.LogInformation($"Stock updated via PO#{id}: +{purchaseOrder.Quantity} units for product {purchaseOrder.Product.Name}");
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Purchase order received! {purchaseOrder.Quantity} units added to stock.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving purchase order");
                TempData["ErrorMessage"] = $"Error receiving purchase order: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.PurchaseOrders.Any(e => e.PurchaseOrderId == id);
        }
    }
}