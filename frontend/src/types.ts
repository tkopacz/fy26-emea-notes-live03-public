/**
 * Shared TypeScript types matching the backend Note model.
 */

/** A single immutable note as returned by the API. */
export interface Note {
  /** UUID v4 identifier assigned by the server. */
  id: string;
  /** Plain-text content of the note. */
  content: string;
  /** UTC timestamp (ISO 8601) when the note was saved on the server. */
  createdAt: string;
}

/** Shape of the POST body for creating a note. */
export interface CreateNoteRequest {
  content: string;
}
