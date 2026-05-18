namespace NoteTaker.Api.Models;

/// <summary>
/// Represents a single immutable note stored for a user identified by their GUID.
/// Once created, a note's content and timestamp cannot be changed (enforced at the API level).
/// </summary>
/// <param name="Id">Unique identifier for this note (UUID v4).</param>
/// <param name="Content">Plain-text body of the note.</param>
/// <param name="CreatedAt">UTC timestamp of when the note was saved on the server.</param>
public record Note(string Id, string Content, DateTimeOffset CreatedAt);
