import './App.css'
import { getNoteId } from './notePath.js'

function App({ notePath }) {
  const currentPath =
    typeof window === 'undefined' ? notePath : window.location.pathname
  const noteId = getNoteId(currentPath)

  return (
    <main className="app-shell">
      <section className="note-card">
        <p className="eyebrow">NT-001</p>
        <h1>Your private notes space is ready</h1>
        <p className="description">
          First-time visitors are assigned a bookmarkable URL that becomes their
          note space identifier.
        </p>
        <div className="note-id-block">
          <span className="label">Current URL path</span>
          <code>{currentPath}</code>
          <span className="label">Current URL identifier</span>
          <code>{noteId}</code>
        </div>
        <p className="hint">Save this URL to return to the same notes space later.</p>
      </section>
    </main>
  )
}

export default App
