import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getNotes, createNote } from '../api/notesApi';
import type { Note } from '../types';
import NoteInput from '../components/NoteInput';
import NoteList from '../components/NoteList';
import styles from './NotesPage.module.css';

/**
 * Regulärer Ausdruck zur Überprüfung, ob ein Zeichenketten-Parameter dem
 * UUID-Format entspricht (beliebige Version, konsistent mit der Backend-Validierung).
 *
 * Regular expression that checks whether a string matches the UUID format
 * (any version, consistent with backend validation).
 */
const UUID_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

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

  // Ungültige GUIDs werden sofort anhand des URL-Parameters erkannt,
  // ohne einen unnötigen API-Aufruf auszulösen.
  // Invalid GUIDs are detected immediately from the URL parameter,
  // without triggering an unnecessary API call.
  const guidIsValid = !!guid && UUID_PATTERN.test(guid);

  const [notes, setNotes] = useState<Note[]>([]);
  // Start in loading state only when the GUID is already known to be valid,
  // avoiding an extra setState call inside the effect for invalid GUIDs.
  const [isLoading, setIsLoading] = useState(guidIsValid);
  const [error, setError] = useState<string | null>(null);
  // Text pre-filled by the "Use as basis" action on any existing note.
  const [prefillContent, setPrefillContent] = useState('');

  // Load notes whenever the GUID changes (e.g., direct navigation).
  useEffect(() => {
    if (!guidIsValid) return;

    setIsLoading(true);
    setError(null);

    getNotes(guid!)
      .then(setNotes)
      .catch((err: Error) => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [guid, guidIsValid]);

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

  // Ungültige GUID: entweder direkt anhand des URL-Formats erkannt oder durch
  // einen HTTP-400-Fehler der API bestätigt.
  // Invalid GUID: detected either directly from the URL format or confirmed by
  // an HTTP 400 error from the API.
  const isInvalidGuid =
    !guidIsValid || error?.includes('400') || error?.includes('Invalid GUID');

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
