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
    public class AttendanceController : ControllerBase
    {
        private readonly IMongoCollection<Attendance> _attendanceCollection;
        private readonly IMongoCollection<OutletVisit> _outletVisitCollection;
        private readonly DbHelper _dbHelper;
        private readonly AuthHelper _authHelper;
        public AttendanceController(DbHelper dbHelper, AuthHelper authHelper)
        {
            _dbHelper = dbHelper;
            _attendanceCollection = _dbHelper.GetCollection<Attendance>();
            _outletVisitCollection = _dbHelper.GetCollection<OutletVisit>();
            _authHelper = authHelper;
        }

        [HttpGet("Status")]
        public async Task<IActionResult> Get()
        {
            var allAttendance = await _attendanceCollection.Find(_ => true).ToListAsync();
            var attendance = allAttendance.Select(a => new Attendance
            {
                UserId = a.Id,
                PunchInTime = a.PunchInTime,
                PunchInLatitude = a.PunchInLatitude,
                PunchInLongitude = a.PunchInLongitude,
                PunchOutTime = a.PunchInTime,
                PunchOutLatitude = a.PunchOutLatitude,
                PunchOutLongitude = a.PunchOutLongitude,
                Status = a.PunchInTime != null && a.PunchOutTime == null ? true : false
            }).ToList();
            return Ok(new
            {
                data = attendance,
            });
        }

        [HttpPost("punchin")]
        public async Task<IActionResult> PunchIn(PunchInModel input)
        {
            AuthHelper.User? authUser = _authHelper.GetUser();

            var filter = Builders<Attendance>.Filter.Eq(x => x.UserId, authUser?.Id) &
                Builders<Attendance>.Filter.Eq(x => x.PunchOutTime, null);

            var existingAttendance = await _attendanceCollection.Find(filter).FirstOrDefaultAsync();
            if (existingAttendance != null)
                return BadRequest("User is already punched in.");

            Attendance attendance = new()
            {
                PunchInTime = DateTime.UtcNow,
                PunchInLatitude = input.Latitude,
                PunchInLongitude = input.Longitude,
                UserId = authUser?.Id
            };

            await _attendanceCollection.InsertOneAsync(attendance);
            return Ok();
        }

        [HttpPost("punchout")]
        public async Task<IActionResult> PunchOut(PunchOutModel input)
        {
            AuthHelper.User? authUser = _authHelper.GetUser();

            var attendanceFilter = AttendanceHelper.CurrentFilter(authUser?.Id);

            var outletVisitFilter = OutletVisitHelper.CurrentFilter(authUser?.Id);

            var existingAttendance = await _attendanceCollection.Find(attendanceFilter).FirstOrDefaultAsync();
            if (existingAttendance == null)
                return BadRequest("User is not checkout of an outlet.");

            var existingOutletVisit = await _outletVisitCollection.Find(outletVisitFilter).FirstOrDefaultAsync();
            if (existingOutletVisit == null)
            {
                return BadRequest("User is currently checked in to an outlet.");
            }

            var update = Builders<Attendance>.Update
                .Set(x => x.PunchOutTime, DateTime.UtcNow)
                .Set(x => x.PunchInLatitude, input.Latitude)
                .Set(x => x.PunchOutLongitude, input.Longitude);

            await _attendanceCollection.UpdateOneAsync(attendanceFilter, update);

            return Ok();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAttendance(string userId)
        {
            var attendanceList = await _attendanceCollection.Find(a => a.UserId == userId).ToListAsync();
            return Ok(attendanceList);
        }

        public class BaseInputModel
        {
            public double Longitude { get; set; }
            public double Latitude { get; set; }
        }

        public class PunchInModel : BaseInputModel { }

        public class PunchOutModel : BaseInputModel { }

        public class PunchInInputModelValidator : AbstractValidator<PunchInModel>
        {
            public PunchInInputModelValidator()
            {
                RuleFor(x => x.Longitude).MustBeLongitude();
                RuleFor(x => x.Latitude).MustBeLatitude();
            }
        }

        public class PunchOutModelValidator : AbstractValidator<PunchOutModel>
        {
            public PunchOutModelValidator()
            {
                RuleFor(x => x.Longitude).MustBeLongitude();
                RuleFor(x => x.Latitude).MustBeLatitude();
            }
        }
    }
}