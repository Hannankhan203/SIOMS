using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reports/Sales
        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;

            var sales = await _context.SalesOrders
                .Include(s => s.Product)
                .Where(s => s.OrderDate >= startDate && s.OrderDate <= endDate)
                .OrderByDescending(s => s.OrderDate)
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalSales = sales.Sum(s => s.TotalAmount);
            ViewBag.TotalOrders = sales.Count;

            return View(sales);
        }

        // GET: Reports/Inventory
        public async Task<IActionResult> Inventory()
        {
            var inventory = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            ViewBag.LowStockCount = inventory.Count(p => p.StockQuantity <= p.ReorderLevel);
            ViewBag.TotalValue = inventory.Sum(p => p.StockQuantity * p.BuyingPrice);

            return View(inventory);
        }

        // GET: Reports/ProfitLoss
        public async Task<IActionResult> ProfitLoss(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;

            var sales = await _context.SalesOrders
                .Include(s => s.Product)
                .Where(s => s.Status == "Completed" && s.OrderDate >= startDate && s.OrderDate <= endDate)
                .ToListAsync();

            var purchases = await _context.PurchaseOrders
                .Include(p => p.Product)
                .Where(p => p.OrderDate >= startDate && p.OrderDate <= endDate)
                .ToListAsync();

            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalCost = sales.Sum(s => s.Quantity * (s.Product?.BuyingPrice ?? 0));
            var totalPurchases = purchases.Sum(p => p.TotalAmount);

            var profitLoss = totalRevenue - totalCost;

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalCost = totalCost;
            ViewBag.TotalPurchases = totalPurchases;
            ViewBag.ProfitLoss = profitLoss;
            ViewBag.ProfitMargin = totalRevenue > 0 ? (profitLoss / totalRevenue * 100) : 0;

            return View(sales);
        }
    }
}