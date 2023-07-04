using AttendanceManagementSystem.Extension;
using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using FluentValidation;
using FluentValidation.Results;
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
                ProductName = a.ProductName,
                BrandName = a.BrandName
            });

            return Ok(new
            {
                data = prooduct,
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(int page, string term, string divisionId,string verticalId,string brandId)
        {
            int limit = 10;
            int start = (page - 1) * limit;

            var filter = Builders<Product>.Filter.Empty;

            if (!string.IsNullOrEmpty(term))
            {
                filter &= _dbHelper.LikeFilter<Product>(x => x.Name, term);
            }

            if (!string.IsNullOrEmpty(divisionId))
            {
                filter &= _dbHelper.LikeFilter<Product>(x => x.DivisionId, divisionId);

            }if (!string.IsNullOrEmpty(verticalId))
            {
                filter &= _dbHelper.LikeFilter<Product>(x => x.VerticalId, verticalId);
            }

            if (!string.IsNullOrEmpty(brandId))
            {
                filter &= _dbHelper.LikeFilter<Product>(x => x.BrandId, brandId);
            }

            long totalRecords = await _productCollection.CountDocumentsAsync(filter);
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)limit);

            var results = await _productCollection
                .Find(filter)
                .Skip(start)
                .Limit(limit)
                .Project(x => new
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

            return Ok(new
            {
                Results = results,
                HasMore = page < totalPages
            });
        }

        [HttpGet("id")]
        public async Task<IActionResult> Get(string id)
        {
            var product = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    DivisionId = x.DivisionId,
                    VerticalId = x.VerticalId,
                    BrandId = x.BrandId,
                    UnitId = x.UnitId,
                    IsFeatured = x.IsFeatured
                },
                Builders<Product>.Filter.Empty
                );

            if (product is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var division = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                },
                Builders<Division>.Filter.Empty
                );

            var vertical = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                },
                Builders<Vertical>.Filter.Empty
                );

            var brand = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                },
                Builders<Brand>.Filter.Empty
                );

            var unit = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                },
                Builders<Unit>.Filter.Empty
                );

            return Ok(new
            {
                Id = product.Id,
                Name = product.Name,
                DivisionId = product.DivisionId,
                VerticalId = product.VerticalId,
                UnitId = product.UnitId, 
                IsFeatured = product.IsFeatured,
                DivisionName = division?.Name,
                VerticalName = vertical?.Name,
                BrandName = brand?.Name,
                UnitName = unit?.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var brand = await _dbHelper.FindByIdAsync(
                input.BrandId,
                x => new
                {
                    Id = x.Id,
                    VerticalId = x.VerticalId,
                    DivisionId = x.DivisionId
                },
                Builders<Brand>.Filter.Empty
            );

            if(brand == null) return BadRequest("sdfs");

            Product product = new()
            {
                DivisionId = brand.DivisionId,
                VerticalId= brand.VerticalId,
                BrandId = input.BrandId,
                UnitId = input.UnitId,
                BrandName = input.BrandName,
                ProductName = input.ProductName,
                IsFeatured = input.IsFeatured,
                IsActive = true,
            };

            await _productCollection.InsertOneAsync( product );
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(UpdateInputModel input, string id)
        {
            var brand = await _dbHelper.FindByIdAsync(
                input.BrandId,
                x => new
                {
                    Id = x.Id,
                    VerticalId = x.VerticalId,
                    DivisionId = x.DivisionId,
                },
                Builders<Brand>.Filter.Empty
                );

            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);

            var Update = Builders<Product>.Update
                .Set(x => x.DivisionId, brand.DivisionId)
                .Set(x => x.VerticalId,brand.VerticalId)
                .Set(x=>x.BrandId, input.BrandId)
                .Set(x=>x.UnitId, input.UnitId)
                .Set(x => x.Name, input.Name)
                .Set(x=>x.IsFeatured,input.IsFeatured)
                .Set(x => x.CreatedAt, DateTime.UtcNow);

            await _productCollection.UpdateOneAsync(filter, Update);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if(!await _dbHelper.IdExistsAsync(id, Builders<Product>.Filter.Empty))
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var filter = Builders<Product>.Filter.Eq(x=>x.Id, id);

            await _productCollection.DeleteOneAsync(filter);
            return Ok();
        }

        public class BaseInputModel
        {
            public string BrandId { get; set; }
            public string Name { get; set; }
            public string UnitId { get; set; }
            public bool IsFeatured { get; set; }

        }

        public class AddInputModel : BaseInputModel { }
        public class UpdateInputModel : BaseInputModel { }

        public class AddInputVlidator : AbstractValidator<AddInputModel>
        {
            public AddInputVlidator(DbHelper dbHelper)
            {
                RuleFor(x => x.BrandId)
                    .NotEmpty()
                    .IdMustExist(dbHelper, Builders<Brand>.Filter.Empty);

                RuleFor(x => x.Name)
                    .NotEmpty();
                RuleFor(x => x.UnitId)
                    .IdMustExist(dbHelper, Builders<Unit>.Filter.Empty);
            }
        }

        public class UpdateInputMelodel : AbstractValidator<UpdateInputModel>
        {
            private readonly DbHelper _dbHelper;
            private readonly string? _id;

            public UpdateInputMelodel(DbHelper dbHelper, IHttpContextAccessor contextAccessor)
            {
                _dbHelper = dbHelper;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.Name)
                    .NotEmpty();

                RuleFor(x => x.UnitId)
                    .NotEmpty()
                    .IdMustExist(dbHelper,Builders<Unit>.Filter.Empty);

                RuleFor(x=>x.BrandId)
                    .NotEmpty()
                    .IdMustExist(dbHelper,Builders<Brand>.Filter.Empty);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (!_dbHelper.IdExists(_id, Builders<Product>.Filter.Empty))
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid"));
                    return false;
                }

                return true;
            }
        }
    }
}


