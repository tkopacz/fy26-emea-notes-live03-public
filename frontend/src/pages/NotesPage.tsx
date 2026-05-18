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
      <header className={styles.header}>
        <h1 className={styles.title}>📝 Note-Taker</h1>
        <p className={styles.subtitle}>Your private, timestamped note feed</p>
      </header>

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
