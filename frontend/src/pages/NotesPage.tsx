import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getNotes, createNote } from '../api/notesApi';
import type { Note } from '../types';
import NoteInput from '../components/NoteInput';
import NoteList from '../components/NoteList';
import styles from './NotesPage.module.css';

/**
 * NotesPage is the main view for a user's private note feed.
 *
 * - Reads the `:guid` route parameter to scope all API calls.
 * - Fetches existing notes on mount (and refreshes after each save).
 * - Renders {@link NoteInput} at the top and {@link NoteList} below.
 * - Handles loading, network errors, and invalid GUID (400) gracefully.
 */
export default function NotesPage() {
  const { guid } = useParams<{ guid: string }>();

  const [notes, setNotes] = useState<Note[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  // Text pre-filled by the "Use as basis" action on any existing note.
  const [prefillContent, setPrefillContent] = useState('');

  // Load notes whenever the GUID changes (e.g., direct navigation).
  useEffect(() => {
    if (!guid) return;

    setIsLoading(true);
    setError(null);

    getNotes(guid)
      .then(setNotes)
      .catch((err: Error) => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [guid]);

  /**
   * Saves a new note and prepends it to the local list without a full refetch,
   * so the UI updates instantly.
   */
  async function handleSave(content: string): Promise<void> {
    if (!guid) return;

    const created = await createNote(guid, { content });
    // Prepend so the newest note is always at the top.
    setNotes((prev) => [created, ...prev]);
  }

  /**
   * Pre-fills the NoteInput textarea with the content of an existing note.
   * Called by the "Use as basis" button on each NoteCard.
   */
  function handleUseAsBasis(content: string): void {
    setPrefillContent(content);
    // Scroll to the top so the user sees the populated input.
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // A 400-style error means the GUID in the URL is malformed.
  const isInvalidGuid = error?.includes('400') || error?.includes('Invalid GUID');

  return (
    <div className={styles.page}>
      {/* GitHub-style dark top navigation bar */}
      <header className={styles.header}>
        {/* Inline SVG of the GitHub mark (Octocat silhouette) */}
        <span className={styles.headerLogo} aria-label="Note-Taker">
          <svg height="32" width="32" viewBox="0 0 16 16" aria-hidden="true">
            <path d="M8 0C3.58 0 0 3.58 0 8a8 8 0 005.47 7.59c.4.07.55-.17.55-.38
              0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13
              -.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66
              .07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15
              -.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82a7.66 7.66 0 014 0c1.53-1.04
              2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87
              3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55
              .38A8.01 8.01 0 0016 8c0-4.42-3.58-8-8-8z" />
          </svg>
        </span>
        <h1 className={styles.title}>Note-Taker</h1>
        <span className={styles.subtitle}>Your private, timestamped note feed</span>
      </header>

      {/* Light sub-header — mimics GitHub's repo breadcrumb bar */}
      <div className={styles.subHeader}>
        <div className={styles.subHeaderInner}>
          <span>📝</span>
          <span>Personal notes workspace</span>
        </div>
      </div>

      <main className={styles.main}>
        {isInvalidGuid ? (
          <div className={styles.errorBox} role="alert">
            <strong>Invalid URL</strong>
            <p>The link you followed does not contain a valid ID. Please go back to the home page to get a new one.</p>
          </div>
        ) : (
          <>
            <NoteInput
              initialContent={prefillContent}
              onContentConsumed={() => setPrefillContent('')}
              onSave={handleSave}
            />

            {error && !isInvalidGuid && (
              <div className={styles.errorBox} role="alert">
                <strong>Error loading notes:</strong> {error}
              </div>
            )}

            {isLoading ? (
              <p className={styles.status}>Loading notes…</p>
            ) : (
              <NoteList notes={notes} onUseAsBasis={handleUseAsBasis} />
            )}
          </>
        )}
      </main>
    </div>
  );
}
