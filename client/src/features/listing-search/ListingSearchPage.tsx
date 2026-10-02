import { useCallback, useMemo, useRef } from 'react'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { type FieldErrors } from '@/api'
import { ResultsToolbar, SearchFilters, SearchResults } from '@/features/listing-search/components'
import { useListingSearch, useSearchForm, useSearchUrlState, type SearchResult } from '@/features/listing-search/hooks'
import {
  activeFilters,
  clearFilters,
  hasErrors,
  toSearchParams,
  validateForm,
  withoutFilter,
  type FilterKey,
} from '@/features/listing-search/model'

/** Which messages to show next to the inputs: the user's latest submit first, then a bad URL, then the server. */
function pickFieldErrors(draftErrors: FieldErrors, urlErrors: FieldErrors, result: SearchResult): FieldErrors {
  if (hasErrors(draftErrors)) return draftErrors
  if (hasErrors(urlErrors)) return urlErrors
  return result.status === 'error' && result.error.kind === 'validation' ? result.error.fieldErrors : {}
}

/** The latest known total, kept while the next page loads so the heading does not flicker. */
function knownTotal(result: SearchResult): number | undefined {
  if (result.status === 'success') return result.data.totalCount
  if (result.status === 'loading') return result.previous?.totalCount
  return undefined
}

/**
 * Container: owns state and wiring; the components it renders are presentational.
 * URL (applied search) -> validated params -> fetch -> components.
 */
export function ListingSearchPage() {
  const [applied, navigate] = useSearchUrlState()
  const form = useSearchForm(applied, navigate)
  const resultsRef = useRef<HTMLElement>(null)

  // A hand-edited URL can hold invalid values; those are explained instead of sent.
  const urlErrors = useMemo(() => validateForm(applied.form), [applied])
  const params = useMemo(() => (hasErrors(urlErrors) ? null : toSearchParams(applied)), [applied, urlErrors])
  const { result, retry } = useListingSearch(params)

  const fieldErrors = pickFieldErrors(form.errors, urlErrors, result)
  const filters = useMemo(() => activeFilters(applied.form), [applied.form])

  const goToPage = useCallback(
    (page: number) => {
      navigate({ form: applied.form, page })
      // Bring the new page into view; respect users who asked the OS for reduced motion.
      const smooth = !window.matchMedia('(prefers-reduced-motion: reduce)').matches
      resultsRef.current?.scrollIntoView({ behavior: smooth ? 'smooth' : 'auto', block: 'start' })
    },
    [applied.form, navigate],
  )
  const changePageSize = useCallback(
    (pageSize: string) => navigate({ form: { ...applied.form, pageSize }, page: 1 }),
    [applied.form, navigate],
  )
  const removeFilter = useCallback(
    (key: FilterKey) => navigate({ form: withoutFilter(applied.form, key), page: 1 }),
    [applied.form, navigate],
  )
  const clearAll = useCallback(() => navigate({ form: clearFilters(applied.form), page: 1 }), [applied.form, navigate])

  return (
    <Stack spacing={3}>
      <div>
        <Typography variant="h4" component="h1">
          Find a listing
        </Typography>
        <Typography color="text.secondary" sx={{ mt: 0.5 }}>
          Search every MLS feed at once. Add a target budget to rank homes by how well the price fits.
        </Typography>
      </div>

      <Paper variant="outlined" sx={{ p: { xs: 2, md: 3 } }}>
        <SearchFilters
          values={form.draft}
          errors={fieldErrors}
          onChange={form.setField}
          onSubmit={form.submit}
          onClear={clearAll}
        />
      </Paper>

      <Stack
        component="section"
        ref={resultsRef}
        aria-labelledby="results-heading"
        spacing={2}
        sx={{ scrollMarginTop: 80 }}
      >
        <ResultsToolbar
          totalCount={knownTotal(result)}
          rankedByBudget={applied.form.targetBudget.trim() !== ''}
          pageSize={applied.form.pageSize}
          onPageSizeChange={changePageSize}
          filters={filters}
          onRemoveFilter={removeFilter}
          onClearFilters={clearAll}
        />
        <SearchResults
          result={result}
          invalid={hasErrors(fieldErrors)}
          hasFilters={filters.length > 0}
          onRetry={retry}
          onPageChange={goToPage}
          onClearFilters={clearAll}
        />
      </Stack>
    </Stack>
  )
}
