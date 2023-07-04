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
        private readonly AuthHelper _authHelper;

        public OutletsController(DbHelper dbHelper, AuthHelper authHelper)
        {
            _dbHelper = dbHelper;
            _outletCollection = _dbHelper.GetCollection<Outlet>();
            _authHelper = authHelper;
        }

        [HttpPost]
        public async Task<ActionResult<Outlet>> CreateOutlet(OutletInputMOdel input)
        {
            AuthHelper.User? authUser = _authHelper.GetUser();

            var filter = Builders<Outlet>.Filter.Eq(x=>x.OwnerEmail, input.OwnerEmail);

            var outletExist =  _outletCollection.Find(filter).Any();
            if(outletExist)
            {
                return BadRequest("Outlet Exists");
            }

            Outlet outlet = new()
            {
                Name = input.Name,
                OwnerEmail = input.OwnerEmail,
                OwnerPhone = input.OwnerPhone,
                OutletPhone = input.OutletPhone,    
                Latitude = input.Latitude,
                Longitude = input.Longitude,
            };
            await _outletCollection.InsertOneAsync(outlet);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Outlet>> GetOutlet(string id)
        {
            var outlet = await _outletCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
            if (outlet == null)
                return NotFound();

            return Ok(outlet);
        }

        public class OutletInputMOdel
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string OwnerEmail { get; set; }
            public string OwnerPhone { get; set; }
            public string OutletPhone { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public class OutletValidator : AbstractValidator<OutletInputMOdel>
        {
            public OutletValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Address).NotEmpty();
                RuleFor(x => x.OwnerEmail).EmailAddress().NotEmpty();
                RuleFor(x => x.OwnerPhone).MustBeNumber(10).NotEmpty();
                RuleFor(x => x.OutletPhone).MustBeNumber(10).NotEmpty();
            }
        }
    }
}