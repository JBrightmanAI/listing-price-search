import { keepPreviousData, skipToken, useQuery, type UseQueryResult } from '@tanstack/react-query'
import { searchListings, toApiError, type ApiError, type SearchParams, type SearchResponse } from '@/api'
import { listingKeys } from './listingKeys'

/** The view-facing contract. Components depend on this, not on React Query, so the data layer can change freely. */
export type SearchResult =
  | { status: 'idle' }
  | { status: 'loading'; previous?: SearchResponse | undefined }
  | { status: 'success'; data: SearchResponse }
  | { status: 'error'; error: ApiError }

/**
 * Fetches results for `params`; null means "do not search".
 *
 * React Query provides: one cached entry per distinct search (back/forward is instant),
 * deduplication of identical in-flight requests, cancellation of requests nobody is waiting for
 * (the AbortSignal is passed through to axios), and retries for transient failures only.
 * keepPreviousData keeps the current page on screen while the next one loads.
 */
export function useListingSearch(params: SearchParams | null) {
  const query = useQuery({
    queryKey: listingKeys.search(params),
    queryFn: params ? ({ signal }) => searchListings(params, signal) : skipToken,
    placeholderData: keepPreviousData,
  })

  const retry = () => void query.refetch()

  return { result: toSearchResult(params, query), retry }
}

type QueryState = Pick<
  UseQueryResult<SearchResponse>,
  'data' | 'error' | 'isError' | 'isFetching' | 'isPlaceholderData'
>

function toSearchResult(params: SearchParams | null, query: QueryState): SearchResult {
  if (!params) return { status: 'idle' }
  // Retrying after an error: show progress, not the stale error.
  if (query.isError && query.isFetching) return { status: 'loading' }
  if (query.isError) return { status: 'error', error: toApiError(query.error) }
  if (query.isPlaceholderData) return { status: 'loading', previous: query.data }
  if (query.data) return { status: 'success', data: query.data }
  return { status: 'loading' }
}
