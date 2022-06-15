namespace api.Controllers
{
    using api.Data;
    using api.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Driver;
    using System.Linq;
    using System.Security.Claims;

    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Post> _postsCollection;
        private readonly IConfiguration _config;

        public PostsController(IDatabaseSettings databaseSettings, IMongoClient mongoClient, IConfiguration config)
        {
            this._config = config;
            IMongoDatabase database = mongoClient.GetDatabase(databaseSettings.DatabaseName);
            this._usersCollection = database.GetCollection<User>(databaseSettings.UserCollectionName);
            this._postsCollection = database.GetCollection<Post>(databaseSettings.PostCollectionName);
        }

        // GET: api/<PostsController>/profile/{username}
        [HttpGet("profile/{username}")]
        [Authorize]
        public IActionResult GetPostsByUsername(string username)
        {
            try
            {
                User user = this._usersCollection.Find(u => u.Username == username).FirstOrDefault();
                List<Post> posts = this._postsCollection.Find(p => p.UserId == user.Id).ToList();

                return Ok(posts);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // GET: api/<PostsController>/{id}
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(string id)
        {
            try
            {
                List<Post> posts = this._postsCollection.Find(p => p.Id == id).ToList();

                return Ok(posts);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // POST: api/<PostsController>
        [HttpPost]
        [Authorize]
        public IActionResult Create(Post post)
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                // Check whether at least one of the properties mentioned below are in the request body
                if (post.Description == null && post.Img == null)
                {
                    throw new Exception("Description or Image is required");
                }

                post.UserId = userId;
                this._postsCollection.InsertOne(post);

                return Ok(post);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // PUT: api/<PostsController>/{id}
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(string id, PostUpdate post)
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                Post updatedPost = this._postsCollection.Find(p => p.Id == id).FirstOrDefault();

                if (updatedPost.UserId != userId)
                {
                    return StatusCode(403, new { message = "You can update only your posts" });
                }

                // Update post properties from request body
                this.UpdatePostProperties(updatedPost, post);
                this._postsCollection.ReplaceOne(p => p.Id == updatedPost.Id, updatedPost);

                return Ok(updatedPost);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // DELETE: api/<PostsController>/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(string id)
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                Post post = this._postsCollection.Find(p => p.Id == id).FirstOrDefault();

                if (post.UserId != userId)
                {
                    return StatusCode(403, new { message = "You can delete only your posts" });
                }

                var result = this._postsCollection.DeleteOne(p => p.Id == post.Id);

                return Ok(new { message = "Post has been deleted successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // PUT: api/<PostsController>/{id}/like
        [HttpPut("{id}/like")]
        [Authorize]
        public IActionResult Like(string id)
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                Post post = this._postsCollection.Find(p => p.Id == id).FirstOrDefault();
                bool isPostLiked = post.Likes.Any(uId => uId == userId);

                // Dislike post 
                if (isPostLiked)
                {
                    post.Likes.Remove(userId);
                    var updateDislike = Builders<Post>.Update.Set("likes", post.Likes);
                    this._postsCollection.UpdateOne(p => p.Id == id, updateDislike);

                    return Ok(new { message = "Post has been disliked successfully" });
                }

                post.Likes.Add(userId);
                var updateLike = Builders<Post>.Update.Set("likes", post.Likes);
                this._postsCollection.UpdateOne(p => p.Id == id, updateLike);

                return Ok(new { message = "Post has been liked successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // GET: api/<PostsController>/timeline
        [HttpGet("timeline")]
        [Authorize]
        public IActionResult Timeline()
        {
            try
            {
                // Get user id from jwt payload
                ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                string userId = identity.Claims.FirstOrDefault(o => o.Type == "_id").Value;

                User currentUser = this._usersCollection.Find(u => u.Id == userId).FirstOrDefault();
                List<Post> userPosts = this._postsCollection.Find(p => p.UserId == userId).ToList();
                List<Post> friendPosts = new List<Post>();

                // Add all friend posts
                foreach (var friendId in currentUser.Following)
                {
                    Post newPost = this._postsCollection.Find(p => p.UserId == friendId).FirstOrDefault();

                    if (newPost == null)
                    {
                        continue;
                    }

                    friendPosts.Add(newPost);
                }

                List<Post> allPosts = userPosts.Concat(friendPosts).ToList();

                return Ok(allPosts);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        private void UpdatePostProperties(Post updatedPost, PostUpdate post)
        {
            var destinationProperties = updatedPost.GetType().GetProperties();

            foreach (var sourceProp in post.GetType().GetProperties())
            {
                foreach (var destProp in destinationProperties)
                {
                    if (sourceProp.Name != destProp.Name)
                    {
                        continue;
                    }

                    if (sourceProp.GetValue(post) == null)
                    {
                        continue;
                    }

                    destProp.SetValue(updatedPost, sourceProp.GetValue(post));
                }
            }
        }
    }
}
