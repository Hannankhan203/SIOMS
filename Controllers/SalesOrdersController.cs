#nullable enable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIOMS.Data;
using SIOMS.Models;
using SIOMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SIOMS.Controllers
{
    public class SalesOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalesOrdersController> _logger;

        public SalesOrdersController(ApplicationDbContext context, ILogger<SalesOrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: SalesOrders
        public async Task<IActionResult> Index()
        {
            var salesOrders = await _context.SalesOrders
                .Include(so => so.Product)
                .OrderByDescending(so => so.OrderDate)
                .ToListAsync();
            
            return View(salesOrders);
        }

        // GET: SalesOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders
                .Include(so => so.Product)
                .FirstOrDefaultAsync(m => m.SalesOrderId == id);
                
            if (salesOrder == null)
            {
                return NotFound();
            }

            return View(salesOrder);
        }

        // GET: SalesOrders/Create
        public async Task<IActionResult> Create(int? productId)
        {
            var viewModel = new SalesOrderViewModel
            {
                OrderDate = DateTime.Now,
                DeliveryDate = DateTime.Now.AddDays(3)
            };

            // Load products for dropdown
            viewModel.Products = await _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} ({p.SKU}) - Stock: {p.StockQuantity}, Price: {p.SellingPrice:C}"
                })
                .ToListAsync();

            // If productId is provided, pre-select it
            if (productId.HasValue)
            {
                viewModel.ProductId = productId.Value;
                var product = await _context.Products.FindAsync(productId.Value);
                if (product != null)
                {
                    viewModel.UnitPrice = product.SellingPrice;
                    viewModel.ProductName = product.Name;
                    viewModel.ProductSKU = product.SKU;
                    viewModel.CurrentSellingPrice = product.SellingPrice;
                    viewModel.CurrentStock = product.StockQuantity;
                    viewModel.BuyingPrice = product.BuyingPrice;
                }
            }

            return View(viewModel);
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
                sellingPrice = product.SellingPrice,
                buyingPrice = product.BuyingPrice
            });
        }

        // POST: SalesOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Calculate total amount
                    model.TotalAmount = model.Quantity * model.UnitPrice;
                    
                    // Check if enough stock is available
                    var product = await _context.Products.FindAsync(model.ProductId);
                    if (product == null)
                    {
                        ModelState.AddModelError("ProductId", "Product not found");
                        return View(model);
                    }
                    
                    if (product.StockQuantity < model.Quantity)
                    {
                        ModelState.AddModelError("Quantity", $"Insufficient stock. Available: {product.StockQuantity} units");
                        model.Products = await _context.Products
                            .Select(p => new SelectListItem
                            {
                                Value = p.ProductId.ToString(),
                                Text = $"{p.Name} ({p.SKU})"
                            })
                            .ToListAsync();
                        return View(model);
                    }

                    var salesOrder = new SalesOrder
                    {
                        ProductId = model.ProductId,
                        Quantity = model.Quantity,
                        UnitPrice = model.UnitPrice,
                        TotalAmount = model.TotalAmount,
                        CustomerName = model.CustomerName,
                        CustomerPhone = model.CustomerPhone,
                        CustomerEmail = model.CustomerEmail,
                        OrderDate = model.OrderDate,
                        DeliveryDate = model.DeliveryDate,
                        Notes = model.Notes,
                        Status = "Pending",
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };

                    _context.Add(salesOrder);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Sales order created: ID {salesOrder.SalesOrderId}, Product: {model.ProductId}, Quantity: {model.Quantity}");
                    
                    TempData["SuccessMessage"] = "Sales order created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating sales order");
                    ModelState.AddModelError("", $"Error creating sales order: {ex.Message}");
                }
            }

            // Reload dropdown if validation fails
            model.Products = await _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} ({p.SKU})"
                })
                .ToListAsync();

            return View(model);
        }

        // GET: SalesOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders.FindAsync(id);
            if (salesOrder == null)
            {
                return NotFound();
            }

            var viewModel = new SalesOrderViewModel
            {
                SalesOrderId = salesOrder.SalesOrderId,
                ProductId = salesOrder.ProductId,
                Quantity = salesOrder.Quantity,
                UnitPrice = salesOrder.UnitPrice,
                TotalAmount = salesOrder.TotalAmount,
                CustomerName = salesOrder.CustomerName,
                CustomerPhone = salesOrder.CustomerPhone,
                CustomerEmail = salesOrder.CustomerEmail,
                OrderDate = salesOrder.OrderDate,
                DeliveryDate = salesOrder.DeliveryDate,
                Notes = salesOrder.Notes
            };

            // Load dropdown
            viewModel.Products = await _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} ({p.SKU})"
                })
                .ToListAsync();

            return View(viewModel);
        }

        // POST: SalesOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalesOrderViewModel model)
        {
            if (id != model.SalesOrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Calculate total amount
                    model.TotalAmount = model.Quantity * model.UnitPrice;
                    
                    var salesOrder = await _context.SalesOrders.FindAsync(id);
                    if (salesOrder == null)
                    {
                        return NotFound();
                    }

                    salesOrder.ProductId = model.ProductId;
                    salesOrder.Quantity = model.Quantity;
                    salesOrder.UnitPrice = model.UnitPrice;
                    salesOrder.TotalAmount = model.TotalAmount;
                    salesOrder.CustomerName = model.CustomerName;
                    salesOrder.CustomerPhone = model.CustomerPhone;
                    salesOrder.CustomerEmail = model.CustomerEmail;
                    salesOrder.OrderDate = model.OrderDate;
                    salesOrder.DeliveryDate = model.DeliveryDate;
                    salesOrder.Notes = model.Notes;
                    salesOrder.UpdatedDate = DateTime.Now;

                    _context.Update(salesOrder);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Sales order updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesOrderExists(id))
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

            // Reload dropdown if validation fails
            model.Products = await _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} ({p.SKU})"
                })
                .ToListAsync();

            return View(model);
        }

        // GET: SalesOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrder = await _context.SalesOrders
                .Include(so => so.Product)
                .FirstOrDefaultAsync(m => m.SalesOrderId == id);
                
            if (salesOrder == null)
            {
                return NotFound();
            }

            return View(salesOrder);
        }

        // POST: SalesOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var salesOrder = await _context.SalesOrders.FindAsync(id);
                if (salesOrder != null)
                {
                    _context.SalesOrders.Remove(salesOrder);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Sales order deleted: ID {id}");
                    TempData["SuccessMessage"] = "Sales order deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sales order");
                TempData["ErrorMessage"] = $"Error deleting sales order: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: SalesOrders/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var salesOrder = await _context.SalesOrders
                    .Include(so => so.Product)
                    .FirstOrDefaultAsync(so => so.SalesOrderId == id);
                    
                if (salesOrder == null)
                {
                    return NotFound();
                }

                // Update sales order status
                salesOrder.Status = "Completed";
                salesOrder.UpdatedDate = DateTime.Now;

                // Update product stock (reduce stock)
                if (salesOrder.Product != null)
                {
                    if (salesOrder.Product.StockQuantity < salesOrder.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Cannot complete order. Insufficient stock. Available: {salesOrder.Product.StockQuantity} units";
                        return RedirectToAction(nameof(Details), new { id });
                    }
                    
                    salesOrder.Product.StockQuantity -= salesOrder.Quantity;
                    salesOrder.Product.UpdatedDate = DateTime.Now;
                    
                    _logger.LogInformation($"Stock updated via SO#{id}: -{salesOrder.Quantity} units for product {salesOrder.Product.Name}");
                }

                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Sales order completed! {salesOrder.Quantity} units sold.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing sales order");
                TempData["ErrorMessage"] = $"Error completing sales order: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.SalesOrderId == id);
        }
    }
}