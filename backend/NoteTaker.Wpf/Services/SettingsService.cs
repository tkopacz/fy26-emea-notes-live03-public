using System.IO;

namespace NoteTaker.Wpf.Services;

/// <summary>
/// Simple service for persisting application settings locally.
/// Uses a plain text file to store the user GUID between sessions.
/// </summary>
public class SettingsService
{
    private readonly string _settingsFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    public SettingsService()
    {
        // Store settings in the user's local app data folder
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NoteTaker.Wpf");

        Directory.CreateDirectory(appDataFolder);
        _settingsFilePath = Path.Combine(appDataFolder, "settings.txt");
    }

    /// <summary>
    /// Gets or creates the user GUID for this installation.
    /// The GUID is persisted to disk and reused across application restarts.
    /// </summary>
    /// <returns>The user's unique identifier.</returns>
    public string GetOrCreateUserGuid()
    {
        try
        {
            // Check if we have a saved GUID
            if (File.Exists(_settingsFilePath))
            {
                var savedGuid = File.ReadAllText(_settingsFilePath).Trim();
                if (Guid.TryParse(savedGuid, out var parsed) && parsed != Guid.Empty)
                {
                    return savedGuid;
                }
            }

            // Generate a new GUID for first-time users
            var newGuid = Guid.NewGuid().ToString();
            File.WriteAllText(_settingsFilePath, newGuid);
            return newGuid;
        }
        catch
        {
            // If we can't read/write settings, just generate a temporary GUID
            // This will be lost when the app closes, but allows the app to work
            return Guid.NewGuid().ToString();
        }
    }
}
