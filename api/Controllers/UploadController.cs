namespace api.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http.Headers;

    [ApiController]
    public class UploadController : ControllerBase
    {
        // POST: api/<UploadController>
        [HttpPost]
        [Route("api/users/[controller]")]
        [Authorize]

        public IActionResult UploadUserImage()
        {
            try
            {
                return this.UploadFile("person");
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        // POST: api/<UploadController>
        [HttpPost]
        [Route("api/posts/[controller]")]
        [Authorize]
        public IActionResult UploadPostImage()
        {
            try
            {
                return this.UploadFile("post");
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = e.Message });
            }
        }

        private IActionResult UploadFile(string entity)
        {
            IFormFile file = Request.Form.Files[0];
            string? folderName = Path.Combine("Resources", "images", entity);
            string? pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            if (file.Length > 0)
            {
                string? fileName = Guid.NewGuid().ToString("N") + "-" +
                    ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string? fullPath = Path.Combine(pathToSave, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return Ok(new { message = "File uploaded", fileName = fileName });
            }
            else
            {
                return BadRequest(new { message = "Please upload a valid file" });
            }
        }
    }
}
