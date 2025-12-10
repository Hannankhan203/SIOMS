using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Repositories
{
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly ApplicationDbContext _context;

        public StockMovementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StockMovement>> GetAllAsync()
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User) // Added to include User
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<StockMovement?> GetByIdAsync(int id)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User) // Added to include User
                .FirstOrDefaultAsync(sm => sm.StockMovementId == id);
        }

        public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User) // Added to include User
                .Where(sm => sm.ProductId == productId)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User) // Added to include User
                .Where(sm => sm.MovementDate >= startDate && sm.MovementDate <= endDate)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(string movementType)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User) // Added to include User
                .Where(sm => sm.MovementType == movementType)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<StockMovement> CreateAsync(StockMovement movement)
        {
            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
            return movement;
        }

        public async Task<StockMovement> UpdateAsync(StockMovement movement)
        {
            _context.StockMovements.Update(movement);
            await _context.SaveChangesAsync();
            return movement;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var movement = await GetByIdAsync(id);
            if (movement == null)
                return false;

            _context.StockMovements.Remove(movement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.StockMovements.AnyAsync(sm => sm.StockMovementId == id);
        }

        public async Task<decimal> GetTotalValueMovedAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.StockMovements
                .Where(sm => sm.ProductId == productId && sm.UnitPrice != null);

            if (startDate.HasValue)
                query = query.Where(sm => sm.MovementDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(sm => sm.MovementDate <= endDate.Value);

            return await query.SumAsync(sm => sm.Quantity * (sm.UnitPrice ?? 0));
        }

        public async Task<int> GetTotalQuantityMovedAsync(int productId, string movementType, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.StockMovements
                .Where(sm => sm.ProductId == productId && sm.MovementType == movementType);

            if (startDate.HasValue)
                query = query.Where(sm => sm.MovementDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(sm => sm.MovementDate <= endDate.Value);

            return await query.SumAsync(sm => sm.Quantity);
        }
    }
}