using SIOMS.Models;

namespace SIOMS.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalCustomers { get; set; }
        public int LowStockItems { get; set; }
        public decimal MonthlySales { get; set; }
        public List<Product> TopSellingProducts { get; set; } = new List<Product>();
        public List<LowStockAlert> ActiveAlerts { get; set; } = new List<LowStockAlert>();
        public List<KeyValuePair<string, decimal>> MonthlySalesChart { get; set; } = new List<KeyValuePair<string, decimal>>();
    }
}