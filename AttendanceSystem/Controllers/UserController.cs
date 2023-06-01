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

            return Ok(new { Token = token, Message="login Sucessfull" });
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        private string GenerateToken(User user)
        {
            string secretKey = "your_secret_key_here";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
        private string HashPassword(string password)
        {
            string salt = BCrypt.Net.BCrypt.GenerateSalt();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }
    }
}