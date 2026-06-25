namespace NoteTaker.Wpf.Models;

/// <summary>
/// Represents a single immutable note retrieved from the API.
/// This model matches the Note structure returned by the REST API.
/// </summary>
/// <param name="Id">Unique identifier for this note (UUID v4).</param>
/// <param name="Content">Plain-text body of the note.</param>
/// <param name="CreatedAt">UTC timestamp of when the note was saved on the server.</param>
public record Note(string Id, string Content, DateTimeOffset CreatedAt);
