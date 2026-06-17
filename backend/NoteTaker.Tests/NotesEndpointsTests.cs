using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NoteTaker.Api.Models;
using NoteTaker.Api.Repositories;

namespace NoteTaker.Tests;

/// <summary>
/// Integration tests for the Notes REST endpoints.
/// Uses <see cref="WebApplicationFactory{TEntryPoint}"/> to spin up the full
/// ASP.NET pipeline in-process, with a fresh temporary data directory per test.
/// </summary>
public sealed class NotesEndpointsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public NotesEndpointsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"NotetakerEndpointTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Override the repository to use our isolated temp directory.
                    services.AddSingleton<IOptions<NotesStorageOptions>>(
                        Options.Create(new NotesStorageOptions { DataDirectory = _tempDir }));
                });
            });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        Directory.Delete(_tempDir, recursive: true);
    }

    // ---------- GET /{guid}/notes ----------

    [Fact]
    public async Task GetNotes_ValidGuid_NoNotes_Returns200EmptyArray()
    {
        var guid = Guid.NewGuid();

        var response = await _client.GetAsync($"/{guid}/notes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
        Assert.Empty(notes);
    }

    [Fact]
    public async Task GetNotes_InvalidGuid_Returns400()
    {
        var response = await _client.GetAsync("/not-a-guid/notes");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetNotes_ReturnsExistingNotes_NewestFirst()
    {
        var guid = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/{guid}/notes", new { content = "First" });
        await Task.Delay(10);
        await _client.PostAsJsonAsync($"/{guid}/notes", new { content = "Second" });

        var response = await _client.GetAsync($"/{guid}/notes");
        var notes = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

        Assert.NotNull(notes);
        Assert.Equal(2, notes!.Count);
        Assert.Equal("Second", notes[0].GetProperty("content").GetString());
        Assert.Equal("First", notes[1].GetProperty("content").GetString());
    }

    // ---------- POST /{guid}/notes ----------

    [Fact]
    public async Task CreateNote_ValidRequest_Returns201WithNote()
    {
        var guid = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"/{guid}/notes",
            new { content = "My first note" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        Assert.Equal("My first note", note!.Content);
        Assert.NotEmpty(note.Id);
        Assert.True(note.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task CreateNote_ValidRequest_ReturnsUtcIso8601Timestamp()
    {
        var guid = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"/{guid}/notes",
            new { content = "UTC timestamp check" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var createdAtRaw = payload.GetProperty("createdAt").GetString();

        Assert.False(string.IsNullOrWhiteSpace(createdAtRaw));
        Assert.True(DateTimeOffset.TryParse(createdAtRaw, out var parsedTimestamp));
        Assert.Equal(TimeSpan.Zero, parsedTimestamp.Offset);
    }

    [Fact]
    public async Task CreateNote_InvalidGuid_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/bad-guid/notes",
            new { content = "Note" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateNote_PathTraversalStyleGuid_Returns400()
    {
        var response = await _client.PostAsJsonAsync(
            "/..%2F..%2Fetc%2Fpasswd/notes",
            new { content = "Blocked" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateNote_EmptyContent_Returns400()
    {
        var guid = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"/{guid}/notes",
            new { content = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateNote_NullContent_Returns400()
    {
        var guid = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"/{guid}/notes",
            new { content = (string?)null });

        // Either 400 (our validation) or 422/400 from model binding failure.
        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity,
            $"Expected 400 or 422, got {response.StatusCode}");
    }

    [Fact]
    public async Task CreateNote_PersistsNoteAcrossRequests()
    {
        var guid = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/{guid}/notes", new { content = "Persistent note" });

        var response = await _client.GetAsync($"/{guid}/notes");
        var notes = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

        Assert.NotNull(notes);
        Assert.Single(notes!);
        Assert.Equal("Persistent note", notes[0].GetProperty("content").GetString());
    }
}
