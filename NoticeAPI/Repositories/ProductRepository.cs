using Dapper;

using Microsoft.Extensions.Caching.Memory;
using NoticeAPI.Models;
using NoticeAPI.Repositories;
using NoticeAPI.Services;
using System.Data;

namespace NoticeAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IMemoryCache _cache;

        public ProductRepository(IDbConnectionFactory connectionFactory, IMemoryCache cache)
        {
            _connectionFactory = connectionFactory;
            _cache = cache;
        }

        public async Task<IEnumerable<Product>> GetAll(int page, int pageSize, string? category = null,
            decimal? minPrice = null, decimal? maxPrice = null, string? search = null, bool? inStock = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            var offset = (page - 1) * pageSize;

            var builder = new SqlBuilder();
            var template = builder.AddTemplate(@"
                SELECT p.*, c.*
                FROM products p
                INNER JOIN categories c ON p.category_id = c.id
                /**where**/
                LIMIT @PageSize OFFSET @Offset",
                new { PageSize = pageSize, Offset = offset });

            if (!string.IsNullOrEmpty(category))
                builder.Where("c.name = @Category", new { Category = category });
            if (minPrice.HasValue)
                builder.Where("p.price >= @MinPrice", new { MinPrice = minPrice });
            if (maxPrice.HasValue)
                builder.Where("p.price <= @MaxPrice", new { MaxPrice = maxPrice });
            if (!string.IsNullOrEmpty(search))
                builder.Where("p.name LIKE @Search", new { Search = $"%{search}%" });
            if (inStock.HasValue)
                builder.Where("p.stock > 0 = @InStock", new { InStock = inStock.Value });

            return await connection.QueryAsync<Product, Category, Product>(
                template.RawSql,
                (product, cat) =>
                {
                    product.Category = cat;
                    return product;
                },
                template.Parameters,
                splitOn: "id");
        }

        public async Task<Product> GetById(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT p.*, c.*
                FROM products p
                INNER JOIN categories c ON p.category_id = c.id
                WHERE p.id = @Id";
            var products = await connection.QueryAsync<Product, Category, Product>(
                sql,
                (product, cat) =>
                {
                    product.Category = cat;
                    return product;
                },
                new { Id = id },
                splitOn: "id");
            return products.FirstOrDefault();
        }

        public async Task Create(Product product)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO products (name, description, price, stock, category_id, image_url)
                VALUES (@Name, @Description, @Price, @Stock, @CategoryId, @ImageUrl);
                SELECT LAST_INSERT_ID()";
            product.Id = await connection.QuerySingleAsync<int>(sql, product);
        }

        public async Task Update(Product product)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                UPDATE products 
                SET name = @Name, description = @Description, price = @Price, 
                    stock = @Stock, category_id = @CategoryId, image_url = @ImageUrl
                WHERE id = @Id";
            await connection.ExecuteAsync(sql, product);
        }

        public async Task Delete(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync("DELETE FROM products WHERE id = @Id", new { Id = id });
        }

        public async Task UpdateStock(int id, int stock)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "UPDATE products SET stock = @Stock WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id, Stock = stock });
        }

        public async Task CreateBulk(IEnumerable<Product> products)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                INSERT INTO products (name, description, price, stock, category_id, image_url)
                VALUES (@Name, @Description, @Price, @Stock, @CategoryId, @ImageUrl)";
            await connection.ExecuteAsync(sql, products);
        }

        public async Task UpdateStockBulk(IEnumerable<(int Id, int Stock)> updates)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = "UPDATE products SET stock = @Stock WHERE id = @Id";
            await connection.ExecuteAsync(sql, updates.Select(u => new { u.Id, u.Stock }));
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            const string cacheKey = "Categories";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Category> categories))
            {
                using var connection = _connectionFactory.CreateConnection();
                categories = await connection.QueryAsync<Category>("SELECT * FROM categories");

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                _cache.Set(cacheKey, categories, cacheOptions);
            }
            return categories;
        }
    }
}