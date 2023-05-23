using AttendanceSystem.Model;
using AttendanceSystem.Models;
using AttendanceSystem.Settings;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutletVisitController : ControllerBase
    {
        private readonly IMongoCollection<OutletVisit> _outletVisitCollection;
        private readonly IMongoCollection<Attendance> _attendaceCollection;

        public OutletVisitController(MongoDbContext dbContext)
        {
            _outletVisitCollection = dbContext.OutletVisits;
            _attendaceCollection = dbContext.Attendances;
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
                Status = (a.CheckInTime != null && a.CheckOutTime == null) ? true : false
            }).ToList();
            return Ok(new
            {
                data = outlet,
            });
        }

        [HttpPost("checkin")]
        public async Task<ActionResult<OutletVisit>> CheckIn(CheckInModel input)
        {
            var existingAttendance = await _attendaceCollection.Find(a => a.UserId == input.User_id && a.PunchInTime !=null && a.PunchOutTime == null).FirstOrDefaultAsync();
            if (existingAttendance == null)
                return BadRequest("User is not punched in.");

            var existingVisit = await _outletVisitCollection.Find(v => v.UserId == input.User_id && v.CheckOutTime == null).FirstOrDefaultAsync();
            if (existingVisit != null)
                return BadRequest("User is already checked in.");

            OutletVisit outletVisit = new()
            {
                UserId = input.User_id,
                OutletId = input.Outlet_id,
                CheckInTime = DateTime.UtcNow,
                CheckInLatitude = input.Latitude,
                CheckInLongitude = input.Longitude
            };

            await _outletVisitCollection.InsertOneAsync(outletVisit);
            return Ok(outletVisit);
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<OutletVisit>> CheckOut(CheckOutModel input)
        {
            var existingAttendance = await _attendaceCollection.Find(a => a.UserId == input.User_id && a.PunchInTime != null && a.PunchOutTime == null).FirstOrDefaultAsync();
            if (existingAttendance == null)
                return BadRequest("User is not punched in.");

            var existingVisit = await _outletVisitCollection.Find(v => v.UserId == input.User_id && v.OutletId == input.Outlet_id && v.CheckOutTime == null).FirstOrDefaultAsync();
            if (existingVisit == null)
                return BadRequest("User is not checked in at the specified outlet.");


            existingVisit.CheckOutTime = DateTime.UtcNow;
            existingVisit.CheckOutLatitude = input.Latitude;
            existingVisit.CheckOutLongitude = input.Longitude;

            _outletVisitCollection.ReplaceOne(v => v.Id == existingVisit.Id, existingVisit);
            return Ok(existingVisit);
        }

        public class CheckInModel
        {
            public string User_id { get; set; }
            public string Outlet_id { get; set; }
            public double Longitude { get; set; }
            public double Latitude { get; set; }
        }

        public class CheckOutModel
        {
            public string User_id { get; set; }
            public string Outlet_id { get; set; }
            public double Longitude { get; set; }
            public double Latitude { get; set; }
        }
    }
}
