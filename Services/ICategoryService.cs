using SIOMS.Models;
using SIOMS.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIOMS.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<CategoryViewModel> GetCategoryWithStatsAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);
    }
}