using AttendanceManagementSystem.Extension;
using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OutletVisitController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly AuthHelper _authHelper;
        private readonly IMongoCollection<OutletVisit> _outletVisitCollection;
        private readonly IMongoCollection<Attendance> _attendaceCollection;

        public OutletVisitController(DbHelper dbHelper,AuthHelper authHelper)
        {
            _dbHelper = dbHelper;
            _authHelper = authHelper;
            _outletVisitCollection = _dbHelper.GetCollection<OutletVisit>();
            _attendaceCollection = _dbHelper.GetCollection<Attendance>();
        }

        [HttpGet("Status")]
        public async Task<ActionResult<OutletVisit>> Get()
        {
            var allOutlet = await _outletVisitCollection.Find(_ => true).ToListAsync();
            var outlet = allOutlet.Select(a => new OutletVisit
            {
                UserId = a.Id,
                CheckInTime = a.CheckInTime,
                CheckInLatitude = a.CheckInLatitude,
                CheckInLongitude = a.CheckInLongitude,
                CheckOutTime = a.CheckInTime,
                CheckOutLatitude = a.CheckOutLatitude,
                CheckOutLongitude = a.CheckOutLongitude,
                Status = a.CheckInTime != null && a.CheckOutTime == null ? true : false
            }).ToList();
            return Ok(new
            {
                data = outlet,
            });
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn(CheckInModel input)
        {
            AuthHelper.User? authUser = _authHelper.GetUser();

            //var punchFilter = AttendanceHelper.CurrentFilter(authUser?.Id);

            var punchInFilter = Builders<Attendance>.Filter.Eq(x => x.UserId, authUser?.Id) &
                Builders<Attendance>.Filter.Eq(x => x.PunchOutTime, null) &
                Builders<Attendance>.Filter.Ne(x => x.PunchInTime, null);

            var checkFilter = Builders<OutletVisit>.Filter.Eq(x => x.UserId, authUser?.Id) &
                Builders<OutletVisit>.Filter.Eq(x => x.OutletId, input.OutletId) &
                Builders<OutletVisit>.Filter.Eq(x => x.CheckOutTime, null);

            var existingAttendance = await _attendaceCollection.Find(punchInFilter).FirstOrDefaultAsync();
            if (existingAttendance == null)
            {
                return BadRequest("User is not punch in.");
            }

            var existingVisit = await _outletVisitCollection.Find(checkFilter).FirstOrDefaultAsync();
            if (existingVisit != null)
            {
                return BadRequest("User is already checked in.");
            }

            OutletVisit outletVisit = new()
            {
                OutletId = input.OutletId,
                CheckInTime = DateTime.UtcNow,
                CheckInLatitude = input.Latitude,
                CheckInLongitude = input.Longitude
            };

            await _outletVisitCollection.InsertOneAsync(outletVisit);
            return Ok();
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut(CheckOutModel input)
        {
            AuthHelper.User? authUser = _authHelper.GetUser();

            var punchInFilter = Builders<Attendance>.Filter.Eq(x => x.UserId, authUser?.Id) &
                Builders<Attendance>.Filter.Eq(x => x.PunchOutTime, null) &
                Builders<Attendance>.Filter.Ne(x => x.PunchInTime, null);

            var checkFilter = Builders<OutletVisit>.Filter.Eq(x => x.UserId, authUser?.Id) &
                Builders<OutletVisit>.Filter.Eq(x => x.OutletId, input.OutletId) &
                Builders<OutletVisit>.Filter.Eq(x => x.CheckOutTime, null);

            var existingAttendance = await _attendaceCollection.Find(punchInFilter).FirstOrDefaultAsync();
            if(existingAttendance == null)
            {
                return BadRequest("User is not punch in");
            }

            var existingVisit = await _outletVisitCollection.Find(checkFilter).FirstOrDefaultAsync();
            if (existingVisit == null)
            {
                return BadRequest("User is not checked in at the specified outlet.");
            }

           

            OutletVisit outletVisit = new()
            {
                UserId = authUser?.Id,
                CheckOutTime = DateTime.UtcNow,
                CheckOutLatitude = input.Latitude,
                CheckOutLongitude = input.Longitude,
            };

            await _outletVisitCollection.ReplaceOneAsync(x => x.Id == existingVisit.UserId, outletVisit);
            return Ok();
        }

        public class BaseInputModel
        {
            public string OutletId { get; set; }
            public double Longitude { get; set; }
            public double Latitude { get; set; }
        }

        public class CheckInModel : BaseInputModel { 
        }

        public class CheckOutModel : BaseInputModel { }

        public class CheckInValidator : AbstractValidator<CheckInModel>
        {
            public CheckInValidator()
            {
                RuleFor(x => x.Longitude).MustBeLongitude();
                RuleFor(x => x.Latitude).MustBeLatitude();
                RuleFor(x=>x.OutletId).NotEmpty();
            }
        }

        public class CheckOutValidator : AbstractValidator<CheckOutModel>
        {
            public CheckOutValidator()
            {
                RuleFor(x => x.Longitude).MustBeLongitude();
                RuleFor(x => x.Latitude).MustBeLatitude();
            }
        }
    }
}
