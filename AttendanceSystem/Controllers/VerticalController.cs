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
    public class VerticalController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IMongoCollection<Vertical> _verticalCollection;

        public VerticalController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _verticalCollection = _dbHelper.GetCollection<Vertical>();
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn,string sortDirection ,string name,string divisionId)
        {
            if (limit <= 0)
            {
                limit = 5;
            }
            if (limit > 20)
            {
                limit = 20;
            }
            int start = (page - 1) * limit;

            var filter = Builders<Vertical>.Filter.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                filter &= _dbHelper.LikeFilter<Vertical>(x => x.Name, name);
            }
            if(!string.IsNullOrEmpty(divisionId))
            {
                filter &= _dbHelper.LikeFilter<Vertical>(x=>x.DivisionId, divisionId);
            }

            long totalRecords = await _verticalCollection.CountDocumentsAsync(filter);

            Expression<Func<Vertical, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                "DivisionId" => x => x.DivisionId,
                "CreatedAt" => x => x.CreatedAt,
                "UpdatedAt" => x => x.UpdatedAt,
                _ => x => x.Id
            };

            DbHelper.Direction direction = sortDirection == "asc" ? DbHelper.Direction.Ascending : DbHelper.Direction.Descending;

            SortDefinition<Vertical> sortDefinition = _dbHelper.GetSortDefinition(field, direction);

            var verticals = await _verticalCollection
                .Find(filter)
                .Sort(sortDefinition)
                .Skip(start)
                .Limit(limit)
                .Project(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    DivisionId = x.DivisionId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                }).ToListAsync();

            var divisions = await _dbHelper.GetParentModels(
                    verticals,
                    x => x.DivisionId,
                    x => x.Id,
                    x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                    },
                    x => x.Id,
                    Builders<Division>.Filter.Empty
                );

            var data = verticals.Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DivisionName = x.DivisionId is not null && divisions.ContainsKey(x.DivisionId) ? divisions[x.DivisionId].Name : null,
            });

            return Ok(new
            {
                TotalRecords = totalRecords,
                Data = data,
            });
         }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var filter = Builders<Vertical>.Filter.Empty;

            var verticals = await _verticalCollection
                .Find(filter)
                .Project(x => new
                {
                    Id = x.Id,
                    DivisionId = x.DivisionId,
                    Name = x.Name
                }).ToListAsync();

            return Ok(new
            {
                Verticals = verticals,
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(int page,string term, string divisionId)
        {
            int limit = 10;
            int start = (page - 1) * limit;
            var filter = Builders<Vertical>.Filter.Empty;

            if (!string.IsNullOrEmpty(term))
            {
                filter &= _dbHelper.LikeFilter<Vertical>(x => x.Name,term);
            }

            if(!string.IsNullOrEmpty(divisionId))
            {
                filter &= _dbHelper.LikeFilter<Vertical>(x=>x.DivisionId,divisionId);
            }

            long totalRecords = await _verticalCollection.CountDocumentsAsync(filter);
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)limit);

            var results = await _verticalCollection
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
            var vertical = await _dbHelper.FindByIdAsync(
                id,
                x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    DivisionId = x.DivisionId
                },
                Builders<Vertical>.Filter.Empty
                );
            
            if( vertical is null)
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

            return Ok(new
            {
                Id = vertical.Id,
                Name = vertical.Name,
                DivisionId = vertical.DivisionId,
                DivisionName = division?.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)

        {
            Vertical vertical = new()
            {
                Name = input.Name,
                DivisionId = input.DivisionId,
            };

            await _verticalCollection.InsertOneAsync(vertical);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(UpdateInputModel input, string id)
        {
            var filter = Builders<Vertical>.Filter.Eq(x => x.Id, id);

            var Update = Builders<Vertical>.Update
                .Set(x => x.DivisionId, input.DivisionId)
                .Set(x => x.Name, input.Name)
                .Set(x => x.CreatedAt, DateTime.UtcNow);

            await _verticalCollection.UpdateOneAsync(filter, Update);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if(!await _dbHelper.IdExistsAsync(id, Builders<Vertical>.Filter.Empty))
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid");
            }

            var filter = Builders<Vertical>.Filter.Eq(x=>x.Id, id);

            await _verticalCollection.DeleteOneAsync(filter);
            return Ok();
        }
            
        public class BaseInputModel
        {
            public string Name { get; set; }
            public string DivisionId { get; set; }
        }

        public class AddInputModel : BaseInputModel { }
        public class UpdateInputModel : BaseInputModel { }

        public class AddInputModelValidator : AbstractValidator<AddInputModel>
        {
            public AddInputModelValidator(DbHelper dbHelper)
            {
                RuleFor(x => x.Name)
                    .NotEmpty();
                RuleFor(x => x.DivisionId)
                    .NotEmpty()
                    .IdMustExist(dbHelper, Builders<Division>.Filter.Empty);
            }
        }

        public class UpdateInputValidator : AbstractValidator<UpdateInputModel>
        {
            private readonly DbHelper _dbHelper;
            private readonly string? _id;

            public UpdateInputValidator(DbHelper dbHelper,IHttpContextAccessor contextAccessor)
            {
                _dbHelper = dbHelper;
                _id = contextAccessor.HttpContext?.Request?.RouteValues["id"]?.ToString();

                RuleFor(x=>x.Name)
                    .NotEmpty();

                RuleFor(x => x.DivisionId)
                    .NotEmpty()
                    .IdMustExist(dbHelper, Builders<Division>.Filter.Empty);
            }

            protected override bool PreValidate(ValidationContext<UpdateInputModel> context, ValidationResult result)
            {
                if (!_dbHelper.IdExists(_id, Builders<Division>.Filter.Empty))
                {
                    result.Errors.Add(new ValidationFailure("Id", "Id is invalid"));
                    return false;
                }
                return true;
            }
        }
    }
}
