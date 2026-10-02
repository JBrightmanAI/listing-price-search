import type { Listing, SearchResponse } from '@/api'

export function makeListing(overrides: Partial<Listing> = {}): Listing {
  return {
    id: 'A1',
    source: 'MLS_A',
    address: '123 Main St, Apt 4B',
    city: 'Springfield',
    state: 'VA',
    zip: '22150',
    price: 450000,
    bedrooms: 2,
    bathrooms: 1.5,
    sqft: 980,
    latitude: 38.7893,
    longitude: -77.1873,
    listedDate: '2026-08-29',
    status: 'active',
    description: 'Bright top-floor condo.',
    relevanceScore: 95.52,
    ...overrides,
  }
}

export function makeResponse(overrides: Partial<SearchResponse> = {}): SearchResponse {
  const items = overrides.items ?? [makeListing()]
  return {
    items,
    page: 1,
    pageSize: 10,
    totalCount: items.length,
    totalPages: items.length === 0 ? 0 : 1,
    ...overrides,
  }
}
