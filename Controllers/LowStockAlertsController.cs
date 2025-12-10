using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Controllers
{
    public class LowStockAlertsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LowStockAlertsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LowStockAlerts
        public async Task<IActionResult> Index()
        {
            var alerts = await _context.LowStockAlerts
                .Include(a => a.Product)
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.AlertDate)
                .ToListAsync();

            return View(alerts);
        }

        // GET: LowStockAlerts/GenerateAlerts
        public async Task<IActionResult> GenerateAlerts()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= p.MinimumStockLevel &&
                           p.StockQuantity > 0 && p.MinimumStockLevel > 0)
                .ToListAsync();

            foreach (var product in lowStockProducts)
            {
                var existingAlert = await _context.LowStockAlerts
                    .AnyAsync(a => a.ProductId == product.ProductId && !a.IsResolved);

                if (!existingAlert)
                {
                    var alert = new LowStockAlert
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        CurrentStock = product.StockQuantity,
                        MinimumStockLevel = product.MinimumStockLevel,
                        AlertDate = DateTime.Now
                    };
                    _context.LowStockAlerts.Add(alert);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateTestAlert()
        {
            // Create alert for Wireless Mouse (ProductId 1)
            var alert = new LowStockAlert
            {
                ProductId = 1, // Change to your mouse's ProductId
                ProductName = "Wireless Mouse",
                CurrentStock = 5,
                MinimumStockLevel = 10,
                AlertDate = DateTime.Now,
                IsResolved = false
            };

            _context.LowStockAlerts.Add(alert);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: LowStockAlerts/All
        public async Task<IActionResult> All()
        {
            var alerts = await _context.LowStockAlerts
                .Include(a => a.Product)
                .OrderByDescending(a => a.AlertDate)
                .ToListAsync();

            return View(alerts);
        }

        public async Task<IActionResult> Debug()
        {
            var products = await _context.Products.ToListAsync();
            var results = products.Select(p => new
            {
                p.Name,
                p.StockQuantity,
                p.MinimumStockLevel,
                IsLowStock = p.StockQuantity <= p.MinimumStockLevel
            });

            return Json(results);
        }



        // POST: LowStockAlerts/MarkAsResolved/5
        [HttpPost]
        public async Task<IActionResult> MarkAsResolved(int id)
        {
            var alert = await _context.LowStockAlerts.FindAsync(id);
            if (alert != null)
            {
                alert.IsResolved = true;
                alert.ResolvedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Alert marked as resolved!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}