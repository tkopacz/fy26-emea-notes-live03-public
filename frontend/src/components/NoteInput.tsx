import { useRef, useState } from 'react';
import styles from './NoteInput.module.css';

interface NoteInputProps {
  /** Content to pre-fill the textarea (from "Use as basis" action). */
  initialContent: string;
  /**
   * Called when the user clicks "Save".
   * Returns a Promise so the component can show a saving state.
   */
  onSave: (content: string) => Promise<void>;
}

/**
 * NoteInput renders the new-note creation area at the top of the notes page.
 *
 * - Textarea is auto-focused on mount.
 * - Save button is disabled until the user types at least one non-whitespace character.
 * - Clears the textarea and re-focuses after a successful save.
 * - Accepts pre-filled content via the `initialContent` prop when the parent
 *   gives us a fresh instance after "Use as basis" is clicked.
 */
export default function NoteInput({ initialContent, onSave }: NoteInputProps) {
  // Start with the basis note text when this input is freshly mounted.
  const [content, setContent] = useState(initialContent);
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const canSave = content.trim().length > 0 && !isSaving;

  async function handleSave() {
    if (!canSave) return;

    setIsSaving(true);
    setSaveError(null);

    try {
      await onSave(content.trim());
      setContent('');
      textareaRef.current?.focus();
    } catch (err) {
      setSaveError(err instanceof Error ? err.message : 'Failed to save note.');
    } finally {
      setIsSaving(false);
    }
  }

  /** Allow Ctrl+Enter / Cmd+Enter as keyboard shortcut for saving. */
  function handleKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
      handleSave();
    }
  }

  return (
    <section className={styles.container} aria-label="New note">
      <textarea
        ref={textareaRef}
        className={styles.textarea}
        value={content}
        onChange={(e) => setContent(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Write a new note… (Ctrl+Enter to save)"
        rows={4}
        disabled={isSaving}
        autoFocus
      />

      <div className={styles.footer}>
        {saveError && (
          <span className={styles.saveError} role="alert">
            {saveError}
          </span>
        )}
        <button
          className={styles.saveButton}
          onClick={handleSave}
          disabled={!canSave}
          aria-label="Save note"
        >
          {isSaving ? 'Saving…' : 'Save'}
        </button>
      </div>
    </section>
  );
}
