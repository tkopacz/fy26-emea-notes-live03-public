import type { Note } from '../types';
import NoteCard from './NoteCard';
import styles from './NoteList.module.css';

interface NoteListProps {
  notes: Note[];
  /** Called when the user clicks "Use as basis" on a note. */
  onUseAsBasis: (content: string) => void;
}

/**
 * NoteList renders the read-only list of existing notes in reverse-chronological
 * order (the backend already sorts them; we display them as-is).
 * Shows a friendly empty-state message when no notes exist yet.
 */
export default function NoteList({ notes, onUseAsBasis }: NoteListProps) {
  if (notes.length === 0) {
    return (
      <div className={styles.emptyState}>
        <p>No notes yet — write your first one above!</p>
      </div>
    );
  }

  return (
    <section aria-label="Your notes">
      <ul className={styles.list}>
        {notes.map((note) => (
          <li key={note.id}>
            <NoteCard note={note} onUseAsBasis={onUseAsBasis} />
          </li>
        ))}
      </ul>
    </section>
  );
}
