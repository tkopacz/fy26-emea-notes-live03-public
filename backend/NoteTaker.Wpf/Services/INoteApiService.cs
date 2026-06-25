using NoteTaker.Wpf.Models;

namespace NoteTaker.Wpf.Services;

/// <summary>
/// Interface for interacting with the Note Taker REST API.
/// Provides methods to retrieve and create notes for a given user GUID.
/// </summary>
public interface INoteApiService
{
    /// <summary>
    /// Retrieves all notes for the specified user GUID from the REST API.
    /// Notes are returned sorted newest-first by the API.
    /// </summary>
    /// <param name="userGuid">The unique identifier for the user.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>A read-only list of notes, or an empty list if no notes exist.</returns>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    Task<IReadOnlyList<Note>> GetNotesAsync(string userGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new note for the specified user GUID via the REST API.
    /// </summary>
    /// <param name="userGuid">The unique identifier for the user.</param>
    /// <param name="content">The content of the note to create. Must not be empty or whitespace.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The newly created note with its server-assigned ID and timestamp.</returns>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    /// <exception cref="ArgumentException">Thrown when content is null, empty, or whitespace.</exception>
    Task<Note> CreateNoteAsync(string userGuid, string content, CancellationToken cancellationToken = default);
}
