using NoteTaker.Api.Endpoints;
using NoteTaker.Api.Models;
using NoteTaker.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

// Bind the "NotesStorage" section so INoteRepository can inject IOptions<NotesStorageOptions>.
builder.Services.Configure<NotesStorageOptions>(
    builder.Configuration.GetSection("NotesStorage"));

// ---------------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------------

// Register the local-file repository as a singleton so that the per-GUID
// SemaphoreSlim dictionary survives across requests.
builder.Services.AddSingleton<INoteRepository, JsonFileNoteRepository>();

// CORS — allow the Vite dev server (and any configurable frontend origin)
// to call the API.  In production, tighten this to the deployed frontend URL.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",  // Vite dev server default
                "http://localhost:4173")  // Vite preview
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// OpenAPI / Swagger — useful for exploring the API during demos.
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    // Serve the OpenAPI JSON at /openapi/v1.json
    app.MapOpenApi();
}

// Apply CORS before any endpoint middleware so preflight requests are handled.
app.UseCors();

// HTTPS redirect is intentionally omitted in development to keep the setup
// friction-free; enable it in production via reverse proxy.

// ---------------------------------------------------------------------------
// Endpoints
// ---------------------------------------------------------------------------
app.MapNotesEndpoints();

app.Run();

// Expose the Program class for integration testing (WebApplicationFactory).
public partial class Program { }

