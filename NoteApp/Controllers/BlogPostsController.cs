using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteApp.Data;
using NoteApp.Models;
using NoteApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoteApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly NotesContext _context;
        private readonly BlogPostService _blogPostService;

        public BlogPostsController(NotesContext context, BlogPostService blogPostService)
        {
            _context = context;
            _blogPostService = blogPostService;
        }

        // GET: api/BlogPosts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
        {
            return await _context.BlogPosts.ToListAsync();
        }

        // GET: api/BlogPosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return Ok(blogPost);
        }

        // POST: api/BlogPosts
        [HttpPost]
        public async Task<ActionResult<BlogPost>> PostBlogPost(BlogPost blogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            blogPost.CreatedDate = DateTime.Now;
            blogPost.LastModifiedDate = DateTime.Now;
            _context.BlogPosts.Add(blogPost);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.BlogPostId }, blogPost);
        }

        // PUT: api/BlogPosts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlogPost(int id, [FromBody] BlogPost updatedBlogPost)
        {
            if (id != updatedBlogPost.BlogPostId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingBlogPost = await _context.BlogPosts.FindAsync(id);

            if (existingBlogPost == null)
            {
                return NotFound();
            }

            existingBlogPost.Content = updatedBlogPost.Content;
            existingBlogPost.LastModifiedDate = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogPostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(existingBlogPost);
        }

        // DELETE: api/BlogPosts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/BlogPosts/Generate
        [HttpPost("Generate")]
        public async Task<ActionResult<BlogPost>> GenerateBlogPost([FromBody] TimeRange timeRange)
        {
            var notes = await _context.Notes
                                      .Where(n => n.CreatedDate >= timeRange.StartTime && n.CreatedDate <= timeRange.EndTime)
                                      .ToListAsync();

            if (notes == null || !notes.Any())
            {
                return NotFound("No notes found for the specified date range.");
            }

            var blogPost = await _blogPostService.GenerateAndSaveBlogPostAsync(notes);

            return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.BlogPostId }, blogPost);
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.BlogPostId == id);
        }
    }

    public class TimeRange
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
