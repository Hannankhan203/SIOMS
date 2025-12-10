using SIOMS.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIOMS.Services
{
    public interface IStockMovementService
    {
        Task<IEnumerable<StockMovement>> GetAllMovementsAsync();
        Task<StockMovement?> GetMovementByIdAsync(int id);
        Task<IEnumerable<StockMovement>> GetMovementsByProductAsync(int productId);
        Task<IEnumerable<StockMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<StockMovement>> SearchMovementsAsync(string searchTerm, DateTime? startDate = null, DateTime? endDate = null);
        Task<StockMovement> CreateMovementAsync(StockMovement movement, bool updateProductStock = true);
        Task<bool> DeleteMovementAsync(int id);
        Task<StockMovementSummary> GetMovementSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ProductMovementSummary>> GetProductMovementSummaryAsync(int productId);
    }

    public class StockMovementSummary
    {
        public int TotalMovements { get; set; }
        public int TotalInQuantity { get; set; }
        public int TotalOutQuantity { get; set; }
        public decimal TotalInValue { get; set; }
        public decimal TotalOutValue { get; set; }
        public Dictionary<string, int> MovementsByType { get; set; } = new();
    }

    public class ProductMovementSummary
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalIn { get; set; }
        public int TotalOut { get; set; }
        public int NetChange { get; set; }
        public decimal TotalValue { get; set; }
    }
}