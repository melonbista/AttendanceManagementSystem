using AttendanceSystem.Model;
using AttendanceSystem.Models.Dto;
using AttendanceSystem.Settings;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;
        public UsersController(MongoDbContext dbContext)
        {
            _userCollection = dbContext.Users;
        }

        [HttpPost("register")]
        public IActionResult RegisterUser([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request data");
            }

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

            return Ok(token);
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
    }
}