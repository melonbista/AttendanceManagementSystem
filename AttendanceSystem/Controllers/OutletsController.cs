using AttendanceSystem.Model;
using AttendanceSystem.Settings;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutletsController : ControllerBase
    {
        private readonly IMongoCollection<Outlet> _outletCollection;

        public OutletsController(MongoDbContext dbContext)
        {
            _outletCollection = dbContext.Outlets;
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
    }
}