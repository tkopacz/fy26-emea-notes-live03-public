using Microsoft.Extensions.Options;
using NoteTaker.Api.Models;
using NoteTaker.Api.Repositories;

namespace NoteTaker.Tests;

/// <summary>
/// Unit tests for <see cref="JsonFileNoteRepository"/>.
/// Each test runs in its own isolated temporary directory to avoid state leakage.
/// </summary>
public sealed class JsonFileNoteRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly JsonFileNoteRepository _sut;

    public JsonFileNoteRepositoryTests()
    {
        // Create a unique temporary directory per test to ensure full isolation.
        _tempDir = Path.Combine(Path.GetTempPath(), $"NotetakerTests_{Guid.NewGuid()}");
        _sut = CreateRepository(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    // ---------- GetAllAsync ----------

    [Fact]
    public async Task GetAllAsync_WhenNoNotesExist_ReturnsEmptyList()
    {
        var guid = Guid.NewGuid().ToString();

        var result = await _sut.GetAllAsync(guid);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsNotesSortedNewestFirst()
    {
        var guid = Guid.NewGuid().ToString();

        var first = await _sut.AddAsync(guid, "First note");
        // Small delay to guarantee distinct timestamps.
        await Task.Delay(10);
        var second = await _sut.AddAsync(guid, "Second note");

        var result = await _sut.GetAllAsync(guid);

        Assert.Equal(2, result.Count);
        // Newest (second) should appear first.
        Assert.Equal(second.Id, result[0].Id);
        Assert.Equal(first.Id, result[1].Id);
    }

    // ---------- AddAsync ----------

    [Fact]
    public async Task AddAsync_PersistsNoteWithCorrectContent()
    {
        var guid = Guid.NewGuid().ToString();
        const string content = "Hello, world!";

        var created = await _sut.AddAsync(guid, content);

        Assert.NotEmpty(created.Id);
        Assert.Equal(content, created.Content);
        Assert.True(created.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task AddAsync_StoresNoteOnDisk()
    {
        var guid = Guid.NewGuid().ToString();

        await _sut.AddAsync(guid, "Persisted note");

        var file = Path.Combine(_tempDir, $"{guid}.json");
        Assert.True(File.Exists(file), "JSON file should exist after adding a note.");
    }

    [Fact]
    public async Task AddAsync_AccumulatesMultipleNotes()
    {
        var guid = Guid.NewGuid().ToString();

        await _sut.AddAsync(guid, "Note A");
        await _sut.AddAsync(guid, "Note B");
        await _sut.AddAsync(guid, "Note C");

        var result = await _sut.GetAllAsync(guid);

        Assert.Equal(3, result.Count);
    }

    // ---------- Concurrent write safety ----------

    [Fact]
    public async Task AddAsync_ConcurrentWrites_DoNotCorruptFile()
    {
        var guid = Guid.NewGuid().ToString();
        const int concurrentCount = 20;

        // Fire off many concurrent writes for the same GUID.
        var tasks = Enumerable
            .Range(1, concurrentCount)
            .Select(i => _sut.AddAsync(guid, $"Concurrent note {i}"))
            .ToList();

        await Task.WhenAll(tasks);

        // All notes must have been persisted without any being lost.
        var result = await _sut.GetAllAsync(guid);
        Assert.Equal(concurrentCount, result.Count);
    }

    // ---------- Multiple GUIDs ----------

    [Fact]
    public async Task AddAsync_DifferentGuids_AreStoredSeparately()
    {
        var guid1 = Guid.NewGuid().ToString();
        var guid2 = Guid.NewGuid().ToString();

        await _sut.AddAsync(guid1, "User 1 note");
        await _sut.AddAsync(guid2, "User 2 note");

        var notes1 = await _sut.GetAllAsync(guid1);
        var notes2 = await _sut.GetAllAsync(guid2);

        Assert.Single(notes1);
        Assert.Single(notes2);
        Assert.Equal("User 1 note", notes1[0].Content);
        Assert.Equal("User 2 note", notes2[0].Content);
    }

    // ---------- Helpers ----------

    private static JsonFileNoteRepository CreateRepository(string dir) =>
        new(Options.Create(new NotesStorageOptions { DataDirectory = dir }));
}
