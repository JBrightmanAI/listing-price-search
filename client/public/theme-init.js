// Applies the saved (or system) colour scheme before React loads, so dark-mode users never see a light flash.
// Mirrors MUI's InitColorSchemeScript, but as a same-origin file: the Content-Security-Policy forbids inline scripts.
// Keep the storage key and class names in sync with src/app/theme.ts (colorSchemeSelector: 'class').
;(function () {
  try {
    const mode = localStorage.getItem('mui-mode') || 'system'
    const dark = mode === 'dark' || (mode === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches)
    const root = document.documentElement
    root.classList.remove('light', 'dark')
    root.classList.add(dark ? 'dark' : 'light')
  } catch {
    // Storage blocked (privacy mode): React applies the scheme a moment later instead.
  }
})()
