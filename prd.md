# PRD: Note-Taker App

## 1. Product overview

### 1.1 Document title and version

- PRD: Note-Taker App
- Version: 1.0

### 1.2 Product summary

The Note-Taker App is a lightweight web application that allows multiple users to create and read personal notes without requiring any login. Each user is identified by a unique, shareable URL containing a GUID, giving them private access to their own note collection.

Notes are immutable once saved — users cannot edit or delete them. If a user needs to revise the content of an existing note, they can create a new note pre-populated with the old content; this produces a distinct note with a new timestamp while the original remains locked and visible in the history.

The first version stores notes as local files on the server. The system is composed of a C# REST API backend and a React 18 TypeScript SPA frontend, run together with a single command.

---

## 2. Goals

### 2.1 Business goals

- Deliver a minimal, focused demo of a note-taking application that showcases the full-stack development workflow.
- Establish a working foundation (local-file storage) that can be extended to cloud storage (e.g., Cosmos DB, Blob Storage) in future iterations.
- Serve as a reference codebase for GitHub Copilot demos within the EMEA team.

### 2.2 User goals

- Quickly capture timestamped notes without creating an account.
- Access a personal note feed via a bookmarkable, unique URL.
- Review the full, unmodified history of all past notes.
- Derive a new note from an existing one when content needs updating, without losing the original record.

### 2.3 Non-goals

- User authentication or authorization (no login, no passwords).
- Editing or deleting existing notes.
- Note titles, tags, categories, or search/filter in v1.
- Real-time collaboration or note sharing between different users' GUIDs.
- Mobile-native or desktop-native packaging.

---

## 3. User personas

### 3.1 Key user types

- **Regular note-taker**: A person who wants a private, persistent log of timestamped text notes, accessible from any browser via their unique URL.
- **Developer/demo audience**: A developer evaluating the codebase or watching a Copilot demo who needs to understand the architecture quickly.

### 3.2 Basic persona details

- **Alex (regular note-taker)**: Alex bookmarks their unique URL and visits the app to log quick notes throughout the day. Alex never needs to edit a note but occasionally wants to refine earlier thoughts by creating a follow-up note copied from the original.

### 3.3 Role-based access

- **Note owner (GUID-holder)**: Can view all notes associated with their GUID and create new notes. Cannot modify or delete any note.
- **Anonymous visitor without a GUID**: Lands on the root URL and is redirected to a newly generated GUID URL, becoming a new note owner.

---

## 4. Functional requirements

- **GUID-based user identity** (Priority: High)
  - When a user visits the root URL `/`, the app generates a new UUID v4 and redirects the user to `/{guid}`.
  - All subsequent operations are scoped to that GUID.
  - No authentication is required.

- **Create a note** (Priority: High)
  - A user can submit a plain-text note via a text area and a "Save" button.
  - The backend records the note with a server-generated UTC timestamp (date + time, ISO 8601).
  - Once saved, the note is immediately displayed in the note list and is permanently read-only.

- **List notes** (Priority: High)
  - The app displays all notes for the current GUID in reverse chronological order (newest first).
  - Each note shows its date, time, and content. No edit or delete controls are shown.

- **Copy-to-new-note** (Priority: Medium)
  - Each note has a "Use as basis" (or equivalent) action.
  - Clicking it pre-fills the new-note text area with the content of the selected note.
  - Submitting creates a brand-new note with the current timestamp; the original note is unchanged.

- **Persistent local-file storage** (Priority: High)
  - The backend stores notes as JSON files on the local filesystem, organized by GUID (e.g., `data/{guid}.json`).
  - The data directory is configurable via an environment variable or `appsettings.json`.

- **Single-command startup** (Priority: High)
  - A single command (e.g., `dotnet run` from a root orchestration project, or a `Makefile`/`package.json` script) starts both the backend and the frontend development server.

---

## 5. User experience

### 5.1 Entry points & first-time user flow

- User visits the app URL with no path → app generates a GUID and redirects to `/{guid}`.
- User bookmarks the new URL for future access.
- User sees an empty note list and a text area inviting them to write their first note.

### 5.2 Core experience

- **Writing a note**: The user types in the text area and clicks "Save." The note appears at the top of the list with its timestamp. The text area resets.
- **Reading notes**: Notes are listed below the input area, each showing the timestamp prominently and the content below it. All notes are read-only.
- **Creating a follow-up note**: The user clicks "Use as basis" on any existing note. The text area is populated with that note's content. The user edits as needed and saves, creating a new timestamped note.

### 5.3 Advanced features & edge cases

