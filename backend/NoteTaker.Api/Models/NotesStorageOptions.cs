namespace NoteTaker.Api.Models;

/// <summary>
/// Strongly-typed options for the local-file notes storage layer.
/// Bound from the "NotesStorage" section of appsettings.json (or the
/// NOTESSTORAGE__ environment variable prefix).
/// </summary>
public sealed class NotesStorageOptions
{
    /// <summary>
    /// Path to the directory where per-GUID JSON note files are stored.
    /// May be absolute or relative to the working directory of the API process.
    /// </summary>
    public string DataDirectory { get; set; } = "data";
}
