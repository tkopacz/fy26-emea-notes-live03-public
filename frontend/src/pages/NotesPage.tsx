import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getNotes, createNote } from '../api/notesApi';
import type { Note } from '../types';
import NoteInput from '../components/NoteInput';
import NoteList from '../components/NoteList';
import styles from './NotesPage.module.css';

/**
 * NotesPage is a thin wrapper that reads the `:guid` route parameter and passes it
 * to {@link NotesPageContent} as both a prop and a React `key`.
 *
 * Using the guid as a `key` forces a full unmount + remount of the inner component
 * whenever the user navigates to a different GUID.  This automatically resets all
 * child state (notes list, loading flag, error message) back to their initial values
 * without any synchronous setState calls inside a useEffect.
 *
 * This is the React-recommended pattern for "resetting all state when a prop changes":
 * https://react.dev/learn/you-might-not-need-an-effect#resetting-all-state-when-a-prop-changes
 */
export default function NotesPage() {
  const { guid } = useParams<{ guid: string }>();
  // The key ensures NotesPageContent remounts (and its state resets) on GUID changes.
  return <NotesPageContent guid={guid} key={guid ?? 'no-guid'} />;
}

/**
 * NotesPageContent is the main view for a user's private note feed.
 *
 * It receives `guid` from its parent wrapper (already extracted from the URL).
 * Because the parent passes `guid` as a React `key`, this component always mounts
 * fresh for each unique GUID, so `isLoading` starts as `true` and there is no need
 * to reset state synchronously inside useEffect (which would violate the
 * react-hooks/set-state-in-effect lint rule).
 *
 * Responsibilities:
 * - Fetches existing notes on mount (all setState calls are in async callbacks).
 * - Renders {@link NoteInput} at the top and {@link NoteList} below.
 * - Handles loading, network errors, and invalid GUID (400) gracefully.
 * - Supports the "Use as basis" flow by passing prefill content down to NoteInput.
 */
function NotesPageContent({ guid }: { guid: string | undefined }) {
  // The list of notes fetched from the server for this GUID.
  const [notes, setNotes] = useState<Note[]>([]);
  // isLoading is initialised as true so the loading indicator appears immediately
  // on mount (and on every remount triggered by a GUID change).
  const [isLoading, setIsLoading] = useState(true);
  // Holds the last fetch error message; null means no error.
  const [error, setError] = useState<string | null>(null);
  // Text to pre-fill in the NoteInput textarea (set by the "Use as basis" action).
  const [prefillContent, setPrefillContent] = useState('');

  /**
   * Fetch the notes for this GUID when the component mounts.
   *
   * Because the parent remounts this component whenever the GUID changes (via the
   * `key` prop), this effect runs exactly once per unique GUID — no synchronous
   * state resets are needed inside the effect body, satisfying the
   * react-hooks/set-state-in-effect rule.
   *
   * All setState calls happen inside async Promise callbacks, not synchronously
   * in the effect body.
   */
  useEffect(() => {
    if (!guid) return;

    // All state transitions happen inside async callbacks to comply with the
    // react-hooks/set-state-in-effect lint rule (no synchronous setState in effects).
    getNotes(guid)
      .then((data) => {
        // Populate the notes list and clear any previous error on success.
        setNotes(data);
        setError(null);
      })
      .catch((err: Error) => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [guid]);

  /**
   * Saves a new note and prepends it to the local list without a full refetch,
   * so the UI updates instantly without a loading flash.
   */
  async function handleSave(content: string): Promise<void> {
    if (!guid) return;

    const created = await createNote(guid, { content });
    // Prepend so the newest note is always at the top (matches API sort order).
    setNotes((prev) => [created, ...prev]);
  }

  /**
   * Pre-fills the NoteInput textarea with the content of an existing note.
   * Called when the user clicks "Use as basis" on a NoteCard.
   */
  function handleUseAsBasis(content: string): void {
    setPrefillContent(content);
    // Scroll to the top so the pre-filled input is immediately visible.
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // A 400-style error response means the GUID in the URL is malformed / not a valid UUID.
  const isInvalidGuid = error?.includes('400') || error?.includes('Invalid GUID');

  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1 className={styles.title}>📝 Note-Taker</h1>
        <p className={styles.subtitle}>Your private, timestamped note feed</p>
      </header>

      <main className={styles.main}>
        {isInvalidGuid ? (
          // Show a clear message when the URL contains a non-UUID slug so the user
          // knows to navigate back and generate a valid link.
          <div className={styles.errorBox} role="alert">
            <strong>Invalid URL</strong>
            <p>The link you followed does not contain a valid ID. Please go back to the home page to get a new one.</p>
          </div>
        ) : (
          <>
            {/*
             * NoteInput is always rendered, even while notes are loading, so the user
             * can start writing immediately (NT-002 AC: "renders a text area and a Save button").
             */}
            <NoteInput
              initialContent={prefillContent}
              onContentConsumed={() => setPrefillContent('')}
              onSave={handleSave}
            />

            {/* Show network / server errors below the input (but not for invalid-GUID errors,
                which are handled by the branch above). */}
            {error && !isInvalidGuid && (
              <div className={styles.errorBox} role="alert">
                <strong>Error loading notes:</strong> {error}
              </div>
            )}

            {isLoading ? (
              <p className={styles.status}>Loading notes…</p>
            ) : (
              // NoteList shows a friendly empty-state message when notes is empty
              // (NT-002 AC: "a friendly empty-state message is shown").
              <NoteList notes={notes} onUseAsBasis={handleUseAsBasis} />
            )}
          </>
        )}
      </main>
    </div>
  );
}
