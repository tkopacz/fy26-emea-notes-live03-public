import type { Note, CreateNoteRequest } from '../types';

/**
 * Base URL for all API requests.
 * In development, Vite proxies /api → http://localhost:5000, stripping the prefix.
 * In production, set VITE_API_URL to the deployed backend origin.
 */
const API_BASE = import.meta.env.VITE_API_URL ?? '/api';

/**
 * Fetches all notes for the given user GUID, sorted newest-first.
 *
 * @param guid - The user's UUID v4 identifier.
 * @returns An array of {@link Note} objects.
 * @throws An error with a descriptive message if the request fails.
 */
export async function getNotes(guid: string): Promise<Note[]> {
  const response = await fetch(`${API_BASE}/${guid}/notes`);

  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    // Fehlermeldung enthält immer den HTTP-Statuscode, damit die aufrufende Seite
    // 400-Fehler zuverlässig erkennen kann.
    // Error message always includes the HTTP status code so callers can reliably
    // detect 400 responses.
    throw new Error(
      `${body?.detail ?? 'Failed to load notes'} (HTTP ${response.status})`,
    );
  }

  return response.json() as Promise<Note[]>;
}

/**
 * Creates a new note for the given user GUID.
 *
 * @param guid - The user's UUID v4 identifier.
 * @param request - The request body containing the note content.
 * @returns The newly created {@link Note} with its server-assigned timestamp.
 * @throws An error with a descriptive message if the request fails.
 */
export async function createNote(guid: string, request: CreateNoteRequest): Promise<Note> {
  const response = await fetch(`${API_BASE}/${guid}/notes`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    // Fehlermeldung enthält immer den HTTP-Statuscode, damit die aufrufende Seite
    // 400-Fehler zuverlässig erkennen kann.
    // Error message always includes the HTTP status code so callers can reliably
    // detect 400 responses.
    throw new Error(
      `${body?.detail ?? 'Failed to save note'} (HTTP ${response.status})`,
    );
  }

  return response.json() as Promise<Note>;
}
