import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { v4 as uuidv4 } from 'uuid';

/**
 * RootPage handles the entry-point route "/".
 *
 * On mount it generates a fresh UUID v4 and immediately redirects the browser
 * to "/{guid}", giving the new visitor their private note space.
 * The redirect is done via `replace` so the empty root is not added to history.
 */
export default function RootPage() {
  const navigate = useNavigate();

  useEffect(() => {
    const newGuid = uuidv4();
    // Replace current history entry so the back button skips the root.
    navigate(`/${newGuid}`, { replace: true });
  }, [navigate]);

  // Render nothing — the redirect is instant.
  return null;
}
