using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIOMS.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PurchaseOrders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Product)
                .OrderByDescending(p => p.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: PurchaseOrders/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "SupplierId", "Name");
            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "ProductId", "Name");
            return View();
        }

        // POST: PurchaseOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrder order)
        {
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                order.TotalAmount = order.Quantity * order.UnitPrice;

                _context.PurchaseOrders.Add(order);
                await _context.SaveChangesAsync();

                // Update product stock
                var product = await _context.Products.FindAsync(order.ProductId);
                if (product != null)
                {
                    product.StockQuantity += order.Quantity;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if validation fails
            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "SupplierId", "Name");
            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "ProductId", "Name");
            return View(order);
        }
    }
}