import { getJson } from './http'
import { type SearchParams, type SearchResponse } from './types'

/**
 * The signal is required on purpose: every caller must be able to cancel a search it no longer needs.
 * axios omits undefined params, so unset filters never reach the query string.
 */
export function searchListings(params: SearchParams, signal: AbortSignal): Promise<SearchResponse> {
  return getJson<SearchResponse>('/api/listings/search', { params, signal })
}
