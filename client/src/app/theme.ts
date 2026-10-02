import { createTheme } from '@mui/material/styles'

/**
 * Light and dark palettes generated as CSS variables. Switching modes swaps a class on <html>
 * instead of re-rendering the tree, and public/theme-init.js applies that class before React loads.
 */
export const theme = createTheme({
  cssVariables: { colorSchemeSelector: 'class' },
  colorSchemes: {
    light: {
      palette: {
        primary: { main: '#4f46e5' },
        secondary: { main: '#0d9488' },
        background: { default: '#f5f6fa', paper: '#ffffff' },
      },
    },
    dark: {
      palette: {
        primary: { main: '#a5b4fc' },
        secondary: { main: '#5eead4' },
        background: { default: '#0b1020', paper: '#121a2e' },
      },
    },
  },
  // System fonts: no web-font download, and nothing to allow from a font CDN in the Content-Security-Policy.
  typography: {
    fontFamily: 'system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
    h4: { fontWeight: 700, letterSpacing: '-0.02em' },
    h6: { fontWeight: 700 },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  shape: { borderRadius: 10 },
  components: {
    MuiButton: { defaultProps: { disableElevation: true } },
    MuiTableCell: {
      styleOverrides: {
        head: {
          color: 'var(--mui-palette-text-secondary)',
          fontSize: '0.75rem',
          fontWeight: 600,
          letterSpacing: '0.04em',
          textTransform: 'uppercase',
          whiteSpace: 'nowrap',
        },
      },
    },
  },
})
