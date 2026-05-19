import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getNotes, createNote } from '../api/notesApi';
import type { Note } from '../types';
import NoteInput from '../components/NoteInput';
import NoteList from '../components/NoteList';
import styles from './NotesPage.module.css';

interface NotesLoadState {
  guid: string | undefined;
  notes: Note[];
  error: string | null;
  status: 'idle' | 'loading' | 'ready' | 'error';
}

interface BasisDraftState {
  content: string;
  version: number;
}

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

  const [loadState, setLoadState] = useState<NotesLoadState>({
    guid,
    notes: [],
    error: null,
    status: guid ? 'loading' : 'idle',
  });
  // Keep a version number so clicking the same note again still refreshes the input.
  const [basisDraft, setBasisDraft] = useState<BasisDraftState>({
    content: '',
    version: 0,
  });

  // Load notes whenever the GUID changes (e.g., direct navigation).
  useEffect(() => {
    if (!guid) return;

    let isCancelled = false;

    getNotes(guid)
      .then((notes) => {
        if (isCancelled) return;

        setLoadState({
          guid,
          notes,
          error: null,
          status: 'ready',
        });
      })
      .catch((err: Error) => {
        if (isCancelled) return;

        setLoadState({
          guid,
          notes: [],
          error: err.message,
          status: 'error',
        });
      });

    return () => {
      isCancelled = true;
    };
  }, [guid]);

  const notes = loadState.guid === guid ? loadState.notes : [];
  const error = loadState.guid === guid ? loadState.error : null;
  const isLoading = Boolean(guid) && (loadState.guid !== guid || loadState.status === 'loading');

  /**
   * Saves a new note and prepends it to the local list without a full refetch,
   * so the UI updates instantly.
   */
  async function handleSave(content: string): Promise<void> {
    if (!guid) return;

    const created = await createNote(guid, { content });
    // Prepend so the newest note is always at the top.
    setLoadState((prev) => {
      if (prev.guid !== guid) {
        return prev;
      }

      return {
        guid,
        notes: [created, ...prev.notes],
        error: null,
        status: 'ready',
      };
    });
  }

  /**
   * Pre-fills the NoteInput textarea with the content of an existing note.
   * Called by the "Use as basis" button on each NoteCard.
   */
  function handleUseAsBasis(content: string): void {
    setBasisDraft((prev) => ({
      content,
      version: prev.version + 1,
    }));
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
              key={basisDraft.version}
              initialContent={basisDraft.content}
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
