namespace api.Controllers
{
    using api.Data;
    using api.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using MongoDB.Driver;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IConfiguration _config;

        public AuthController(IDatabaseSettings databaseSettings, IMongoClient mongoClient, IConfiguration config)
        {
            this._config = config;
            IMongoDatabase database = mongoClient.GetDatabase(databaseSettings.DatabaseName);
            this._usersCollection = database.GetCollection<User>(databaseSettings.UserCollectionName);
        }

        // POST: api/<AuthController>/user
        [HttpGet("user")]
        [Authorize]
        public IActionResult GetAuthUser()
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                User user = this._usersCollection.Find(u => u.Id == userId).FirstOrDefault();

                if (user == null)
                {
                    throw new Exception("something went wrong");
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // POST: api/<AuthController>/register
        [HttpPost("register")]
        public IActionResult Register(UserRegister user)
        {
            try
            {
                // Check whether password and confirm password are equals
                if (user.Password != user.ConfirmPassword)
                {
                    return BadRequest(new { message = "Enter the same password" });
                }

                // Check whether user exists
                bool userExist = this._usersCollection.Find(u => u.Email == user.Email).FirstOrDefault() != null;

                if (userExist)
                {
                    return BadRequest(new { message = "User with this email already exists" });
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                User newUser = new User(user.Email, user.Username, hashedPassword);
                string uId = Guid.NewGuid().ToString("N").Substring(0, 24);
                newUser.Id = uId;

                this._usersCollection.InsertOne(newUser);

                return StatusCode(201, new { message = "You have been registered successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // POST: api/<AuthController>/login
        [HttpPost("login")]
        public IActionResult Login(UserLogin userLogin)
        {
            try
            {
                object invalidEmailOrPassword = new
                {
                    message = "Wrong email or password"
                };
                User? user = this._usersCollection.Find(u => u.Email == userLogin.Email).FirstOrDefault();

                // Invalid email
                if (user == null)
                {
                    return NotFound(invalidEmailOrPassword);
                }

                bool isValidPassword = BCrypt.Net.BCrypt.Verify(userLogin.Password, user.Password);

                // Invalid password
                if (!isValidPassword)
                {
                    return NotFound(invalidEmailOrPassword);
                }

                string jwtToken = this.GenerateJwt(user);

                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Expires = new DateTimeOffset(DateTime.Now.AddDays(1));
                cookieOptions.HttpOnly = true;

                Response.Cookies.Append("token", jwtToken, cookieOptions);
                return Ok(new { message = "You have been logged in successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // POST: api/<AuthController>/logout
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(-1);

                Response.Cookies.Append("token", "", cookieOptions);    
                return Ok(new { message = "You have been logged out successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        private string GenerateJwt(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("_id", user.Id)
            };

            var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddMinutes(60), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
