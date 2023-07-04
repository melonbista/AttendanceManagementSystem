using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IMongoCollection<Product> _productCollection;

        public ProductController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _productCollection = _dbHelper.GetCollection<Product>();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var allProduct = await _productCollection.Find(_ => true).ToListAsync();
            var prooduct = allProduct.Select(a => new Product
            {
                ProductId = a.ProductId,
                ProductName = a.ProductName,
                Price = a.Price,
                BrandName = a.BrandName
            });

            return Ok(new
            {
                data = prooduct,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductInputModel input)
        {
            Product product = new Product
            {
                ProductId = input.ProductId,
                ProductName = input.ProductName,
                Price = input.Price,
                BrandName = input.BrandName
            };
            await _productCollection.InsertOneAsync(product);
            return Ok();
        }


        public class BaseInputModel
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string Price { get; set; }
            public string BrandName { get; set; }
        }

        public class ProductInputModel : BaseInputModel { }
    }
}


