using AttendanceManagementSystem.Extension;
using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace AttendanceManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IMongoCollection<Brand> _brandCollection;

        public BrandsController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _brandCollection = _dbHelper.GetCollection<Brand>();
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name, string divisionId, string verticalId)
        {
            if (limit <= 0)
            {
                limit = 5;
            }
            else if (limit > 20)
            {
                limit = 20;
            }

            int start = (page - 1) * limit;

            var filter = Builders<Brand>.Filter.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                filter &= _dbHelper.LikeFilter<Brand>(x => x.Name, name);
            }

            if (!string.IsNullOrEmpty(divisionId))
            {
                filter &= Builders<Brand>.Filter.Eq(x => x.DivisionId, divisionId);
            }

            if (!string.IsNullOrEmpty(verticalId))
            {
                filter &= Builders<Brand>.Filter.Eq(x => x.VerticalId, verticalId);
            }

            long totalRecords = await _brandCollection.CountDocumentsAsync(filter);

            Expression<Func<Brand, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "CreatedAt" => x => x.CreatedAt,
                "UpdatedAt" => x => x.UpdatedAt,
                _ => x => x.Id
            };

            DbHelper.Direction direction = sortDirection == "asc" ? DbHelper.Direction.Ascending : DbHelper.Direction.Descending;

            SortDefinition<Brand> sortDefinition = _dbHelper.GetSortDefinition(field, direction);

            var brands = await _brandCollection
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(start)
                .Limit(limit)
                .Project(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    DivisionId = x.DivisionId,
                    VerticalId = x.VerticalId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            var divisions = await _dbHelper.GetParentModels(
                brands,
                x => x.DivisionId,
                x => x.Id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name
                },
                x => x.Id,
                Builders<Division>.Filter.Empty
            );

            var verticals = await _dbHelper.GetParentModels(
                brands,
                x => x.VerticalId,
                x => x.Id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name
                },
                x => x.Id,
                Builders<Vertical>.Filter.Empty
            );

            var data = brands
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    DivisionName = x.DivisionId is not null && divisions.ContainsKey(x.DivisionId)
                                    ? divisions[x.DivisionId].Name
                                    : null,
                    VerticalName = x.VerticalId is not null && verticals.ContainsKey(x.VerticalId)
                                    ? verticals[x.VerticalId].Name
                                    : null,
                });

            return Ok(new
            {
                TotalRecords = totalRecords,
                Data = data
            });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var filter = Builders<Brand>.Filter.Empty;

            var brands = await _brandCollection
                .Find(filter)
                .Project(x => new
                {
                    VerticalId = x.VerticalId,
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

            return Ok(new
            {
                Brands = brands
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(int page, string term, string divisionId)
        {
            int limit = 10;
            int start = (page - 1) * limit;
            var filter = Builders<Brand>.Filter.Empty;

            if (!string.IsNullOrEmpty(term))
            {
                filter &= _dbHelper.LikeFilter<Brand>(x => x.Name, term);
            }

            if (!string.IsNullOrEmpty(divisionId))
            {
                filter &= _dbHelper.LikeFilter<Brand>(x => x.DivisionId, divisionId);
            }

            long totalRecords = await _brandCollection.CountDocumentsAsync(filter);
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)limit);

            var results = await _brandCollection
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
            var brand = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    VerticalId = x.DivisionId
                },
                Builders<Vertical>.Filter.Empty
                );

            if (brand is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var vertical = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                },
                Builders<Vertical>.Filter.Empty
                );

            return Ok(new
            {
                Id = brand.Id,
                Name = brand.Name,
                VerticalId = brand.VerticalId,
                DivisionName = vertical?.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)

        {
            var vertical = await _dbHelper.FindByIdAsync(
                input.VerticalId,
                x => new
                {
                    Id = x.Id,
                    DivisionId = x.DivisionId,
                },
                Builders<Vertical>.Filter.Empty
                );

            Brand brand = new Brand
            {
                DivisionId = vertical.DivisionId,
                VerticalId = input.VerticalId,
                Name = input.Name
            };

            await _brandCollection.InsertOneAsync(brand);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(UpdateInputModel input, string id)
        {
            var filter = Builders<Brand>.Filter.Eq(x => x.Id, id);

            var vertical = await _dbHelper.FindByIdAsync(
                input.VerticalId,
                x => new
                {
                    Id = x.Id,
                    DivisionId = x.DivisionId
                },
                Builders<Vertical>.Filter.Empty
                );

            var Update = Builders<Brand>.Update
                .Set(x => x.DivisionId, vertical.DivisionId)
                .Set(x => x.VerticalId, input.VerticalId)
                .Set(x=>x.Name,input.Name)
                .Set(x => x.CreatedAt, DateTime.UtcNow);

            await _brandCollection.UpdateOneAsync(filter, Update);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _dbHelper.IdExistsAsync(id, Builders<Brand>.Filter.Empty))
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var filter = Builders<Brand>.Filter.Eq(x => x.Id, id);

            await _brandCollection.DeleteOneAsync(filter);
            return Ok();
        }

        public class BaseInputModel
        {
            public string VerticalId { get; set; }
            public string Name { get; set; }
        }

        public class AddInputModel : BaseInputModel { }
        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator(DbHelper dbHelper)
            {
                RuleFor(x => x.VerticalId)
                    .NotEmpty()
                    .IdMustExist(dbHelper, Builders<Vertical>.Filter.Empty);


                RuleFor(x => x.Name)
                    .NotEmpty();
            }
            
        }

        public class UpdateInputValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DbHelper _dbHelper;
            private readonly string? _id;

            public UpdateInputValidator(DbHelper dbHelper, IHttpContextAccessor contextAccessor)
            {
                _dbHelper = dbHelper;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.Name)
                    .NotEmpty();

                RuleFor(x => x.VerticalId)
                    .NotEmpty()
                    .IdMustExist(dbHelper, Builders<Vertical>.Filter.Empty);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (!_dbHelper.IdExists(_id, Builders<Brand>.Filter.Empty))
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid"));
                    return false;
                }
                return true;
            }
        }

    }
}
