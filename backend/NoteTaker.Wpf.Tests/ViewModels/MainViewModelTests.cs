using Moq;
using NoteTaker.Wpf.Models;
using NoteTaker.Wpf.Services;
using NoteTaker.Wpf.ViewModels;

namespace NoteTaker.Wpf.Tests.ViewModels;

/// <summary>
/// Unit tests for the MainViewModel class.
/// Verifies command-driven note editing behaviors exposed to the WPF view.
/// </summary>
public class MainViewModelTests
{
    [Fact]
    public void UseAsBasisCommand_WithSourceNote_CopiesContentToNoteContent()
    {
        var apiService = new Mock<INoteApiService>();
        var settingsService = new SettingsService();
        var viewModel = new MainViewModel(apiService.Object, settingsService);
        var sourceNote = new Note("note-1", "Seed content", DateTimeOffset.UtcNow);

        viewModel.UseAsBasisCommand.Execute(sourceNote);

        Assert.Equal(sourceNote.Content, viewModel.NoteContent);
    }
}
