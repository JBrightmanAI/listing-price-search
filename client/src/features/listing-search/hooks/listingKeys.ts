import type { SearchParams } from '@/api'

/**
 * Query keys for listings, built in one place so caching and invalidation stay consistent.
 * The params object is part of the key: identical searches share one cached result.
 */
export const listingKeys = {
  all: ['listings'] as const,
  search: (params: SearchParams | null) => [...listingKeys.all, 'search', params] as const,
}
