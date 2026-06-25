using NoteTaker.Wpf.Services;

namespace NoteTaker.Wpf.Tests.Services;

/// <summary>
/// Unit tests for the SettingsService class.
/// Tests GUID generation and persistence functionality.
/// </summary>
public class SettingsServiceTests
{
    [Fact]
    public void GetOrCreateUserGuid_FirstRun_GeneratesNewGuid()
    {
        // Arrange
        var service = new SettingsService();

        // Act
        var guid1 = service.GetOrCreateUserGuid();
        var guid2 = service.GetOrCreateUserGuid();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(guid1));
        Assert.True(Guid.TryParse(guid1, out var parsed));
        Assert.NotEqual(Guid.Empty, parsed);

        // The same GUID should be returned on subsequent calls (persisted)
        Assert.Equal(guid1, guid2);
    }

    [Fact]
    public void GetOrCreateUserGuid_ReturnsValidGuid()
    {
        // Arrange
        var service = new SettingsService();

        // Act
        var result = service.GetOrCreateUserGuid();

        // Assert
        Assert.True(Guid.TryParse(result, out var guid));
        Assert.NotEqual(Guid.Empty, guid);
    }
}
