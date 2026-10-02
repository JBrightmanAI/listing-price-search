import type { ReactElement } from 'react'
import { render } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AppProviders, createQueryClient } from '@/app'

/**
 * Renders inside the real app providers with a fresh query cache and no automatic retries,
 * so every test is isolated and failures surface immediately.
 */
export function renderWithProviders(ui: ReactElement) {
  const queryClient = createQueryClient({ retry: false })
  return {
    user: userEvent.setup(),
    queryClient,
    ...render(<AppProviders queryClient={queryClient}>{ui}</AppProviders>),
  }
}
