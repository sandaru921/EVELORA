using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// using EveloraBlogAPI.Data;
// using EveloraBlogAPI.Models;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BlogsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/Blogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogs()
        {
            return await _context.Blogs
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        // GET: api/Blogs/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Blog>> GetBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }

        // GET: api/Blogs/slug/how-to-tackle-security-testing
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<Blog>> GetBlogBySlug(string slug)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(b => b.Slug == slug);

            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }

        // GET: api/Blogs/category/Software
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogsByCategory(string category)
        {
            return await _context.Blogs
                .Where(b => b.Category == category)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        // POST: api/Blogs
        [HttpPost]
        public async Task<ActionResult<Blog>> PostBlog([FromForm] BlogCreateDto blogDto)
        {
            var blog = new Blog
            {
                Title = blogDto.Title,
                Content = blogDto.Content,
                Category = blogDto.Category,
                CreatedAt = DateTime.UtcNow
            };
            
            // Generate slug from title
            blog.GenerateSlug();

            if (blogDto.Image != null)
            {
                // Save image to server
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "blogs");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + blogDto.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                // Ensure directory exists
                Directory.CreateDirectory(uploadsFolder);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await blogDto.Image.CopyToAsync(fileStream);
                }

                // Save image path to database
                blog.ImageUrl = $"/images/blogs/{uniqueFileName}";
            }

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBlog", new { id = blog.Id }, blog);
        }

        // PUT: api/Blogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlog(int id, [FromForm] BlogUpdateDto blogDto)
        {
            var blog = await _context.Blogs.FindAsync(id); // Fetch the blog from the database
            if (blog == null)
            {
                return NotFound();
            }

            // Update the blog properties
            blog.Title = blogDto.Title;
            blog.Content = blogDto.Content;
            blog.Category = blogDto.Category;
            blog.UpdatedAt = DateTime.UtcNow;

            // Regenerate slug from updated title
            blog.GenerateSlug();

            if (blogDto.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(blog.ImageUrl))
                {
                    string oldImagePath = Path.Combine(_environment.WebRootPath, blog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "blogs");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + blogDto.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await blogDto.Image.CopyToAsync(fileStream);
                }

                blog.ImageUrl = $"/images/blogs/{uniqueFileName}";
            }

            try
            {
                await _context.SaveChangesAsync(); // Save changes to the database
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Blogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            // Delete image if exists
            if (!string.IsNullOrEmpty(blog.ImageUrl))
            {
                string imagePath = Path.Combine(_environment.WebRootPath, blog.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }

    public class BlogCreateDto
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string Category { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class BlogUpdateDto
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string Category { get; set; }
        public IFormFile? Image { get; set; }
    }
}
