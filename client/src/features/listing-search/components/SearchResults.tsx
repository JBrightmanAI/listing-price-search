import FilterAltOffOutlined from '@mui/icons-material/FilterAltOffOutlined'
import SearchOffOutlined from '@mui/icons-material/SearchOffOutlined'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import { EmptyState, ErrorState } from '@/components'
import { type SearchResult } from '@/features/listing-search/hooks'
import { ListingResults, ResultsSkeleton } from './ListingResults'
import { ResultsPagination } from './ResultsPagination'

interface Props {
  result: SearchResult
  /** The filters hold invalid values, so no search was (or will be) sent. */
  invalid: boolean
  hasFilters: boolean
  onRetry: () => void
  onPageChange: (page: number) => void
  onClearFilters: () => void
}

const INVALID_MESSAGE = 'Some search values are invalid. Check the highlighted fields.'

/** Maps every search outcome to exactly one view. The switch is exhaustive, so a new status will not compile until handled. */
export function SearchResults({ result, invalid, hasFilters, onRetry, onPageChange, onClearFilters }: Props) {
  switch (result.status) {
    case 'idle':
      return invalid ? <Alert severity="warning">{INVALID_MESSAGE}</Alert> : null

    case 'loading':
      return result.previous ? <ListingResults listings={result.previous.items} busy /> : <ResultsSkeleton />

    case 'error': {
      const { error } = result
      if (error.kind === 'validation') {
        return <Alert severity="warning">{INVALID_MESSAGE}</Alert>
      }
      const wait = error.retryAfterSeconds === undefined ? '' : ` Try again in ${error.retryAfterSeconds} seconds.`
      return <ErrorState title="Search failed" message={`${error.message}${wait}`} onRetry={onRetry} />
    }

    case 'success': {
      const { items, page, totalPages, totalCount } = result.data

      if (totalCount === 0) {
        return (
          <EmptyState
            icon={<SearchOffOutlined />}
            title="No listings match your search"
            description="Try widening the price range, lowering the bedroom count, or removing a filter."
            action={
              hasFilters && (
                <Button variant="outlined" startIcon={<FilterAltOffOutlined />} onClick={onClearFilters}>
                  Clear filters
                </Button>
              )
            }
          />
        )
      }

      // A valid but out-of-range page (an old link, or results shrank since it was shared).
      if (items.length === 0) {
        return (
          <EmptyState
            title={`Page ${page} is past the end of the results`}
            description={`There ${totalPages === 1 ? 'is 1 page' : `are ${totalPages} pages`} of results.`}
            action={
              <Button variant="contained" onClick={() => onPageChange(totalPages)}>
                Go to last page
              </Button>
            }
          />
        )
      }

      return (
        <>
          <ListingResults listings={items} />
          <ResultsPagination page={page} totalPages={totalPages} totalCount={totalCount} onPageChange={onPageChange} />
        </>
      )
    }
  }
}
