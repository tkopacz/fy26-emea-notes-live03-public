import { useEffect, useRef, useState } from 'react';
import styles from './NoteInput.module.css';

interface NoteInputProps {
  /** Content to pre-fill the textarea (from "Use as basis" action). */
  initialContent: string;
  /** Called after the pre-fill content has been consumed so the parent can clear it. */
  onContentConsumed: () => void;
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
 * - Save button is disabled until the user types at least one non-whitespace character
 *   (satisfies NT-002 AC: "The Save button is disabled when the text area is empty or
 *   contains only whitespace").
 * - Clears the textarea and re-focuses after a successful save.
 * - Accepts pre-filled content via the `initialContent` prop (from "Use as basis").
 *
 * ## How prefill is handled (React-recommended patterns)
 *
 * Two React rules constrain how we can sync `initialContent` into local state:
 *   1. react-hooks/set-state-in-effect — calling setState synchronously inside
 *      useEffect causes cascading re-renders and is forbidden by the linter.
 *   2. react-hooks/refs — writing to a ref during render is also forbidden.
 *
 * Solution: use React's "adjust state during render" pattern for the state update,
 * and keep the non-state side effects (parent callback + focus) in a separate
 * useEffect that only runs when `initialContent` changes to a non-empty value.
 *
 * See: https://react.dev/learn/you-might-not-need-an-effect#adjusting-some-state-when-a-prop-changes
 */
export default function NoteInput({ initialContent, onContentConsumed, onSave }: NoteInputProps) {
  // The textarea's current text — typed by the user or pre-filled by the parent.
  const [content, setContent] = useState('');
  // True while the async save is in flight, used to disable the button and textarea.
  const [isSaving, setIsSaving] = useState(false);
  // Holds the last error message returned by onSave, shown inline below the textarea.
  const [saveError, setSaveError] = useState<string | null>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  /**
   * lastConsumedContent tracks the most recent value of `initialContent` that this
   * component has already consumed (adopted into the textarea).  Storing it in React
   * state lets React trigger a synchronous re-render during the current render pass
   * and commit the updated textarea value in one paint.
   *
   * Why we need this tracker:
   *   - When `initialContent` changes from '' → 'Note A', we adopt it.
   *   - When the parent resets it back to '' (after we call onContentConsumed), we
   *     clear our tracker so the same text can be used as a basis a second time.
   */
  const [lastConsumedContent, setLastConsumedContent] = useState('');

  // ---------------------------------------------------------------------------
  // Adjust state during render — NOT inside useEffect.
  //
  // React explicitly supports conditional setState calls in the render body as
  // long as they are guarded by a condition that becomes false after the update,
  // preventing infinite re-renders.
  //
  // How infinite re-renders are prevented here:
  //   - First render: `initialContent = "Note A"`, `lastConsumedContent = ""`
  //     → condition is true → we call setLastConsumedContent("Note A") and setContent("Note A").
  //   - React immediately re-renders with updated state:
  //     `lastConsumedContent = "Note A"` → `initialContent !== lastConsumedContent` is now FALSE
  //     → neither condition fires → no more setState → no more re-renders. ✓
  // ---------------------------------------------------------------------------

  if (initialContent && initialContent !== lastConsumedContent) {
    // A new prefill value has arrived (e.g., "Use as basis" was clicked).
    // Adopt it into the textarea state right now, during this render, so there is
    // no extra useEffect round-trip and no intermediate stale render for the user.
    setLastConsumedContent(initialContent);
    setContent(initialContent);
  }

  if (!initialContent && lastConsumedContent) {
    // The parent has cleared the prefill (it resets to '' after onContentConsumed fires).
    // Reset our tracker so the same text can be used as a basis again later.
    setLastConsumedContent('');
  }

  /**
   * Side-effect handler for the prefill event.
   *
   * This effect fires whenever `initialContent` changes to a non-empty value, which
   * is exactly when the parent sends a new "Use as basis" prefill.  The state update
   * (setContent / setLastConsumedContent) already happened synchronously in the render
   * body above; here we only perform non-state side effects:
   *   - Notify the parent to clear its prefillContent state.
   *   - Move focus to the textarea so the user can start editing immediately.
   *
   * Neither call here is a setState on this component, so the
   * react-hooks/set-state-in-effect rule is not violated.
   */
  useEffect(() => {
    if (initialContent) {
      // Tell the parent we have consumed the prefill so it resets to ''.
      onContentConsumed();
      textareaRef.current?.focus();
    }
  }, [initialContent, onContentConsumed]);

  // Save is only possible when there is at least one non-whitespace character
  // AND no save is already in progress.  This drives the disabled state of the
  // Save button (NT-002 acceptance criterion).
  const canSave = content.trim().length > 0 && !isSaving;

  /**
   * Trims the content, calls the parent's onSave handler, and resets the
   * textarea on success.  Shows an inline error on failure.
   */
  async function handleSave() {
    if (!canSave) return;

    setIsSaving(true);
    setSaveError(null);

    try {
      // Trim whitespace before sending to match the server-side validation rule
      // (the API returns 400 for whitespace-only content).
      await onSave(content.trim());
      // Clear the textarea and return focus so the user can write the next note.
      setContent('');
      textareaRef.current?.focus();
    } catch (err) {
      setSaveError(err instanceof Error ? err.message : 'Failed to save note.');
    } finally {
      setIsSaving(false);
    }
  }

  /** Allow Ctrl+Enter / Cmd+Enter as a keyboard shortcut for saving. */
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
        {/*
         * The button is disabled when `canSave` is false — i.e., when the textarea
         * is empty or contains only whitespace, or while a save is already in flight.
         * This directly satisfies NT-002 AC: "The Save button is disabled when the
         * text area is empty or contains only whitespace."
         */}
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
