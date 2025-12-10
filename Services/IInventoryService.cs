using SIOMS.Models;
using SIOMS.ViewModels;

namespace SIOMS.Services
{
    public interface IInventoryService
    {
        // Product Management
        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task AddProductAsync(ProductViewModel model);
        Task UpdateProductAsync(ProductViewModel model);
        Task DeleteProductAsync(int id);
        
        // Stock Management
        Task UpdateStockAsync(int productId, int quantity, string movementType, string reference, string notes, string user);
        Task<List<StockMovement>> GetStockMovementsAsync(int? productId = null, DateTime? fromDate = null, DateTime? toDate = null);
        
        // Low Stock Alerts
        Task<List<LowStockAlert>> GetActiveLowStockAlertsAsync();
        Task ResolveLowStockAlertAsync(int alertId);
        
        // Dashboard Data
        Task<DashboardViewModel> GetDashboardDataAsync();
        
        // Reports
        Task<List<Product>> GetTopSellingProductsAsync(int count = 10);
        Task<decimal> GetMonthlySalesAsync(int month, int year);

        // Background task
        Task DailyStockReconciliationAsync();
    }
}