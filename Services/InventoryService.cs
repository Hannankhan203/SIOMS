#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIOMS.Data;
using SIOMS.Models;
using SIOMS.ViewModels;

namespace SIOMS.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryService> _logger;
        
        // Delegate for low stock notifications
        public delegate void LowStockNotificationHandler(LowStockAlert alert);
        public event LowStockNotificationHandler? OnLowStockDetected;
        
        public InventoryService(ApplicationDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
            
        }
        
        private void SubscribeToProductEvents()
        {
            var products = _context.Products.ToList();
            foreach (var product in products)
            {
                product.LowStockEvent += HandleLowStockEvent;
            }
        }
        
        private void HandleLowStockEvent(object? sender, LowStockEventArgs e)
        {
            _logger.LogWarning($"Low stock alert for product: {e.ProductName} (ID: {e.ProductId})");
            
            // Create and save alert
            var alert = new LowStockAlert
            {
                ProductId = e.ProductId,
                ProductName = e.ProductName,
                CurrentStock = e.StockQuantity,
                MinimumStockLevel = e.MinimumStock,
                AlertDate = e.EventDate,
                IsResolved = false
            };
            
            _context.LowStockAlerts.Add(alert);
            _context.SaveChanges();
            
            // Trigger notification event
            OnLowStockDetected?.Invoke(alert);
        }
        
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();
        }
        
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }
        
        public async Task AddProductAsync(ProductViewModel model)
        {
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                SKU = model.SKU,
                CategoryId = model.CategoryId,
                SupplierId = model.SupplierId,
                BuyingPrice = model.BuyingPrice,
                SellingPrice = model.SellingPrice,
                StockQuantity = model.CurrentStock,
                MinimumStockLevel = model.MinimumStockLevel,
                CreatedDate = DateTime.Now
            };
            
            // Subscribe to low stock event
            product.LowStockEvent += HandleLowStockEvent;
            
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            
            // Check if low stock immediately
            product.CheckLowStock();
        }
        
        public async Task UpdateProductAsync(ProductViewModel model)
        {
            var product = await _context.Products.FindAsync(model.ProductId);
            if (product != null)
            {
                product.Name = model.Name;
                product.Description = model.Description;
                product.SKU = model.SKU;
                product.CategoryId = model.CategoryId;
                product.SupplierId = model.SupplierId;
                product.BuyingPrice = model.BuyingPrice;
                product.SellingPrice = model.SellingPrice;
                product.StockQuantity = model.CurrentStock;
                product.MinimumStockLevel = model.MinimumStockLevel;
                product.UpdatedDate = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                // Check for low stock after update
                product.CheckLowStock();
            }
        }
        
        public async Task UpdateStockAsync(int productId, int quantity, string movementType, 
                                         string reference, string notes, string user)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                // Update stock
                if (movementType == "Purchase" || movementType == "Adjustment-In")
                    product.StockQuantity += quantity;
                else if (movementType == "Sale" || movementType == "Adjustment-Out")
                    product.StockQuantity -= quantity;
                
                // Record movement
                var movement = new StockMovement
                {
                    ProductId = productId,
                    MovementType = movementType,
                    Quantity = quantity,
                    UnitPrice = product.SellingPrice,
                    ReferenceNumber = reference,
                    Notes = notes,
                    MovementDate = DateTime.Now,
                    UserId = user
                };
                
                _context.StockMovements.Add(movement);
                await _context.SaveChangesAsync();
                
                // Check for low stock
                product.CheckLowStock();
            }
        }
        
        public async Task<List<LowStockAlert>> GetActiveLowStockAlertsAsync()
        {
            return await _context.LowStockAlerts
                .Where(a => !a.IsResolved)
                .Include(a => a.Product)
                .OrderByDescending(a => a.AlertDate)
                .ToListAsync();
        }
        
        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var model = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                LowStockItems = await _context.LowStockAlerts.CountAsync(a => !a.IsResolved),
                ActiveAlerts = await GetActiveLowStockAlertsAsync()
            };
            
            // Monthly sales (current month)
            var today = DateTime.Today;
            model.MonthlySales = await GetMonthlySalesAsync(today.Month, today.Year);
            
            // Top selling products (using LINQ)
            model.TopSellingProducts = await GetTopSellingProductsAsync(5);
            
            // Monthly sales chart data (last 6 months)
            model.MonthlySalesChart = new List<KeyValuePair<string, decimal>>();
            for (int i = 5; i >= 0; i--)
            {
                var date = today.AddMonths(-i);
                var sales = await GetMonthlySalesAsync(date.Month, date.Year);
                model.MonthlySalesChart.Add(new KeyValuePair<string, decimal>(
                    date.ToString("MMM yyyy"), sales));
            }
            
            return model;
        }
        
        public async Task<List<Product>> GetTopSellingProductsAsync(int count = 10)
        {
            // Using LINQ to query top selling products
            return await _context.Products
                .Where(p => p.SalesOrderItems.Any())
                .OrderByDescending(p => p.SalesOrderItems.Sum(i => i.Quantity))
                .Take(count)
                .Include(p => p.Category)
                .ToListAsync();
        }
        
        public async Task<decimal> GetMonthlySalesAsync(int month, int year)
        {
            return await _context.SalesOrders
                .Where(so => so.OrderDate.Month == month && 
                           so.OrderDate.Year == year && 
                           so.Status == "Completed")
                .SumAsync(so => so.TotalAmount);
        }
        
        public async Task DailyStockReconciliationAsync()
        {
            _logger.LogInformation("Starting daily stock reconciliation...");
            
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= p.MinimumStockLevel)
                .ToListAsync();
            
            foreach (var product in lowStockProducts)
            {
                _logger.LogWarning($"Product {product.Name} is low on stock: {product.StockQuantity}/{product.MinimumStockLevel}");
                product.CheckLowStock();
            }
            
            _logger.LogInformation($"Daily reconciliation completed. Found {lowStockProducts.Count} low stock items.");
        }
        
        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<List<StockMovement>> GetStockMovementsAsync(int? productId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.StockMovements.AsQueryable();
            
            if (productId.HasValue)
            {
                query = query.Where(m => m.ProductId == productId.Value);
            }
            
            if (fromDate.HasValue)
            {
                query = query.Where(m => m.MovementDate >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                query = query.Where(m => m.MovementDate <= toDate.Value);
            }
            
            return await query
                .Include(m => m.Product)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();
        }
        
        public async Task ResolveLowStockAlertAsync(int alertId)
        {
            var alert = await _context.LowStockAlerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.IsResolved = true;
                alert.ResolvedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}