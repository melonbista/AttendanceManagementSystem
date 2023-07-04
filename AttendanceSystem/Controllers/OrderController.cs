using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<Attendance> _attendanceCollection;
        private readonly IMongoCollection<OutletVisit> _outletVisitCollection;

        public OrderController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _orderCollection = _dbHelper.GetCollection<Order>();
            _attendanceCollection = _dbHelper.GetCollection<Attendance>();
            _outletVisitCollection = _dbHelper.GetCollection<OutletVisit>();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var allOrder = await _orderCollection.Find(_ => true).ToListAsync();
            var order = allOrder.Select(a => new Order
            {
                OutletVisitId = a.OutletVisitId,
                UserId = a.UserId,
                Username = a.Username,
                OutletName = a.OutletName,
                Products = a.Products,
                ProductCount = a.ProductCount,
                TotalAmount = a.TotalAmount,
            }).ToList(); 

            return Ok(new
            {
                data = order,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddInputModel input)
        {
            var punchInFilter = Builders<Attendance>.Filter.Ne(x => x.PunchInTime, null) &
                Builders<Attendance>.Filter.Eq(x => x.PunchOutTime, null);
            var checkInFilter = Builders<OutletVisit>.Filter.Eq(x => x.CheckOutTime, null) &
                Builders<OutletVisit>.Filter.Ne(x => x.CheckInTime, null);
            var isOnCallFilter = Builders<Order>.Filter.Eq(x => x.IsOnCall, true);

            var isPunchIn = await _attendanceCollection.Find(punchInFilter).FirstOrDefaultAsync();
            if (isPunchIn == null)
            {
                return BadRequest("User is not punched in");
            }

            var isCheckIn = await _outletVisitCollection.Find(checkInFilter).FirstOrDefaultAsync();
            if (isCheckIn == null)
            {
                return BadRequest("User is not checked in");
            }

            Order order = new()
            {
               Quantity = input.Quantity,
               IsOnCall = false,
               OutletId = input.OutletId,
            };

            await _orderCollection.InsertOneAsync(order);
            return Ok();
        }

        public IEnumerable<ProductInputModel> Products { get; set; } = new List<ProductInputModel>();

        public class ProductInputModel
        {
            public string Id { get; set; }
            public string Quantity { get; set; }
        }

        public class AddInputModel : ProductInputModel
        {
            public bool IsOnCall { get; set; }
            public string OutletId { get; set; }    
        }
    }
}
