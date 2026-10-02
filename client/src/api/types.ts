// Mirrors the API contract in src/ListingSearch.Api/Contracts.

export type ListingStatus = 'active' | 'pending' | 'sold'

export interface Listing {
  id: string
  source: string
  address: string
  city: string
  state: string
  zip: string
  price: number
  bedrooms: number
  bathrooms: number
  sqft: number
  latitude: number
  longitude: number
  listedDate: string
  status: ListingStatus
  description: string
  relevanceScore: number
}

export interface SearchResponse {
  items: Listing[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

/** undefined means "filter not applied"; axios leaves undefined params out of the query string. */
export interface SearchParams {
  minPrice?: number | undefined
  maxPrice?: number | undefined
  minBedrooms?: number | undefined
  city?: string | undefined
  keyword?: string | undefined
  targetBudget?: number | undefined
  page: number
  pageSize: number
}

/** Validation messages keyed by query parameter name, as returned in ValidationProblemDetails. */
export type FieldErrors = Partial<Record<string, string[]>>
