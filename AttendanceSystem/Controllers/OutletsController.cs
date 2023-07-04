using AttendanceManagementSystem.Extension;
using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutletsController : ControllerBase
    {
        private readonly IMongoCollection<Outlet> _outletCollection;
        private readonly DbHelper _dbHelper;

        public OutletsController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _outletCollection = _dbHelper.GetCollection<Outlet>();
        }

        [HttpPost]
        public async Task<ActionResult<Outlet>> CreateOutlet(Outlet outlet)
        {
            await _outletCollection.InsertOneAsync(outlet);
            return CreatedAtAction(nameof(GetOutlet), new { id = outlet.OutletId }, outlet);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Outlet>> GetOutlet(string id)
        {
            var outlet = await _outletCollection.Find(o => o.OutletId == id).FirstOrDefaultAsync();
            if (outlet == null)
                return NotFound();

            return Ok(outlet);
        }

        public class OutletValidator : AbstractValidator<Outlet>
        {
            public OutletValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Address).NotEmpty();
                RuleFor(x => x.OwnerEmail).EmailAddress().NotEmpty();
                RuleFor(x => x.OwnerPhone).MustBeNumber(10).NotEmpty();
                RuleFor(x => x.OutletPhone).MustBeNumber(10).NotEmpty();
                RuleFor(x => x.UserId).NotEmpty();
            }
        }
    }
}