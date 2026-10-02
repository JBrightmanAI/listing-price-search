import { useCallback, useEffect, useState } from 'react'
import { fromUrlQuery, toUrlQuery, type SearchState } from '@/features/listing-search/model'

/**
 * The URL is the source of truth for the applied search, so a search can be shared or bookmarked
 * and the browser's back and forward buttons move between searches and pages.
 */
export function useSearchUrlState() {
  const [state, setState] = useState<SearchState>(() => fromUrlQuery(window.location.search))

  useEffect(() => {
    const onPopState = () => setState(fromUrlQuery(window.location.search))
    window.addEventListener('popstate', onPopState)
    return () => window.removeEventListener('popstate', onPopState)
  }, [])

  const navigate = useCallback((next: SearchState) => {
    const query = toUrlQuery(next)
    window.history.pushState(null, '', query ? `?${query}` : window.location.pathname)
    setState(next)
  }, [])

  return [state, navigate] as const
}
