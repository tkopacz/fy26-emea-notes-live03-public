import { BrowserRouter, Route, Routes } from 'react-router-dom';
import RootPage from './pages/RootPage';
import NotesPage from './pages/NotesPage';

/**
 * App sets up the client-side router.
 *
 * Routes:
 *   /         → RootPage  (generates a UUID and redirects to /:guid)
 *   /:guid    → NotesPage (the user's private note feed)
 */
export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<RootPage />} />
        <Route path="/:guid" element={<NotesPage />} />
      </Routes>
    </BrowserRouter>
  );
}
