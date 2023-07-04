using AttendanceManagementSystem.Extension;
using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace AttendanceManagementSystem.Controllers
{
    [ApiController]
    [Route("api/controller")]
    public class DivisionController: ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IMongoCollection<Division> _divisionCollection;

        public DivisionController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _divisionCollection = _dbHelper.GetCollection<Division>();  
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page,int limit,string sortColumn,string sortDirection, string name,string abbreviation)
        {
            if(limit <= 0) {
                limit = 5;
            }
            else if(limit > 20) {
                limit = 20;
            }
            int start = (page - 1) * limit;

            var filter = Builders<Division>.Filter.Empty;

            if(!string.IsNullOrEmpty(name))
            {
                filter &= _dbHelper.LikeFilter<Division>(x=>x.Name, name);
            }
            if(!string.IsNullOrEmpty(abbreviation))
            {
                filter &= _dbHelper.LikeFilter<Division>(x=>x.Abbreviation, abbreviation);
            }

            long totalRecords = await _divisionCollection.CountDocumentsAsync(filter);

            Expression<Func<Division, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "Abbreviation" => x => x.Abbreviation,
                "CreatedAt" => x => x.CreatedAt,
                "UpdatedAt" => x => x.UpdatedAt,
                _ => x => x.Id
            };

            DbHelper.Direction direction = sortDirection == "asc" ? DbHelper.Direction.Ascending: DbHelper.Direction.Descending;

            SortDefinition<Division> sortDefinition = _dbHelper.GetSortDefinition(field,direction);

            var data = await _divisionCollection
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(start)
                .Limit(limit)
                .Project(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Abbreviation = x.Abbreviation,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                }).ToListAsync();

            return Ok(new
            {
                TotalRecords = totalRecords,
                Data = data
            });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var filter = Builders<Division>.Filter.Empty;
            var divisions = await _divisionCollection
                .Find(filter)
                .Project(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                }).ToListAsync();

            return Ok(new
            {
                Divisions = divisions
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var division = await _dbHelper.FindByIdAsync(
                    id,
                    x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Abbreviation = x.Abbreviation
                    },
                    Builders<Division>.Filter.Empty
                );
            if(division is null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            return Ok(new
            {
                Division = division
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            Division division = new()
            {
                Name = input.Name,
                Abbreviation = input.Abbreviation,
            };

            await _divisionCollection.InsertOneAsync(division);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id,UpdateInputModel input)
        {
            var filter = Builders<Division>.Filter.Eq(x => x.Id, id);

            var update = Builders<Division>.Update
                .Set(x => x.Name, input.Name)
                .Set(x => x.Abbreviation, input.Abbreviation)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _divisionCollection.UpdateOneAsync(filter, update);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _dbHelper.IdExistsAsync(id, Builders<Division>.Filter.Empty))
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var filter = Builders<Division>.Filter.Eq(x => x.Id, id);

            await _divisionCollection.DeleteOneAsync(filter);
            return Ok();
        }

        public class BaseInputModel
        {
            public string Name { get; set; }
            public string Abbreviation { get; set; }
        }

        public class AddInputModel : BaseInputModel { }

        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator :AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator(DbHelper dbHelper)
            {
                RuleFor(x => x.Name)
                    .NotEmpty();

                RuleFor(x => x.Abbreviation)
                    .NotEmpty()
                    .MustBeUnique(dbHelper, x => x.Abbreviation, Builders<Division>.Filter.Empty);
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
                    .NotEmpty();

                RuleFor(x => x.Abbreviation)
                    .NotEmpty()
                    .MustBeUnique(dbHelper, x => x.Abbreviation, Builders<Division>.Filter.Ne(x => x.Id, _id));
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (!_dbHelper.IdExists(_id, Builders<Division>.Filter.Empty))
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid."));
                    return false;
                }

                return true;
            }
        }
    }
}
