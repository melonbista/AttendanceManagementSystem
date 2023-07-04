using AttendanceManagementSystem.Helper;
using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AttendanceManagementSystem.Authentication
{
    public class CustomAuthenticationHandler : AuthenticationSchemeOptions
    {

    }
    public class UserAuthenticationHandler : AuthenticationHandler<CustomAuthenticationHandler>
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly AuthHelper _authHelper;

        public UserAuthenticationHandler(
            IOptionsMonitor<CustomAuthenticationHandler> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            DbHelper dbHelper,
            AuthHelper authHelper)
            : base(options, logger, encoder, clock)
        {
            _userCollection = dbHelper.GetCollection<User>();
            _authHelper = authHelper;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("X-Auth"))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            string token = Request.Headers["X-Auth"];
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            try
            {
                return await ValidateToken(token);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex.Message);
            }
        }

        private async Task<AuthenticateResult> ValidateToken(string token)
        {
            var tokenFilter = Builders<User>.Filter.Eq(x => x.Token, token);
            var isActiveFilter = Builders<User>.Filter.Eq(x => x.IsActive, true);

            var user = await _userCollection.Find(tokenFilter & isActiveFilter)
                .Project(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    Email = x.Email,
                    Phone = x.Phone
                }).FirstOrDefaultAsync();

            if (user is null) {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new System.Security.Principal.GenericPrincipal(identity, null);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            AuthHelper.User authUser = new AuthHelper.User
            {
                Id = user.Id,
                Name = user.Name,
                Address = user.Address,
                Email = user.Email,
                Phone = user.Phone
            };

            _authHelper.SetUser(authUser);

            return AuthenticateResult.Success(ticket);
        }

    }
}
