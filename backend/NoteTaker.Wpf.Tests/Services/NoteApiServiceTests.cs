using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Moq;
using Moq.Protected;
using NoteTaker.Wpf.Models;
using NoteTaker.Wpf.Services;

namespace NoteTaker.Wpf.Tests.Services;

/// <summary>
/// Unit tests for the NoteApiService class.
/// Tests API communication, error handling, and data serialization.
/// </summary>
public class NoteApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly NoteApiService _service;

    public NoteApiServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        _service = new NoteApiService(_httpClient);
    }

    [Fact]
    public async Task GetNotesAsync_ValidGuid_ReturnsNotes()
    {
        // Arrange
        var userGuid = Guid.NewGuid().ToString();
        var expectedNotes = new List<Note>
        {
            new Note("id1", "Note 1", DateTimeOffset.UtcNow),
            new Note("id2", "Note 2", DateTimeOffset.UtcNow.AddMinutes(-5))
        };

        var responseContent = JsonSerializer.Serialize(expectedNotes);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"{userGuid}/notes")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.GetNotesAsync(userGuid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Note 1", result[0].Content);
        Assert.Equal("Note 2", result[1].Content);
    }

    [Fact]
    public async Task GetNotesAsync_EmptyGuid_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetNotesAsync(string.Empty));
    }

    [Fact]
    public async Task CreateNoteAsync_ValidContent_ReturnsCreatedNote()
    {
        // Arrange
        var userGuid = Guid.NewGuid().ToString();
        var content = "Test note content";
        var createdNote = new Note("new-id", content, DateTimeOffset.UtcNow);

        var responseContent = JsonSerializer.Serialize(createdNote);
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"{userGuid}/notes")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _service.CreateNoteAsync(userGuid, content);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-id", result.Id);
        Assert.Equal(content, result.Content);
    }

    [Fact]
    public async Task CreateNoteAsync_EmptyContent_ThrowsArgumentException()
    {
        // Arrange
        var userGuid = Guid.NewGuid().ToString();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateNoteAsync(userGuid, string.Empty));
    }

    [Fact]
    public async Task CreateNoteAsync_WhitespaceContent_ThrowsArgumentException()
    {
        // Arrange
        var userGuid = Guid.NewGuid().ToString();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateNoteAsync(userGuid, "   "));
    }
}
