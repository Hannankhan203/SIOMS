using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;

        public StockMovementService(ApplicationDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<IEnumerable<StockMovement>> GetAllMovementsAsync()
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<StockMovement?> GetMovementByIdAsync(int id)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .FirstOrDefaultAsync(sm => sm.StockMovementId == id);
        }

        public async Task<IEnumerable<StockMovement>> GetMovementsByProductAsync(int productId)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Where(sm => sm.ProductId == productId)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Where(sm => sm.MovementDate >= startDate && sm.MovementDate <= endDate)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> SearchMovementsAsync(string searchTerm, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.StockMovements
                .Include(sm => sm.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(sm =>
                    sm.Product.Name.Contains(searchTerm) ||
                    sm.Product.SKU.Contains(searchTerm) ||
                    (sm.ReferenceNumber != null && sm.ReferenceNumber.Contains(searchTerm)) ||
                    (sm.Notes != null && sm.Notes.Contains(searchTerm)) ||
                    sm.MovementType.Contains(searchTerm));
            }

            if (startDate.HasValue)
                query = query.Where(sm => sm.MovementDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(sm => sm.MovementDate <= endDate.Value);

            return await query
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<StockMovement> CreateMovementAsync(StockMovement movement, bool updateProductStock = true)
        {
            // Set default movement date if not provided
            if (movement.MovementDate == default)
                movement.MovementDate = DateTime.Now;

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();

            // Update product stock if requested
            if (updateProductStock)
            {
                var product = await _context.Products.FindAsync(movement.ProductId);
                if (product != null)
                {
                    if (movement.MovementType.ToUpper() == "IN" ||
                        movement.MovementType.ToUpper() == "ADJUSTMENT" ||
                        movement.MovementType.ToUpper() == "TRANSFER" && movement.DestinationLocation != null)
                    {
                        product.StockQuantity += movement.Quantity;
                    }
                    else if (movement.MovementType.ToUpper() == "OUT" ||
                             movement.MovementType.ToUpper() == "TRANSFER" && movement.SourceLocation != null)
                    {
                        product.StockQuantity -= movement.Quantity;
                    }

                    product.UpdatedDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }

            return movement;
        }

        public async Task<bool> DeleteMovementAsync(int id)
        {
            var movement = await GetMovementByIdAsync(id);
            if (movement == null)
                return false;

            // Reverse the stock adjustment if needed
            var product = await _context.Products.FindAsync(movement.ProductId);
            if (product != null)
            {
                if (movement.MovementType.ToUpper() == "IN" ||
                    movement.MovementType.ToUpper() == "ADJUSTMENT" ||
                    movement.MovementType.ToUpper() == "TRANSFER" && movement.DestinationLocation != null)
                {
                    product.StockQuantity -= movement.Quantity;
                }
                else if (movement.MovementType.ToUpper() == "OUT" ||
                         movement.MovementType.ToUpper() == "TRANSFER" && movement.SourceLocation != null)
                {
                    product.StockQuantity += movement.Quantity;
                }

                product.UpdatedDate = DateTime.Now;
            }

            _context.StockMovements.Remove(movement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StockMovementSummary> GetMovementSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.StockMovements.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(sm => sm.MovementDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(sm => sm.MovementDate <= endDate.Value);

            var movements = await query.ToListAsync();

            var summary = new StockMovementSummary
            {
                TotalMovements = movements.Count,
                TotalInQuantity = movements
                    .Where(sm => sm.MovementType.ToUpper() == "IN")
                    .Sum(sm => sm.Quantity),
                TotalOutQuantity = movements
                    .Where(sm => sm.MovementType.ToUpper() == "OUT")
                    .Sum(sm => sm.Quantity),
                TotalInValue = movements
                    .Where(sm => sm.MovementType.ToUpper() == "IN" && sm.UnitPrice.HasValue)
                    .Sum(sm => sm.Quantity * sm.UnitPrice!.Value),
                TotalOutValue = movements
                    .Where(sm => sm.MovementType.ToUpper() == "OUT" && sm.UnitPrice.HasValue)
                    .Sum(sm => sm.Quantity * sm.UnitPrice!.Value),
            };

            // Group by movement type
            summary.MovementsByType = movements
                .GroupBy(sm => sm.MovementType)
                .ToDictionary(g => g.Key, g => g.Sum(sm => sm.Quantity));

            return summary;
        }

        public async Task<IEnumerable<ProductMovementSummary>> GetProductMovementSummaryAsync(int productId)
        {
            var movements = await _context.StockMovements
                .Where(sm => sm.ProductId == productId)
                .Include(sm => sm.Product)
                .ToListAsync();

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return new List<ProductMovementSummary>();

            return new List<ProductMovementSummary>
            {
                new ProductMovementSummary
                {
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    TotalIn = movements
                        .Where(sm => sm.MovementType.ToUpper() == "IN")
                        .Sum(sm => sm.Quantity),
                    TotalOut = movements
                        .Where(sm => sm.MovementType.ToUpper() == "OUT")
                        .Sum(sm => sm.Quantity),
                    NetChange = movements
                        .Where(sm => sm.MovementType.ToUpper() == "IN")
                        .Sum(sm => sm.Quantity) -
                        movements.Where(sm => sm.MovementType.ToUpper() == "OUT")
                        .Sum(sm => sm.Quantity),
                    TotalValue = movements
                        .Where(sm => sm.UnitPrice.HasValue)
                        .Sum(sm => sm.Quantity * sm.UnitPrice!.Value)
                }
            };
        }
    }
}