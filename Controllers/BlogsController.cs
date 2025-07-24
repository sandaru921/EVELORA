using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using AssessmentPlatform.Backend.Repositories;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AssessmentPlatform.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly IConfiguration _configuration;
        private readonly IBlogRepository _blogRepository;

        public BlogsController(
            AppDbContext context,
            BlobServiceClient blobServiceClient,
            IConfiguration configuration,
            IBlogRepository blogRepository)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _blogRepository = blogRepository;
            _containerName = configuration["AzureBlobStorage:BlogContainerName"]
                ?? throw new InvalidOperationException("AzureBlobStorage:BlogContainerName is not configured.");
        }

        // GET: api/Blogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogs()
        {
            return Ok(await _blogRepository.GetAllBlogsAsync());
        }

        // GET: api/Blogs/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Blog>> GetBlog(int id)
        {
            var blog = await _blogRepository.GetBlogByIdAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return Ok(blog);
        }

        // GET: api/Blogs/slug/how-to-tackle-security-testing
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<Blog>> GetBlogBySlug(string slug)
        {
            var blog = await _blogRepository.GetBlogBySlugAsync(slug);
            if (blog == null)
            {
                return NotFound();
            }
            return Ok(blog);
        }

        // GET: api/Blogs/category/Software
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogsByCategory(string category)
        {
            return Ok(await _blogRepository.GetBlogsByCategoryAsync(category));
        }

        // POST: api/Blogs
        [HttpPost]
        public async Task<ActionResult<Blog>> PostBlog([FromForm] BlogCreateDto blogDto)
        {
            // Field-specific validation for required fields
            if (string.IsNullOrWhiteSpace(blogDto.Title))
            {
                return BadRequest("Title is required.");
            }
            if (string.IsNullOrWhiteSpace(blogDto.Content))
            {
                return BadRequest("Content is required.");
            }
            if (string.IsNullOrWhiteSpace(blogDto.Category))
            {
                return BadRequest("Category is required.");
            }

            // Validate image
            if (blogDto.Image != null)
            {
                if (!new[] { "image/jpeg", "image/png", "image/gif" }.Contains(blogDto.Image.ContentType))
                {
                    return BadRequest("Only JPEG, PNG, and GIF images are allowed.");
                }
                if (blogDto.Image.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return BadRequest("Image size must be less than 5MB.");
                }
            }

            var blog = new Blog
            {
                Title = blogDto.Title,
                Content = blogDto.Content,
                Category = blogDto.Category,
                CreatedAt = DateTime.UtcNow
            };
            blog.GenerateSlug();

            // Ensure slug is unique
            int suffix = 1;
            string baseSlug = blog.Slug;
            while (await _context.Blogs.AnyAsync(b => b.Slug == blog.Slug && b.Id != blog.Id))
            {
                blog.Slug = $"{baseSlug}-{suffix++}";
            }

            if (blogDto.Image != null)
            {
                try
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                    await containerClient.CreateIfNotExistsAsync();
                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(blogDto.Image.FileName)}";
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    using (var stream = blogDto.Image.OpenReadStream())
                    {
                        var blobHttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = blogDto.Image.ContentType
                        };
                        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                    }

                    blog.ImageUrl = blobClient.Uri.ToString();
                }
                catch (Azure.RequestFailedException ex)
                {
                    return StatusCode(500, $"Error uploading image to Blob Storage: {ex.Message}");
                }
            }

            var createdBlog = await _blogRepository.CreateBlogAsync(blog);
            return CreatedAtAction("GetBlog", new { id = createdBlog.Id }, createdBlog);
        }

        // PUT: api/Blogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlog(int id, [FromForm] BlogUpdateDto blogDto)
        {
            var blog = await _blogRepository.GetBlogByIdAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            // Field-specific validation for required fields
            if (string.IsNullOrWhiteSpace(blogDto.Title))
            {
                return BadRequest("Title is required.");
            }
            if (string.IsNullOrWhiteSpace(blogDto.Content))
            {
                return BadRequest("Content is required.");
            }
            if (string.IsNullOrWhiteSpace(blogDto.Category))
            {
                return BadRequest("Category is required.");
            }

            // Validate image
            if (blogDto.Image != null)
            {
                if (!new[] { "image/jpeg", "image/png", "image/gif" }.Contains(blogDto.Image.ContentType))
                {
                    return BadRequest("Only JPEG, PNG, and GIF images are allowed.");
                }
                if (blogDto.Image.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return BadRequest("Image size must be less than 5MB.");
                }
            }

            blog.Title = blogDto.Title;
            blog.Content = blogDto.Content;
            blog.Category = blogDto.Category;
            blog.UpdatedAt = DateTime.UtcNow;
            blog.GenerateSlug();

            // Ensure slug is unique
            int suffix = 1;
            string baseSlug = blog.Slug;
            while (await _context.Blogs.AnyAsync(b => b.Slug == blog.Slug && b.Id != blog.Id))
            {
                blog.Slug = $"{baseSlug}-{suffix++}";
            }

            if (blogDto.Image != null)
            {
                try
                {
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                    await containerClient.CreateIfNotExistsAsync();

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(blog.ImageUrl))
                    {
                        try
                        {
                            var oldBlobUri = new Uri(blog.ImageUrl);
                            var oldBlobName = oldBlobUri.Segments.Last(); // More robust than Path.GetFileName
                            var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                            await oldBlobClient.DeleteIfExistsAsync();
                        }
                        catch (Azure.RequestFailedException ex)
                        {
                            // Log the error but continue with the update
                            Console.WriteLine($"Error deleting old image: {ex.Message}");
                        }
                    }

                    // Upload new image
                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(blogDto.Image.FileName)}";
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    using (var stream = blogDto.Image.OpenReadStream())
                    {
                        var blobHttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = blogDto.Image.ContentType
                        };
                        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                    }

                    blog.ImageUrl = blobClient.Uri.ToString();
                }
                catch (Azure.RequestFailedException ex)
                {
                    return StatusCode(500, $"Error updating image in Blob Storage: {ex.Message}");
                }
            }

            var success = await _blogRepository.UpdateBlogAsync(blog);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        
    [HttpDelete("{id}")]
