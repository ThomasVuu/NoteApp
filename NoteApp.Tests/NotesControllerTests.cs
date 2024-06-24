using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteApp.Controllers;
using NoteApp.Data;
using NoteApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class NotesControllerTests
{
    private readonly NotesContext _context;
    private readonly NotesController _controller;
    private readonly ITestOutputHelper _output;

    public NotesControllerTests(ITestOutputHelper output)
    {
        var options = new DbContextOptionsBuilder<NotesContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new NotesContext(options);
        _controller = new NotesController(_context);
        _output = output;
    }

    [Fact]
    public async Task GetNotes_ReturnsEmptyList_WhenNoNotes()
    {
        // Act
        var result = await _controller.GetNotes();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Note>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var notes = Assert.IsAssignableFrom<IEnumerable<Note>>(okResult.Value);
        Assert.Empty(notes);
    }

    [Fact]
    public async Task PostNote_CreatesNewNote()
    {
        // Arrange
        var newNote = new Note { Content = "Test Note" };

        // Act
        var result = await _controller.PostNote(newNote);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Note>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var note = Assert.IsType<Note>(createdAtActionResult.Value);
        Assert.Equal("Test Note", note.Content);
        Assert.NotEqual(default(DateTime), note.CreatedDate);
        Assert.NotEqual(default(DateTime), note.LastModifiedDate);
    }

    [Fact]
    public async Task GetNote_ReturnsNotFound_WhenNoteDoesNotExist()
    {
        // Act
        var result = await _controller.GetNote(1);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetNote_ReturnsNote_WhenNoteExists()
    {
        // Arrange
        var note = new Note { Content = "Existing Note", CreatedDate = DateTime.Now, LastModifiedDate = DateTime.Now };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetNote(note.NoteId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Note>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedNote = Assert.IsType<Note>(okResult.Value);
        Assert.Equal(note.NoteId, returnedNote.NoteId);
        Assert.Equal(note.Content, returnedNote.Content);
    }

    [Fact]
    public async Task PostNote_ReturnsBadRequest_WhenNoteIsInvalid()
    {
        // Arrange
        var invalidNote = new Note { Content = null }; // Invalid note

        // Act
        var result = await _controller.PostNote(invalidNote);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Note>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var validationErrors = Assert.IsType<SerializableError>(badRequestResult.Value);
        Assert.True(validationErrors.ContainsKey("Content"));
    }


    [Fact]
    public async Task PutNote_ReturnsNotFound_WhenNoteDoesNotExist()
    {
        // Arrange
        var note = new Note { NoteId = 1, Content = "Non-Existent Note" };

        // Act
        var result = await _controller.PutNote(note.NoteId, note);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PutNote_UpdatesNote_WhenNoteExists()
    {
        // Arrange
        var note = new Note { Content = "Original Note", CreatedDate = DateTime.Now, LastModifiedDate = DateTime.Now };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        var updatedNote = new Note { Content = "Updated Note" };

        // Act
        var result = await _controller.PutNote(note.NoteId, updatedNote);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dbNote = Assert.IsType<Note>(okResult.Value);
        Assert.Equal("Updated Note", dbNote.Content);
    }



    [Fact]
    public async Task DeleteNote_DeletesExistingNote()
    {
        // Arrange
        var note = new Note { Content = "Note to Delete", CreatedDate = DateTime.Now, LastModifiedDate = DateTime.Now };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteNote(note.NoteId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Null(await _context.Notes.FindAsync(note.NoteId));
    }

    [Fact]
    public async Task DeleteNote_ReturnsNotFound_WhenNoteDoesNotExist()
    {
        // Act
        var result = await _controller.DeleteNote(1);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PostNote_ReturnsBadRequest_WhenContentIsEmpty()
    {
        // Arrange
        var newNote = new Note { Content = "" };

        // Act
        var result = await _controller.PostNote(newNote);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Note>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        var validationErrors = Assert.IsType<SerializableError>(badRequestResult.Value);
        Assert.True(validationErrors.ContainsKey("Content"));
    }

    [Fact]
    public async Task PutNote_ReturnsBadRequest_WhenContentIsEmpty()
    {
        // Arrange
        var note = new Note { Content = "Original Note", CreatedDate = DateTime.Now, LastModifiedDate = DateTime.Now };
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        var updatedNote = new Note { NoteId = note.NoteId, Content = "" }; // Invalid updated note

        // Act
        var result = await _controller.PutNote(note.NoteId, updatedNote);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var validationErrors = Assert.IsType<SerializableError>(badRequestResult.Value);
        Assert.True(validationErrors.ContainsKey("Content"));
    }
}
