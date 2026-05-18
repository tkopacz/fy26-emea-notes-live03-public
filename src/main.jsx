import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import { resolveNotePath } from './notePath.js'

const notePath = resolveNotePath(window.location.pathname)

if (notePath.redirected) {
  window.history.replaceState(window.history.state, '', notePath.path)
}

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App notePath={notePath.path} />
  </StrictMode>,
)
