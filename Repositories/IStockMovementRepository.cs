using SIOMS.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIOMS.Repositories
{
    public interface IStockMovementRepository
    {
        Task<IEnumerable<StockMovement>> GetAllAsync();
        Task<StockMovement?> GetByIdAsync(int id);
        Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId);
        Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(string movementType);
        Task<StockMovement> CreateAsync(StockMovement movement);
        Task<StockMovement> UpdateAsync(StockMovement movement);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<decimal> GetTotalValueMovedAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalQuantityMovedAsync(int productId, string movementType, DateTime? startDate = null, DateTime? endDate = null);
    }
}