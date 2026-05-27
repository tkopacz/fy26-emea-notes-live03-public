import type { Note } from '../types';
import styles from './NoteCard.module.css';

interface NoteCardProps {
  note: Note;
  /** Called when the user clicks "Use as basis" to pre-fill the note input. */
  onUseAsBasis: (content: string) => void;
}

/**
 * NoteCard displays a single read-only note.
 *
 * - Timestamp is converted from UTC to the browser's local timezone.
 * - Visual treatment (border + background) reinforces immutability.
 * - "Use as basis" button populates the new-note textarea with this note's content.
 */
export default function NoteCard({ note, onUseAsBasis }: NoteCardProps) {
  // Format the UTC ISO timestamp in the user's local timezone for readability.
  const formattedDate = new Date(note.createdAt).toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });

  return (
    <article className={styles.card} aria-label={`Note from ${formattedDate}`}>
      <header className={styles.header}>
        <time className={styles.timestamp} dateTime={note.createdAt}>
          {formattedDate}
        </time>
        <span className={styles.lockedBadge} aria-label="Read-only note">🔒 Saved</span>
      </header>

      {/* Pre-wrap preserves line breaks in the note content. */}
      <p className={styles.content}>{note.content}</p>

      <footer className={styles.footer}>
        <button
          className={styles.basisButton}
          onClick={() => onUseAsBasis(note.content)}
          aria-label="Use this note as the basis for a new one"
        >
          Use as basis
        </button>
      </footer>
    </article>
  );
}
