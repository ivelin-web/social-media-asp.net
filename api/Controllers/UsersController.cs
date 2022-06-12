namespace api.Controllers
{
    using api.Data;
    using api.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using System.Security.Claims;

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IConfiguration _config;

        public UsersController(IDatabaseSettings databaseSettings, IMongoClient mongoClient, IConfiguration config)
        {
            this._config = config;
            IMongoDatabase database = mongoClient.GetDatabase(databaseSettings.DatabaseName);
            this._usersCollection = database.GetCollection<User>(databaseSettings.UserCollectionName);
        }

        // GET: api/<UsersController>?id/username=<some value>
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            try
            {
                string? username = Request.Query["username"];
                string? id = Request.Query["id"];

                // Find user by username
                if (username != null)
                {
                    User user = this._usersCollection.Find(u => u.Username == username).FirstOrDefault();

                    return Ok(user);
                }
                // Find user by id
                else
                {
                    User user = this._usersCollection.Find(u => u.Id == id).FirstOrDefault();

                    return Ok(user);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // GET: api/<UsersController>/search?username=<some value>
        [HttpGet("search")]
        [Authorize]
        public IActionResult GetAllByUsername()
        {
            try
            {
                string? username = Request.Query["username"];
                var filter = Builders<User>.Filter.Regex("username", new BsonRegularExpression(username, "i"));
                List<User>? users = this._usersCollection.Find(filter).ToList<User>();

                return Ok(users);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // GET: api/<UsersController>/friends/{id}
        [HttpGet("friends/{id}")]
        [Authorize]
        public IActionResult GetFriends(string id)
        {
            try
            {
                User user = this._usersCollection.Find(u => u.Id == id).FirstOrDefault();

                List<User> friends = user.Following
                    .Select(id => this._usersCollection.Find(u => u.Id == id).FirstOrDefault()).ToList<User>();

                return Ok(friends);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // DELETE: api/<UsersController>/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(string id)
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                if (userId != id)
                {
                    return StatusCode(403, new { message = "You can delete only your account" });
                }

                var result = this._usersCollection.DeleteOne(u => u.Id == userId);

                return Ok(new { message = "Account has been deleted successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UsersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
    }
}
