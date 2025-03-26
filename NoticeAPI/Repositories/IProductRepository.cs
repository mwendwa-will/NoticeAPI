using NoticeAPI.Models;

namespace NoticeAPI.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAll(int page, int pageSize, string? category = null,
            decimal? minPrice = null, decimal? maxPrice = null, string? search = null, bool? inStock = null);
        Task<Product> GetById(int id);
        Task Create(Product product);
        Task Update(Product product);
        Task Delete(int id);
        Task UpdateStock(int id, int stock);
        Task CreateBulk(IEnumerable<Product> products);
        Task UpdateStockBulk(IEnumerable<(int Id, int Stock)> updates);
        Task<IEnumerable<Category>> GetCategories();
    }
}