public async Task<IActionResult> DeleteBlog(int id)
{
    var blog = await _blogRepository.GetBlogByIdAsync(id);
    if (blog == null)
    {
        return NotFound(new { Message = $"Blog with ID {id} not found." });
    }

    // Delete image from Blob Storage if it's a valid Blob Storage URL
    if (!string.IsNullOrEmpty(blog.ImageUrl))
    {
        try
        {
            // Check if ImageUrl is a Blob Storage URL
            if (blog.ImageUrl.StartsWith("https://") && blog.ImageUrl.Contains(_containerName))
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobUri = new Uri(blog.ImageUrl);
                var blobName = string.Join("", blobUri.Segments.Skip(blobUri.Segments.Length - 1)); // Get the blob name
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            // If ImageUrl is a local path (legacy), skip deletion
            else
            {
                Console.WriteLine($"Skipping deletion of non-Blob Storage image: {blog.ImageUrl}");
            }
        }
        catch (Exception ex) // Catch all exceptions, not just Azure.RequestFailedException
        {
            // Log the error but continue with blog deletion
            Console.WriteLine($"Error deleting image: {ex.Message}");
        }
    }

    var success = await _blogRepository.DeleteBlogAsync(id);
    if (!success)
    {
        return NotFound(new { Message = $"Blog with ID {id} not found." });
    }

    return NoContent();
}




    }
}

    
    public class BlogCreateDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Content is required")]
        [MinLength(1, ErrorMessage = "Content cannot be empty")]
        public string Content { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Category is required")]
        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [MinLength(1, ErrorMessage = "Category cannot be empty")]
        public string Category { get; set; } = string.Empty;
        
        public IFormFile? Image { get; set; }
    }

    public class BlogUpdateDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Content is required")]
        [MinLength(1, ErrorMessage = "Content cannot be empty")]
        public string Content { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Category is required")]
        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [MinLength(1, ErrorMessage = "Category cannot be empty")]
        public string Category { get; set; } = string.Empty;
        
        public IFormFile? Image { get; set; }
    }

