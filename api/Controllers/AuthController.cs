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

        // POST: api/<AuthController>/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegister user)
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

            return Ok(new { message = "You have been logged in successfully", token = jwtToken });
        }

        // GET api/<AuthController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AuthController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<AuthController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AuthController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
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
