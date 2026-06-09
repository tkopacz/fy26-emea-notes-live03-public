namespace NoteTaker.Wpf.Models;

/// <summary>
/// Request model for creating a new note via the REST API.
/// </summary>
/// <param name="Content">Plain-text content of the note. Must contain at least one non-whitespace character.</param>
public record CreateNoteRequest(string Content);
