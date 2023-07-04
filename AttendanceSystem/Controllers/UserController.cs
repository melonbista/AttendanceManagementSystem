using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Models.Dto;
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
            Console.WriteLine("BEfore");
            //if (!ModelState.IsValid)
            //{
            //    Console.WriteLine("after 123");

            //    return BadRequest("Invalid request data");
            //}
            Console.WriteLine("after");

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
        public IActionResult Login([FromBody] LoginRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request data");
            }

            var user = _userCollection.Find(u => u.Email == request.Email).FirstOrDefault();

            if (user == null || !VerifyPassword(user.Password, request.Password))
            {
                return Unauthorized("Invalid email or password");
            }

            var token = GenerateToken(user);

            return Ok(new
            {
                Token = token,
            });
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        private string GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1swek3u4uo2u4a6e"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "https://localhost:5001",
                audience: "https://localhost:5001",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;

        }
        private string HashPassword(string password)
        {
            string salt = BCrypt.Net.BCrypt.GenerateSalt();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }

        public class BaseInputModel
        {
            public string? Name { get; set; }
            public string? Address { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? Password { get; set; }
        }

        public class RegisterRequest : BaseInputModel { }

        public class UserValidator : AbstractValidator<RegisterRequest>
        {
            public UserValidator()
            {
                RuleFor(x => x.Phone).MustBeNumber(10).NotEmpty();
                RuleFor(x => x.Email).EmailAddress().NotEmpty();
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
            }
        }
    }
}