- If the GUID in the URL is malformed (not a valid UUID), the backend returns a 400 error and the frontend displays a friendly error message.
- Submitting an empty note is prevented — the "Save" button is disabled until at least one non-whitespace character is entered.
- Very long notes should be handled gracefully (e.g., content area scrolls; no hard truncation in the list view).
- Concurrent writes from multiple browser tabs for the same GUID are handled safely (file locking or atomic writes on the backend).

### 5.4 UI/UX highlights

- Minimal, distraction-free interface: text area + save button at the top, scrollable note list below.
- Timestamps are displayed in a human-readable local format (converted from UTC in the browser).
- Locked notes are visually distinct from the input area (e.g., a subtle background color or border), reinforcing their immutability.
- Responsive layout for both desktop and mobile browsers.

---

## 6. Narrative

Alex opens the Note-Taker App for the first time and is instantly assigned a personal, private URL. Without signing up or logging in, Alex types a quick thought, hits "Save," and sees it appear timestamped at the top of the page. A week later, Alex revisits the same bookmarked URL and finds the full history of notes exactly as written — nothing has changed. When Alex wants to refine an earlier idea, they click "Use as basis," update the text, and save — the new note appears at the top while the original remains intact below. The app stays out of the way, letting Alex focus entirely on capturing thoughts.

---

## 7. Success metrics

### 7.1 User-centric metrics

- A user can create their first note within 30 seconds of landing on the app.
- Zero data loss: all saved notes are retrievable on subsequent visits.
- The "Use as basis" flow requires no more than 2 clicks before the user can start typing.

### 7.2 Business metrics

- The codebase serves as a working demo for at least 3 GitHub Copilot demo sessions.
- The app can be cloned and started by a new developer in under 5 minutes.

### 7.3 Technical metrics

- API response time for fetching and saving notes: < 200 ms on localhost.
- Frontend initial load: < 2 seconds on a standard dev machine.
- Zero data corruption from concurrent writes to the same GUID file.

---

## 8. Technical considerations

### 8.1 Integration points

- **Backend ↔ Frontend**: REST API consumed by the React SPA. API base URL configured via environment variable (`VITE_API_URL` or similar).
- **Single-command startup**: A root-level script (e.g., `Makefile`, `package.json` `start` script, or a .NET orchestration project) starts both the ASP.NET backend and the Vite/React dev server concurrently.
- **Future storage migration**: The file-storage layer should be behind an interface/repository abstraction to allow swapping in Cosmos DB or Blob Storage without changing API controllers.

### 8.2 Data storage & privacy

- Notes are stored as JSON files at `data/{guid}.json` on the server's local filesystem.
- No personal data is collected — the GUID is the only identifier.
- The GUID acts as a shared secret (security-by-obscurity); users should be advised to keep their URL private.
- The data directory must not be served statically or be accessible via the web server.

### 8.3 Scalability & performance

- Local-file storage is sufficient for demo purposes; a single server instance handles all requests.
- File-level locking or atomic append operations must prevent corruption when the same GUID is accessed concurrently.
- Each GUID's JSON file grows unboundedly in v1 — no pagination is required for the demo but should be planned for v2.

### 8.4 Potential challenges

- **Concurrent write safety**: JSON file rewrites (read-modify-write) must be atomic to avoid corruption.
- **GUID guessing**: A 128-bit UUID v4 is probabilistically unguessable, but the app provides no additional access control.
- **Monorepo coordination**: Running two separate projects (backend + frontend) with one command requires a reliable process orchestration strategy across Windows and Linux/macOS.
- **CORS configuration**: The backend must allow requests from the frontend dev server origin.

---

## 9. Milestones & sequencing

### 9.1 Project estimate

- Small: 3–5 developer-days for a working v1 demo.

### 9.2 Team size & composition

