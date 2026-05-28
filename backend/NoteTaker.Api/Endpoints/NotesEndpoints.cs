using NoteTaker.Api.Models;
using NoteTaker.Api.Repositories;

namespace NoteTaker.Api.Endpoints;

/// <summary>
/// Extension method that registers all note-related Minimal API endpoints on the
/// <see cref="WebApplication"/> instance. Keeping endpoint registration in a
/// dedicated class avoids cluttering <c>Program.cs</c>.
/// </summary>
public static class NotesEndpoints
{
    /// <summary>
    /// Maps the following endpoints:
    /// <list type="bullet">
    ///   <item><description><c>GET  /{guid}/notes</c> — list all notes (newest first)</description></item>
    ///   <item><description><c>POST /{guid}/notes</c> — create a new note</description></item>
    /// </list>
    /// </summary>
    /// <param name="app">The web application to register routes on.</param>
    /// <returns>The same <see cref="WebApplication"/> for fluent chaining.</returns>
    public static WebApplication MapNotesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/{guid}/notes")
                       .WithTags("Notes");

        /// <summary>Returns all notes for the user identified by <c>{guid}</c>, sorted newest-first.</summary>
        group.MapGet("/", async (string guid, INoteRepository repo, CancellationToken ct) =>
        {
            if (!IsValidGuid(guid))
            {
                return Results.Problem(
                    detail: $"'{guid}' is not a valid UUID v4.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid GUID");
            }

            var notes = await repo.GetAllAsync(guid, ct);
            return Results.Ok(notes);
        })
        .WithName("GetNotes")
        .WithSummary("Get all notes for a user")
        .Produces<IReadOnlyList<Note>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        /// <summary>Creates a new immutable note for the user identified by <c>{guid}</c>.</summary>
        group.MapPost("/", async (string guid, CreateNoteRequest request, INoteRepository repo, CancellationToken ct) =>
        {
            if (!IsValidGuid(guid))
            {
                return Results.Problem(
                    detail: $"'{guid}' is not a valid UUID v4.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid GUID");
            }

            // Prevent empty or whitespace-only notes from being persisted.
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Results.Problem(
                    detail: "Note content must contain at least one non-whitespace character.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Empty content");
            }

            var created = await repo.AddAsync(guid, request.Content, ct);

            // Return 201 Created with a Location header and the new note body.
            return Results.Created($"/{guid}/notes/{created.Id}", created);
        })
        .WithName("CreateNote")
        .WithSummary("Create a new note for a user")
        .Produces<Note>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    /// <summary>
    /// Validates that <paramref name="value"/> is a non-empty, well-formed UUID.
    /// Uses <see cref="Guid.TryParse"/> which accepts any UUID version; the PRD
    /// does not require version-4 enforcement on incoming user GUIDs.
    /// </summary>
    /// <remarks>
    /// Gibt <c>false</c> zurück, wenn der Wert leer ist oder kein gültiges UUID-Format hat.
    /// In diesem Fall antworten die Endpunkte mit HTTP 400 (Bad Request).
    /// Returns <c>false</c> when the value is empty or does not match a valid UUID format,
    /// in which case the endpoints respond with HTTP 400 (Bad Request).
    /// </remarks>
    private static bool IsValidGuid(string value) =>
        Guid.TryParse(value, out var parsed) && parsed != Guid.Empty;
}
