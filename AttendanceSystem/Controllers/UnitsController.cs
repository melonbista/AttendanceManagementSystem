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
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IMongoCollection<Unit> _unitCollection;

        public UnitsController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _unitCollection = _dbHelper.GetCollection<Unit>();
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
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

            var filter = Builders<Unit>.Filter.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                filter &= _dbHelper.LikeFilter<Unit>(x => x.Name, name);
            }

            long totalRecords = await _unitCollection.CountDocumentsAsync(filter);

            Expression<Func<Unit, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "CreatedAt" => x => x.CreatedAt,
                "UpdatedAt" => x => x.UpdatedAt,
                _ => x => x.Id
            };

            DbHelper.Direction direction = sortDirection == "asc" ? DbHelper.Direction.Ascending : DbHelper.Direction.Descending;

            SortDefinition<Unit> sortDefinition = _dbHelper.GetSortDefinition(field, direction);

            var units = await _unitCollection
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(start)
                .Limit(limit)
                .Project(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            

            return Ok(new
            {
                TotalRecords = totalRecords,
                Data = units
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(int page, string term)
        {
            int limit = 10;
            int start = (page - 1) * limit;
            var filter = Builders<Unit>.Filter.Empty;

            if (!string.IsNullOrEmpty(term))
            {
                filter &= _dbHelper.LikeFilter<Unit>(x => x.Name, term);
            }

            long totalRecords = await _unitCollection.CountDocumentsAsync(filter);
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)limit);

            var results = await _unitCollection
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
            var unit = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                },
                Builders<Unit>.Filter.Empty
                );

            if (unit is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }


            return Ok(new
            {
                Id = unit.Id,
                Name = unit.Name,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            Unit unit = new()
            {
                Name = input.Name,
            };

            await _unitCollection.InsertOneAsync(unit);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(UpdateInputModel input, string id)
        {
            var filter = Builders<Unit>.Filter.Eq(x => x.Id, id);


            var Update = Builders<Unit>.Update
                .Set(x => x.Name, input.Name)
                .Set(x => x.CreatedAt, DateTime.UtcNow);

            await _unitCollection.UpdateOneAsync(filter, Update);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _dbHelper.IdExistsAsync(id, Builders<Unit>.Filter.Empty))
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var filter = Builders<Unit>.Filter.Eq(x => x.Id, id);

            await _unitCollection.DeleteOneAsync(filter);
            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
        }
        
        public class AddInputModel : BaseInputModel { }
        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator(DbHelper dbHelper)
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MustBeUnique(dbHelper, x => x.Name, Builders<Unit>.Filter.Empty);
            }
        }

        public class UpdateInputModelValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DbHelper _dbHelper;
            private readonly string? _id;

            public UpdateInputModelValidator(DbHelper dbHelper, IHttpContextAccessor contextAccessor)
            {
                _dbHelper = dbHelper;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MustBeUnique(dbHelper, x => x.Name, Builders<Unit>.Filter.Ne(x => x.Id, _id));
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (!_dbHelper.IdExists(_id, Builders<Unit>.Filter.Empty))
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }

    }
}
