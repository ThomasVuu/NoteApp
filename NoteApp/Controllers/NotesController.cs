using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteApp.Data;
using NoteApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoteApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly NotesContext _context;

        public NotesController(NotesContext context)
        {
            _context = context;
        }

        // GET: api/Notes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
        {
            var notes = await _context.Notes.ToListAsync();
            return Ok(notes);
        }

        // GET: api/Notes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            return Ok(note);
        }

        // POST: api/Notes
        [HttpPost]
        public async Task<ActionResult<Note>> PostNote(Note note)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(note.Content))
            {
                ModelState.AddModelError("Content", "Content is required.");
                return BadRequest(ModelState);
            }

            note.CreatedDate = DateTime.Now;
            note.LastModifiedDate = DateTime.Now;
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNote), new { id = note.NoteId }, note);
        }

        // PUT: api/Notes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNote(int id, [FromBody] Note updatedNote)
        {

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(updatedNote.Content))
            {
                ModelState.AddModelError("Content", "Content is required.");
                return BadRequest(ModelState);
            }

            var existingNote = await _context.Notes.FindAsync(id);

            if (existingNote == null)
            {
                return NotFound();
            }

            existingNote.Content = updatedNote.Content;
            existingNote.LastModifiedDate = DateTime.Now;

            // Log the entity state before saving
            Console.WriteLine($"Before Save - Entity State: {_context.Entry(existingNote).State}");
            Console.WriteLine($"Before Save - LastModifiedDate: {existingNote.LastModifiedDate}");


            try
            {
                await _context.SaveChangesAsync();
                // Log the entity state after saving
                Console.WriteLine($"After Save - Entity State: {_context.Entry(existingNote).State}");
                Console.WriteLine($"After Save - LastModifiedDate: {existingNote.LastModifiedDate}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(existingNote);
        }

        // DELETE: api/Notes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.NoteId == id);
        }
    }
}
