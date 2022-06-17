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
                    User user = this._usersCollection.Find(u => u._id == id).FirstOrDefault();

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
                User user = this._usersCollection.Find(u => u._id == id).FirstOrDefault();

                List<User> friends = user.Following
                    .Select(id => this._usersCollection.Find(u => u._id == id).FirstOrDefault()).ToList<User>();

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

                var result = this._usersCollection.DeleteOne(u => u._id == userId);

                return Ok(new { message = "Account has been deleted successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // PUT: api/<UsersController>/{id}
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(string id, UserUpdate user)
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                if (userId != id)
                {
                    return StatusCode(403, new { message = "You can update only your account" });
                }

                User updatedUser = this._usersCollection.Find(u => u._id == userId).FirstOrDefault();

                // Update user properties from request body
                this.UpdateUserProperties(updatedUser, user);
                this._usersCollection.ReplaceOne(u => u._id == userId, updatedUser);

                return Ok(updatedUser);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // PUT: api/<UsersController>/{id}/follow
        [HttpPut("{id}/follow")]
        [Authorize]
        public IActionResult Follow(string id)
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                if (userId == id)
                {
                    return StatusCode(403, new { message = "You can't follow yourself" });
                }

                User currentUser = this._usersCollection.Find(u => u._id == userId).FirstOrDefault();
                User otherUser = this._usersCollection.Find(u => u._id == id).FirstOrDefault();

                bool isFollowed = otherUser.Followers.Where(uId => uId == userId).FirstOrDefault() != null;

                if (isFollowed)
                {
                    return StatusCode(403, new { message = "You already follow this user" });
                }

                // Add other user to current user followings
                List<string> following = currentUser.Following;
                following.Add(id);

                // Add current user to other user followers
                List<string> followers = otherUser.Followers;
                followers.Add(userId);

                // Update current user following
                var updateCurrent = Builders<User>.Update.Set("following", following);
                this._usersCollection.UpdateOne(u => u._id == userId, updateCurrent);

                // Update other user followers
                var updateOther = Builders<User>.Update.Set("followers", followers);
                this._usersCollection.UpdateOne(u => u._id == id, updateOther);

                return Ok(new { message = "User has been followed successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // PUT: api/<UsersController>/{id}/unfollow
        [HttpPut("{id}/unfollow")]
        [Authorize]
        public IActionResult Unfollow(string id)
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                if (userId == id)
                {
                    return StatusCode(403, new { message = "You can't unfollow yourself" });
                }

                User currentUser = this._usersCollection.Find(u => u._id == userId).FirstOrDefault();
                User otherUser = this._usersCollection.Find(u => u._id == id).FirstOrDefault();

                bool isFollowed = otherUser.Followers.Where(uId => uId == userId).FirstOrDefault() != null;

                if (!isFollowed)
                {
                    return StatusCode(403, new { message = "You don't follow this user" });
                }

                // Remove other user to current user followings
                List<string> following = currentUser.Following;
                following.Remove(id);

                // Remove current user to other user followers
                List<string> followers = otherUser.Followers;
                followers.Remove(userId);

                // Update current user following
                var updateCurrent = Builders<User>.Update.Set("following", following);
                this._usersCollection.UpdateOne(u => u._id == userId, updateCurrent);

                // Update other user followers
                var updateOther = Builders<User>.Update.Set("followers", followers);
                this._usersCollection.UpdateOne(u => u._id == id, updateOther);

                return Ok(new { message = "User has been unfollowed successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        private void UpdateUserProperties(User updatedUser, UserUpdate user)
        {
            var destinationProperties = updatedUser.GetType().GetProperties();

            foreach (var sourceProp in user.GetType().GetProperties())
            {
                foreach (var destProp in destinationProperties)
                {
                    if (sourceProp.Name != destProp.Name)
                    {
                        continue;
                    }

                    if (sourceProp.GetValue(user) == null)
                    {
                        continue;
                    }

                    if (sourceProp.Name == "Password")
                    {
                        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
                        user.Password = hashedPassword;
                    }

                    destProp.SetValue(updatedUser, sourceProp.GetValue(user));
                }
            }
        }
    }
}