using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using NoticeAPI.Models;
using NoticeAPI.Repositories;
using NoticeAPI.Services;

namespace NoticeAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly INotificationService _notificationService;

        public ProductsController(IProductRepository repository, INotificationService notificationService)
        {
            _repository = repository;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? search = null,
            [FromQuery] bool? inStock = null)
        {
            var products = await _repository.GetAll(page, pageSize, category, minPrice, maxPrice, search, inStock);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repository.GetById(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _repository.Create(product);

            // Send notification to a topic (e.g., "new-products")
            await _notificationService.SendTopicNotificationAsync(
                "new-products",
                "New Product Added",
                $"Check out {product.Name} now available for ${product.Price}!"
            );

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id) return BadRequest("ID mismatch");
            await _repository.Update(product);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.Delete(id);
            return NoContent();
        }

        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int stock)
        {
            if (stock < 0) return BadRequest("Stock cannot be negative");
            await _repository.UpdateStock(id, stock);

            // Send notification to a specific device (requires a client token)
            // For demo, assume a hardcoded token; in practice, store tokens per user
            string clientToken = "YOUR_DEVICE_TOKEN_HERE";
            await _notificationService.SendNotificationAsync(
                clientToken,
                "Stock Updated",
                $"Product ID {id} stock is now {stock}."
            );

            return NoContent();
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] IEnumerable<Product> products)
        {
            if (!products.Any()) return BadRequest("No products provided");
            await _repository.CreateBulk(products);
            return Ok();
        }

        [HttpPut("stock/bulk")]
        public async Task<IActionResult> UpdateStockBulk([FromBody] IEnumerable<(int Id, int Stock)> updates)
        {
            if (!updates.Any()) return BadRequest("No updates provided");
            await _repository.UpdateStockBulk(updates);
            return NoContent();
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _repository.GetCategories();
            return Ok(categories);
        }
    }
}