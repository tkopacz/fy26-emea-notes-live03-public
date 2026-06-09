using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NoteTaker.Wpf.Services;
using NoteTaker.Wpf.ViewModels;

namespace NoteTaker.Wpf;

/// <summary>
/// Main application class that configures dependency injection and starts the WPF application.
/// Uses Microsoft.Extensions.DependencyInjection for service registration and resolution.
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class and configures services.
    /// </summary>
    public App()
    {
        // Build the service container with all application dependencies
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Configures all services for dependency injection.
    /// Registers HttpClient, API service, and ViewModels.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    private void ConfigureServices(IServiceCollection services)
    {
        // Register HttpClient with base address pointing to the local API server
        // In production, this could be loaded from configuration or environment variables
        services.AddHttpClient<INoteApiService, NoteApiService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        });

        // Register application services
        services.AddSingleton<SettingsService>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();

        // Register MainWindow as transient so it can be created with DI
        services.AddTransient<MainWindow>();
    }

    /// <summary>
    /// Override OnStartup to resolve and show the MainWindow using DI.
    /// This replaces the default StartupUri approach to enable constructor injection.
    /// </summary>
    /// <param name="e">Startup event arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Resolve MainWindow from DI container and show it
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    /// <summary>
    /// Clean up resources when the application shuts down.
    /// </summary>
    /// <param name="e">Exit event arguments.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}

