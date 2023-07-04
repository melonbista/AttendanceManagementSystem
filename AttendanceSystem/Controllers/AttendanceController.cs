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
        public AttendanceController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _attendanceCollection = _dbHelper.GetCollection<Attendance>();
            _outletVisitCollection = _dbHelper.GetCollection<OutletVisit>();
        }

        [HttpGet("Status")]
        public async Task<ActionResult<Attendance>> Get()
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
        [Authorize]
        public async Task<IActionResult> PunchIn(PunchInModel input)
        {
            if (input.User_id == null)
            {
                return BadRequest("User Id is required");
            }

            var filter = Builders<Attendance>.Filter.Eq(x => x.UserId, input.User_id) &
                Builders<Attendance>.Filter.Eq(x => x.PunchOutTime, null);

            //var existingAttendance = await _attendanceCollection.Find(a => a.UserId == input.User_id && a.PunchOutTime == null).FirstOrDefaultAsync();
            var existingAttendance = await _attendanceCollection.Find(filter).FirstOrDefaultAsync();
            if (existingAttendance != null)
                return BadRequest("User is already punched in.");


            Attendance attendance = new()
            {
                PunchInTime = DateTime.UtcNow,
                PunchInLatitude = input.Latitude,
                PunchInLongitude = input.Longitude,
                UserId = input.User_id
            };

            await _attendanceCollection.InsertOneAsync(attendance);
            return Ok(attendance);
        }

        [HttpPost("punchout")]
        public async Task<ActionResult<Attendance>> PunchOut(PunchOutModel input)
        {
            var existingAttendance = _attendanceCollection.Find(a => a.UserId == input.User_id && a.PunchOutTime == null).FirstOrDefault();
            if (existingAttendance == null)
                return BadRequest("User is not punched in.");

            var existingVisit = await _outletVisitCollection.Find(v => v.UserId == input.User_id && v.CheckInTime != null && v.CheckOutTime == null).FirstOrDefaultAsync();
            if (existingVisit != null)
                return BadRequest("User is currently checked in to an outlet.");

            existingAttendance.PunchOutTime = DateTime.UtcNow;
            existingAttendance.PunchOutLatitude = input.Latitude;
            existingAttendance.PunchOutLongitude = input.Longitude;

            await _attendanceCollection.ReplaceOneAsync(a => a.Id == existingAttendance.Id, existingAttendance);
            return Ok();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<Attendance>> GetAttendance(string userId)
        {
            var attendanceList = await _attendanceCollection.Find(a => a.UserId == userId).ToListAsync();
            return Ok(attendanceList);
        }

        public class BaseInputModel
        {
            public string? User_id { get; set; }
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