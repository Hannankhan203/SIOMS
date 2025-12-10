using SIOMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIOMS.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetProductCountAsync(int categoryId);
        Task<bool> HasProductsAsync(int categoryId);
    }
}