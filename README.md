# Note-Taker App

A lightweight full-stack demo app where multiple users can create and read personal notes — **no login required**. Each user gets a unique, bookmarkable URL containing a GUID.

Built as a reference codebase for GitHub Copilot demos.

---

## Architecture

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 10 Minimal API (C# 14) |
| Frontend | React 18 + Vite + TypeScript |
| Storage | Local JSON files (`data/{guid}.json`) |
| Startup | `concurrently` (single command) |

```
/
├── backend/
│   ├── NoteTaker.Api/       # REST API — Models, Repositories, Endpoints
│   └── NoteTaker.Tests/     # xUnit unit + integration tests
├── frontend/
│   └── src/
│       ├── api/             # API client (fetch wrapper)
│       ├── components/      # NoteInput, NoteList, NoteCard
│       └── pages/           # RootPage (redirect), NotesPage
├── package.json             # Root — single-command startup
└── README.md
```

---

## Prerequisites

| Tool | Minimum version |
|------|----------------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |
| [Node.js](https://nodejs.org/) | 20 LTS |
| npm | 10 |

---

## Quick start

```bash
# 1. Clone the repo
git clone https://github.com/<your-org>/fy26-emea-notes-live03-public.git
cd fy26-emea-notes-live03-public

# 2. Install root dependencies (concurrently)
npm install

# 3. Start both backend and frontend with ONE command
npm start
```

- **Backend API** → http://localhost:5000  
- **Frontend** → http://localhost:5173  

Open http://localhost:5173 in your browser. You'll be redirected to a unique URL (e.g., `/3fa85f64-...`) that is your private note space.

---

## API endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/{guid}/notes` | List all notes for a user (newest first) |
| `POST` | `/{guid}/notes` | Create a new note |

OpenAPI docs available at http://localhost:5000/openapi/v1.json when running in Development mode.

### POST body

```json
{ "content": "Your note text here" }
```

### Note response

```json
{
  "id": "uuid-v4",
  "content": "Your note text here",
  "createdAt": "2026-05-18T10:30:00Z"
}
```

---

## Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `NotesStorage:DataDirectory` | `data` | Path (absolute or relative) where JSON note files are stored |

Override via environment variable: `NOTESSTORAGE__DATADIRECTORY=/mnt/notes`

---

## Running tests

```bash
npm test
# or directly:
dotnet test backend/NoteTaker.Tests
```

---

## Key design decisions

- **Immutable notes**: No `PUT`/`PATCH`/`DELETE` endpoints exist. The "Use as basis" UX creates a new note pre-populated from an old one.
- **Concurrent write safety**: A `SemaphoreSlim` per GUID serialises file reads/writes, preventing corruption under concurrent tab usage.
- **Repository abstraction**: `INoteRepository` decouples the API from the file system. Swapping in Cosmos DB or Azure Blob Storage requires only a new implementation.
- **Security-by-obscurity**: The 128-bit UUID v4 is probabilistically unguessable. Keep your URL private.
