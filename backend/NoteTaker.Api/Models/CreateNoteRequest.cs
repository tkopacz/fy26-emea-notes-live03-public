namespace NoteTaker.Api.Models;

/// <summary>
/// Request body for creating a new note.
/// </summary>
/// <param name="Content">Plain-text content of the note. Must contain at least one non-whitespace character.</param>
public record CreateNoteRequest(string Content);
