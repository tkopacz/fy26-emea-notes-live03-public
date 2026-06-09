using System.Windows;
using NoteTaker.Wpf.ViewModels;

namespace NoteTaker.Wpf;

/// <summary>
/// Main window for the Note Taker WPF application.
/// Implements the MVVM pattern with MainViewModel as the DataContext.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// The MainViewModel is injected via constructor for testability and clean architecture.
    /// </summary>
    /// <param name="viewModel">The view model that manages the window's data and logic.</param>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        // Set the ViewModel as the DataContext for XAML data binding
        DataContext = viewModel;

        // Load notes when the window is loaded
        Loaded += async (sender, args) => await viewModel.LoadNotesCommand.ExecuteAsync(null);
    }
}
