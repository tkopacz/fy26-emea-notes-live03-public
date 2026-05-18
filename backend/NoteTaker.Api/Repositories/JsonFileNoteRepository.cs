using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NoteTaker.Api.Models;

namespace NoteTaker.Api.Repositories;

/// <summary>
/// Stores notes as a JSON array on the local filesystem, one file per user GUID
/// (e.g., <c>data/{guid}.json</c>). Concurrent writes to the same file are
/// serialised with a per-GUID <see cref="SemaphoreSlim"/> so that no data is
/// lost even when multiple browser tabs post notes simultaneously.
/// </summary>
public sealed class JsonFileNoteRepository : INoteRepository
{
    // One semaphore per GUID; maximum concurrency for a single GUID is 1.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    // Reuse serialiser options to avoid repeated allocation.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly string _dataDirectory;

    /// <summary>
    /// Initialises the repository and ensures the data directory exists.
    /// </summary>
    /// <param name="options">Strongly-typed storage options bound from configuration.</param>
    public JsonFileNoteRepository(IOptions<NotesStorageOptions> options)
    {
        _dataDirectory = options.Value.DataDirectory;
        // Create the directory on first use so the app works out of the box.
        Directory.CreateDirectory(_dataDirectory);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Note>> GetAllAsync(
        string guid,
        CancellationToken cancellationToken = default)
    {
        var path = FilePath(guid);

        if (!File.Exists(path))
        {
            return [];
        }

        // Read under the per-GUID lock to avoid reading a partially-written file.
        var semaphore = GetLock(guid);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            var notes = JsonSerializer.Deserialize<List<Note>>(json, _jsonOptions) ?? [];
            // Sort descending so the newest note is first.
            return notes.OrderByDescending(n => n.CreatedAt).ToList().AsReadOnly();
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Note> AddAsync(
        string guid,
        string content,
        CancellationToken cancellationToken = default)
    {
        var newNote = new Note(
            Id: Guid.NewGuid().ToString(),
            Content: content,
            // Always use UTC so clients can convert to any local timezone.
            CreatedAt: DateTimeOffset.UtcNow);

        var path = FilePath(guid);
        var semaphore = GetLock(guid);

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            // Read existing notes (empty list if the file does not yet exist).
            List<Note> notes;

            if (File.Exists(path))
            {
                var existing = await File.ReadAllTextAsync(path, cancellationToken);
                notes = JsonSerializer.Deserialize<List<Note>>(existing, _jsonOptions) ?? [];
            }
            else
            {
                notes = [];
            }

            notes.Add(newNote);

            // Write to a temporary file first, then atomically replace the target.
            // This prevents a crash mid-write from corrupting the existing data.
            var tempPath = path + ".tmp";
            var serialised = JsonSerializer.Serialize(notes, _jsonOptions);
            await File.WriteAllTextAsync(tempPath, serialised, cancellationToken);
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            semaphore.Release();
        }

        return newNote;
    }

    /// <summary>Returns the absolute path of the JSON file for the given GUID.</summary>
    private string FilePath(string guid) =>
        Path.Combine(_dataDirectory, $"{guid}.json");

    /// <summary>
    /// Returns (or creates) the per-GUID semaphore used to serialize concurrent file access.
    /// </summary>
    private static SemaphoreSlim GetLock(string guid) =>
        _locks.GetOrAdd(guid, _ => new SemaphoreSlim(1, 1));
}
