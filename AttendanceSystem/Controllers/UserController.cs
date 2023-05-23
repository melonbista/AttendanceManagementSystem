using AttendanceSystem.Model;
using AttendanceSystem.Settings;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IMongoCollection<User> _userCollection;

    public UsersController(MongoDbContext dbContext)
    {
        _userCollection = dbContext.Users;
    }

    [HttpPost]
    public ActionResult<User> CreateUser(User user)
    {
        _userCollection.InsertOne(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    public ActionResult<User> GetUser(string id)
    {
        var user = _userCollection.Find(u => u.Id == id).FirstOrDefault();
        if (user == null)
            return NotFound();

        return user;
    }
}