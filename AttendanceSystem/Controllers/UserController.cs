using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using FluentValidation;
using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Extension;
using AttendanceManagementSystem.Helper;

namespace AttendanceManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbHelper _dbHelper;
        private readonly IMongoCollection<User> _userCollection;
        public UsersController(DbHelper dbContext)
        {
            _dbHelper = dbContext;
            _userCollection = _dbHelper.GetCollection<User>();
        }

        [HttpPost("register")]
        public IActionResult RegisterUser(RegisterRequest request)
        {

            if (_userCollection.Find(u => u.Email == request.Email).Any())
            {
                return BadRequest("Email is already registered");
            }

            if (_userCollection.Find(u => u.Phone == request.Phone).Any())
            {
                return BadRequest("Phone number is already registered");
            }

            var user = new User
            {
                Name = request.Name,
                Address = request.Address,
                Email = request.Email,
                Phone = request.Phone,
                Password = HashPassword(request.Password)
            };

            _userCollection.InsertOne(user);

            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var emailFilter = Builders<User>.Filter.Eq(x => x.Email, request.Email);
            var isActiveFilter = Builders<User>.Filter.Eq(x => x.IsActive, true);

            var user = await _userCollection
                .Find(emailFilter)
                .Project(x => new {
                    Id = x.Id,
                    Email = x.Email,
                    Password = x.Password
                })
                .FirstOrDefaultAsync();

            if (user is null)
            {
                return Unauthorized("user is null");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized();
            }
            string token = JwtTokenHelper.GenerateToken(user.Id, user.Email);

            var filter = Builders<User>.Filter.Eq(x => x.Id, user.Id);

            var update = Builders<User>.Update
                .Set(x => x.Token, token)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            AuthHelper.User authUser = new AuthHelper.User
            {
                Id = user.Id,
                Email = user.Email,
            };

            await _userCollection.UpdateOneAsync(filter, update);


            return Ok(new
            {
                Token = token,
            });
        }
   
        private string HashPassword(string password)
        {
            string salt = BCrypt.Net.BCrypt.GenerateSalt();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }

        public class BaseInputModel
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
        }

        public class LoginRequest : BaseInputModel { }

        public class RegisterRequest : BaseInputModel {
            public string? Name { get; set; }
            public string? Address { get; set; }
            public string? Phone { get; set; }
        }

        public class LoginRequestValidator : AbstractValidator<LoginRequest>
        {
            public LoginRequestValidator()
            {
                RuleFor(x => x.Email).NotEmpty();
                RuleFor(x=>x.Password).NotEmpty();
            }
        }

        public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
        {
            public RegisterRequestValidator(DbHelper dbHelper,AuthHelper authHelper)
            {
                AuthHelper.User? authUser = authHelper.GetUser();

                RuleFor(x => x.Email).EmailAddress()
                    .MustBeUnique(dbHelper, x => x.Email, Builders<User>.Filter.Ne(x => x.Id, authUser?.Id))
                    .Unless(x=>string.IsNullOrEmpty(x.Email));

            }
        }
    }
}