- 2 developers: 1 backend-focused (C#), 1 frontend-focused (TypeScript/React).

### 9.3 Suggested phases

- **Phase 1**: Project scaffolding & single-command startup (0.5 days)
  - Initialize ASP.NET Web API project and React 18 + Vite TypeScript project.
  - Configure CORS, proxy, and single-command launch script.

- **Phase 2**: Core backend API (1 day)
  - `GET /{guid}/notes` — returns all notes for a GUID.
  - `POST /{guid}/notes` — creates a new note with server timestamp.
  - Local-file JSON storage with safe concurrent writes.
  - GUID validation (400 on invalid format).

- **Phase 3**: Core frontend (1.5 days)
  - GUID generation & redirect on root visit.
  - Note list view (reverse-chronological, read-only).
  - New note input with "Save" button (disabled when empty).
  - "Use as basis" action to pre-fill the text area.

- **Phase 4**: Polish & demo readiness (0.5 days)
  - Responsive styling, locked-note visual treatment.
  - Local timestamp formatting.
  - README with setup and run instructions.
  - Basic error handling (invalid GUID, empty note, network error).

---

## 10. User stories

### 10.1. New user gets a personal URL

- **ID**: NT-001
- **Description**: As a first-time visitor, I want to be automatically assigned a unique URL so that I have a private space for my notes without signing up.
- **Acceptance criteria**:
  - Visiting `/` redirects the browser to `/{new-uuid-v4}`.
  - The generated value is a valid UUID v4.
  - Refreshing the redirected URL does not generate another GUID.

### 10.2. View empty note list

- **ID**: NT-002
- **Description**: As a new note owner, I want to see an empty note list and a note input area so that I can start writing immediately.
- **Acceptance criteria**:
  - The page at `/{guid}` renders a text area and a "Save" button.
  - When no notes exist for the GUID, a friendly empty-state message is shown.
  - The "Save" button is disabled when the text area is empty or contains only whitespace.

### 10.3. Create a note

- **ID**: NT-003
- **Description**: As a note owner, I want to type content and save it as a note so that my thought is permanently recorded with a timestamp.
- **Acceptance criteria**:
  - Clicking "Save" with non-empty content sends a `POST /{guid}/notes` request.
  - The backend responds with the saved note including a UTC ISO 8601 timestamp.
  - The new note appears at the top of the list without a page reload.
  - The text area is cleared after a successful save.
  - The note is persisted to `data/{guid}.json` on the server.

### 10.4. View all notes in reverse-chronological order

- **ID**: NT-004
- **Description**: As a returning note owner, I want to see all my notes ordered newest-first so that the most recent entry is immediately visible.
- **Acceptance criteria**:
  - `GET /{guid}/notes` returns all notes for the GUID sorted by timestamp descending.
  - Each note displays its date, time (converted to local timezone in the browser), and content.
  - No edit or delete controls are present on any note.

### 10.5. Notes are permanently read-only after saving

- **ID**: NT-005
- **Description**: As a note owner, I want existing notes to be locked so that the historical record is never altered.
- **Acceptance criteria**:
  - There is no API endpoint to update or delete a note.
  - The frontend renders no edit or delete affordances on saved notes.
  - Saved notes are visually distinguishable from the input area (e.g., different background).

### 10.6. Create a follow-up note based on an existing one

- **ID**: NT-006
- **Description**: As a note owner, I want to use an existing note as the starting point for a new one so that I can refine my earlier thoughts without modifying the original.
- **Acceptance criteria**:
  - Each note has a "Use as basis" (or equivalent) action button.
  - Clicking it populates the text area with the content of that note.
  - Saving creates a new note with the current server timestamp; the original note is unchanged.
  - The new note and the original note coexist in the list as separate entries.

### 10.7. Persist notes across sessions

- **ID**: NT-007
- **Description**: As a returning note owner, I want my notes to still be there when I revisit my URL so that my history is never lost.
- **Acceptance criteria**:
  - Notes written in a previous session are returned by `GET /{guid}/notes` in a new session.
  - Notes survive a backend restart (data is on disk, not in memory).

### 10.8. Handle invalid GUID gracefully

- **ID**: NT-008
- **Description**: As a user who navigates to a malformed URL, I want a clear error message so that I understand the URL is not valid.
- **Acceptance criteria**:
  - If `{guid}` in the URL is not a valid UUID v4, the backend returns HTTP 400.
  - The frontend displays a user-friendly error message (not a raw JSON error).

### 10.9. Start the full application with a single command

- **ID**: NT-009
- **Description**: As a developer, I want to start both the backend and frontend with one command so that setup is fast and frictionless.
- **Acceptance criteria**:
  - A documented single command (e.g., `make start`, `npm run start`) launches both the ASP.NET backend and the React dev server.
  - Both processes are visible in the terminal output (multiplexed or side-by-side).
  - The command works on Windows and Linux/macOS.

### 10.10. Concurrent write safety

- **ID**: NT-010
- **Description**: As a note owner accessing the app from multiple tabs, I want all saved notes to be recorded without data loss or file corruption.
- **Acceptance criteria**:
  - Simultaneous `POST /{guid}/notes` requests do not corrupt the JSON file.
  - All notes from concurrent requests are present in the file after all requests complete.
