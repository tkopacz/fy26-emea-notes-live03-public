# Note Taker - WPF Client

A Windows Presentation Foundation (WPF) desktop client for the Note Taker REST API. This application provides a native Windows desktop experience for managing your personal notes.

## Features

- **Modern MVVM Architecture**: Built using the Model-View-ViewModel pattern with CommunityToolkit.Mvvm
- **Dependency Injection**: Uses Microsoft.Extensions.DependencyInjection for clean, testable code
- **Persistent User Identity**: Your unique GUID is saved locally and persists across sessions
- **Real-time API Integration**: Communicates with the Note Taker REST API for note storage
- **Rich UI**: Clean, intuitive interface built with WPF and XAML
- **"Use as Basis" Feature**: Create new notes pre-populated from existing ones

## Architecture

The application follows clean architecture principles:

```
NoteTaker.Wpf/
├── Models/                 # Data models (Note, CreateNoteRequest)
├── Services/               # Business logic (API client, Settings)
│   ├── INoteApiService.cs
│   ├── NoteApiService.cs
│   └── SettingsService.cs
├── ViewModels/             # MVVM ViewModels
│   └── MainViewModel.cs
├── Converters/             # XAML value converters
├── MainWindow.xaml         # Main UI layout
└── App.xaml.cs            # Application startup & DI configuration
```

## Prerequisites

- **Windows OS**: Required for running WPF applications
- **.NET 10 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Running API Server**: The backend API must be running on `http://localhost:5000`

## Getting Started

### 1. Start the Backend API

Before running the WPF client, ensure the Note Taker API is running:

```bash
# From the repository root
cd backend/NoteTaker.Api
dotnet run
```

The API should be running at `http://localhost:5000`.

### 2. Run the WPF Application

```bash
cd backend/NoteTaker.Wpf
dotnet run
```

### 3. Build for Release

To create a release build:

```bash
dotnet build -c Release
```

The executable will be located in:
```
backend/NoteTaker.Wpf/bin/Release/net10.0-windows/NoteTaker.Wpf.exe
```

## How It Works

### First Run

When you launch the application for the first time:

1. A unique GUID is generated for you
2. This GUID is saved to your local app data folder
3. All your notes are associated with this GUID

### Creating Notes

1. Type your note content in the text box
2. Click "Save Note" to create the note
3. The note appears immediately in the list below

### Using Notes as Templates

Click "Use as Basis" on any existing note to copy its content into the input field, allowing you to create a new note based on an existing one.

### Sharing Your Notes

Click "Copy My URL" to copy your personal notes URL to the clipboard. You can use this URL in a web browser to access your notes through the React frontend.

## Configuration

### API Base URL

The API base URL is hardcoded to `http://localhost:5000` in `App.xaml.cs`. To change it:

1. Open `backend/NoteTaker.Wpf/App.xaml.cs`
2. Modify the `ConfigureServices` method:

```csharp
services.AddHttpClient<INoteApiService, NoteApiService>(client =>
{
    client.BaseAddress = new Uri("http://your-api-url:port");
});
```

### User Data Location

Your GUID and settings are stored in:
```
%LOCALAPPDATA%\NoteTaker.Wpf\settings.txt
```

## Development

### Project Structure

- **Models**: Plain C# records matching the API contracts
- **Services**: API communication and local storage
- **ViewModels**: Observable properties and commands using CommunityToolkit.Mvvm
- **Views**: XAML-based UI with data binding

### Key Technologies

- **WPF**: Windows Presentation Foundation for UI
- **MVVM Pattern**: Using CommunityToolkit.Mvvm for boilerplate reduction
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **HTTP Client**: System.Net.Http with JSON extensions

### Running Tests

```bash
cd backend/NoteTaker.Wpf.Tests
dotnet test
```

**Note**: Tests must be run on Windows as they reference WPF assemblies.

## Design Decisions

### Why MVVM?

The MVVM pattern provides:
- Clear separation of concerns
- Testable business logic in ViewModels
- Data binding for automatic UI updates
- No UI logic in code-behind files

### Why Dependency Injection?

DI enables:
- Loose coupling between components
- Easy unit testing with mock services
- Single responsibility principle
- Flexible service configuration

### Why CommunityToolkit.Mvvm?

The MVVM Toolkit provides:
- Source generators to reduce boilerplate
- `[ObservableProperty]` for automatic property notifications
- `[RelayCommand]` for automatic command creation
- Modern C# features (C# 14)

## Troubleshooting

### "Cannot connect to API" Error

**Solution**: Ensure the backend API is running at `http://localhost:5000`

### "Failed to load notes" Error

**Solution**:
1. Check that your GUID is valid
2. Verify the API is accessible
3. Check network connectivity

### Application Won't Start

**Solution**:
1. Verify .NET 10 SDK is installed
2. Check that you're running on Windows
3. Try cleaning and rebuilding: `dotnet clean && dotnet build`

## Future Enhancements

Potential features for future versions:

- [ ] Configurable API endpoint through UI settings
- [ ] Dark/Light theme toggle
- [ ] Note search and filtering
- [ ] Offline mode with local caching
- [ ] Export notes to various formats
- [ ] Rich text editing support
- [ ] Note tags and categories

## License

This project is part of the Note Taker demonstration application.
