import { useState, type ReactNode } from 'react'
import CssBaseline from '@mui/material/CssBaseline'
import { ThemeProvider } from '@mui/material/styles'
import { QueryClientProvider, type QueryClient } from '@tanstack/react-query'
import { createQueryClient } from './queryClient'
import { theme } from './theme'

interface Props {
  children: ReactNode
  /** Tests pass their own client so each starts with an empty cache. */
  queryClient?: QueryClient | undefined
}

/** Every app-wide provider in one place, reused by tests so components render as they do in the app. */
export function AppProviders({ children, queryClient }: Props) {
  // Created once per mount, not per render, so the cache survives re-renders.
  const [defaultClient] = useState(createQueryClient)

  return (
    <QueryClientProvider client={queryClient ?? defaultClient}>
      {/* "system" follows the OS until the user picks a mode; the choice is saved in localStorage. */}
      <ThemeProvider theme={theme} defaultMode="system">
        <CssBaseline enableColorScheme />
        {children}
      </ThemeProvider>
    </QueryClientProvider>
  )
}
