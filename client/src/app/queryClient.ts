import { QueryClient, type DefaultOptions } from '@tanstack/react-query'
import { toApiError } from '@/api'

const MAX_RETRIES = 2

/**
 * Retry only failures that a second attempt can fix: lost connections, timeouts and 5xx.
 * A 400 will fail the same way again, and a 429 retried immediately only deepens the throttling.
 */
export function shouldRetry(failureCount: number, error: unknown): boolean {
  if (failureCount >= MAX_RETRIES) return false
  const { kind, status } = toApiError(error)
  return kind === 'network' || kind === 'timeout' || (kind === 'server' && (status === undefined || status >= 500))
}

const defaults: DefaultOptions = {
  queries: {
    retry: shouldRetry,
    // Listings change on feed ingestion, not second to second; 30s of reuse makes back/forward instant.
    staleTime: 30_000,
    refetchOnWindowFocus: false,
  },
}

/** One client per app (or per test, so cached results never leak between tests). */
export function createQueryClient(overrides: DefaultOptions['queries'] = {}) {
  return new QueryClient({ defaultOptions: { queries: { ...defaults.queries, ...overrides } } })
}
