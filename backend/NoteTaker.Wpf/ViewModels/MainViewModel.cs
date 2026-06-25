using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NoteTaker.Wpf.Models;
using NoteTaker.Wpf.Services;

namespace NoteTaker.Wpf.ViewModels;

/// <summary>
/// ViewModel for the main window, implementing the MVVM pattern.
/// Manages the note collection, user input, and communication with the API service.
/// Uses CommunityToolkit.Mvvm for observable properties and commands.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly INoteApiService _apiService;
    private readonly SettingsService _settingsService;

    /// <summary>
    /// The user's unique identifier (GUID) used for all API calls.
    /// This can be loaded from settings or generated on first run.
    /// </summary>
    [ObservableProperty]
    private string _userGuid;

    /// <summary>
    /// The current text input by the user for a new note.
    /// Bound to the TextBox in the UI.
    /// </summary>
    [ObservableProperty]
    private string _noteContent = string.Empty;

    /// <summary>
    /// Error message to display to the user when an operation fails.
    /// Empty when no error is present.
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Indicates whether the UI should show a loading indicator.
    /// True when an async operation is in progress.
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Observable collection of notes displayed in the UI.
    /// Automatically notifies the UI when notes are added or removed.
    /// </summary>
    public ObservableCollection<Note> Notes { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="apiService">The service for communicating with the REST API.</param>
    /// <param name="settingsService">The service for persisting application settings.</param>
    public MainViewModel(INoteApiService apiService, SettingsService settingsService)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        // Load or generate user GUID from persistent storage
        _userGuid = _settingsService.GetOrCreateUserGuid();
    }

    /// <summary>
    /// Loads all notes for the current user from the API.
    /// This method is called when the window loads or when refreshing the list.
    /// </summary>
    [RelayCommand]
    private async Task LoadNotesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var notes = await _apiService.GetNotesAsync(UserGuid);

            // Clear and repopulate the observable collection
            Notes.Clear();
            foreach (var note in notes)
            {
                Notes.Add(note);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load notes: {ex.Message}";
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Creates a new note with the current content and adds it to the list.
    /// The content is validated before sending to the API.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCreateNote))]
    private async Task CreateNoteAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var newNote = await _apiService.CreateNoteAsync(UserGuid, NoteContent);

            // Add the new note to the beginning of the list (newest first)
            Notes.Insert(0, newNote);

            // Clear the input field after successful creation
            NoteContent = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create note: {ex.Message}";
            MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Determines whether the CreateNote command can execute.
    /// Returns true only if the note content is not empty or whitespace.
    /// </summary>
    private bool CanCreateNote()
    {
        return !string.IsNullOrWhiteSpace(NoteContent);
    }

    /// <summary>
    /// Creates a new note with content from an existing note.
    /// Implements the "Use as basis" functionality from the original web UI.
    /// </summary>
    /// <param name="sourceNote">The note whose content should be used as a template.</param>
    [RelayCommand]
    private void UseAsBasis(Note sourceNote)
    {
        if (sourceNote is not null)
        {
            NoteContent = sourceNote.Content;
        }
    }

    /// <summary>
    /// Refreshes the note list by reloading from the API.
    /// This is useful when the user suspects the data might be stale.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadNotesAsync();
    }

    /// <summary>
    /// Copies the user's unique URL to the clipboard.
    /// This allows the user to bookmark or share their note space.
    /// </summary>
    [RelayCommand]
    private void CopyUserUrl()
    {
        var baseUrl = _apiService.GetType().Assembly.GetName().Name; // This should be configurable
        var url = $"http://localhost:5173/{UserGuid}";
        Clipboard.SetText(url);
        MessageBox.Show("Your personal notes URL has been copied to the clipboard!",
            "URL Copied", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    partial void OnNoteContentChanged(string value)
    {
        // Notify that the CanExecute state of CreateNoteCommand may have changed
        CreateNoteCommand.NotifyCanExecuteChanged();
    }
}
