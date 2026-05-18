using NoteTaker.Api.Models;

namespace NoteTaker.Api.Repositories;

/// <summary>
/// Defines the contract for storing and retrieving notes.
/// The abstraction allows swapping the local-file implementation for cloud
/// storage (e.g., Cosmos DB, Azure Blob Storage) without changing API controllers.
/// </summary>
public interface INoteRepository
{
    /// <summary>
    /// Returns all notes for the given user GUID, ordered newest-first.
    /// </summary>
    /// <param name="guid">The user's UUID v4 identifier.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// A read-only list of <see cref="Note"/> records sorted descending by <see cref="Note.CreatedAt"/>.
    /// Returns an empty list when no notes exist for the GUID.
    /// </returns>
    Task<IReadOnlyList<Note>> GetAllAsync(string guid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new note for the given user GUID.
    /// The implementation must guarantee that concurrent calls for the same GUID
    /// do not corrupt the underlying storage.
    /// </summary>
    /// <param name="guid">The user's UUID v4 identifier.</param>
    /// <param name="content">Plain-text content of the note.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The newly created <see cref="Note"/> with a server-assigned UTC timestamp.</returns>
    Task<Note> AddAsync(string guid, string content, CancellationToken cancellationToken = default);
}
