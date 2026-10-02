import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

// jsdom implements neither of these browser APIs. matchMedia answers "no match" for every query:
// light system theme, desktop layout, motion allowed.
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(() => false),
  })),
})
Element.prototype.scrollIntoView = vi.fn()

// Vitest runs without globals here, so Testing Library cannot register its own cleanup.
afterEach(() => {
  cleanup()
  window.history.replaceState(null, '', '/')
  // The theme mode persists in localStorage and as a class on <html>; reset both between tests.
  localStorage.clear()
  document.documentElement.className = ''
})
