using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using NoteTaker.Wpf.Models;

namespace NoteTaker.Wpf.Services;

/// <summary>
/// HTTP-based implementation of <see cref="INoteApiService"/> that communicates
/// with the Note Taker REST API running at the configured base address.
/// Uses System.Net.Http.Json for simplified JSON serialization/deserialization.
/// </summary>
public class NoteApiService : INoteApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteApiService"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The HTTP client configured with the base address of the Note Taker API.
    /// Typically injected by the DI container with a named or typed client.
    /// </param>
    public NoteApiService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        // Configure JSON options to match the API's casing (camelCase by default in ASP.NET Core)
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Retrieves all notes for the specified user GUID from the REST API.
    /// Makes a GET request to /{userGuid}/notes.
    /// </summary>
    /// <param name="userGuid">The unique identifier for the user.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>A read-only list of notes, sorted newest-first by the API.</returns>
    /// <exception cref="HttpRequestException">Thrown when the API returns a non-success status code.</exception>
    public async Task<IReadOnlyList<Note>> GetNotesAsync(string userGuid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userGuid))
        {
            throw new ArgumentException("User GUID cannot be null or whitespace.", nameof(userGuid));
        }

        var response = await _httpClient.GetAsync($"{userGuid}/notes", cancellationToken);
        response.EnsureSuccessStatusCode();

        var notes = await response.Content.ReadFromJsonAsync<List<Note>>(_jsonOptions, cancellationToken);
        return notes ?? new List<Note>();
    }

    /// <summary>
    /// Creates a new note for the specified user GUID via the REST API.
    /// Makes a POST request to /{userGuid}/notes with the note content.
    /// </summary>
    /// <param name="userGuid">The unique identifier for the user.</param>
    /// <param name="content">The content of the note to create. Must not be empty or whitespace.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The newly created note with its server-assigned ID and timestamp.</returns>
    /// <exception cref="ArgumentException">Thrown when userGuid or content is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns a non-success status code.</exception>
    public async Task<Note> CreateNoteAsync(string userGuid, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userGuid))
        {
            throw new ArgumentException("User GUID cannot be null or whitespace.", nameof(userGuid));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Note content cannot be null or whitespace.", nameof(content));
        }

        var request = new CreateNoteRequest(content);
        var response = await _httpClient.PostAsJsonAsync($"{userGuid}/notes", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        var note = await response.Content.ReadFromJsonAsync<Note>(_jsonOptions, cancellationToken);
        return note ?? throw new InvalidOperationException("API returned null note after creation.");
    }
}
