import { screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '@/api'
import type { SearchResult } from '@/features/listing-search/hooks'
import { makeListing, makeResponse, renderWithProviders } from '@/test'
import { SearchResults } from './SearchResults'

function renderResults(result: SearchResult, { invalid = false, hasFilters = false } = {}) {
  const onRetry = vi.fn()
  const onPageChange = vi.fn()
  const onClearFilters = vi.fn()
  const view = renderWithProviders(
    <SearchResults
      result={result}
      invalid={invalid}
      hasFilters={hasFilters}
      onRetry={onRetry}
      onPageChange={onPageChange}
      onClearFilters={onClearFilters}
    />,
  )
  return { ...view, onRetry, onPageChange, onClearFilters }
}

describe('SearchResults', () => {
  it('shows a loading indicator on first load', () => {
    renderResults({ status: 'loading' })

    expect(screen.getByText('Loading listings…')).toBeInTheDocument()
  })

  it('keeps the previous results visible while the next page loads', () => {
    renderResults({ status: 'loading', previous: makeResponse() })

    expect(screen.getByText('123 Main St, Apt 4B')).toBeInTheDocument()
    expect(screen.getByLabelText('Updating results')).toBeInTheDocument()
  })

  it('renders address, price, bedrooms and score for each listing', () => {
    renderResults({ status: 'success', data: makeResponse() })

    const row = screen.getByRole('row', { name: /123 Main St/ })
    expect(row).toHaveTextContent('$450,000')
    expect(row).toHaveTextContent('2')
    expect(row).toHaveTextContent('95.52')
  })

  it('renders listings that share an id across sources', () => {
    const items = [
      makeListing({ source: 'MLS_A', id: '1' }),
      makeListing({ source: 'MLS_B', id: '1', address: '1 Other Rd' }),
    ]

    renderResults({ status: 'success', data: makeResponse({ items }) })

    expect(screen.getAllByRole('row')).toHaveLength(3) // header + 2
  })

  it('formats the listed date without a time-zone shift', () => {
    renderResults({ status: 'success', data: makeResponse({ items: [makeListing({ listedDate: '2026-08-29' })] }) })

    expect(screen.getByText('Aug 29, 2026')).toBeInTheDocument()
  })

  it('explains an empty result', () => {
    renderResults({ status: 'success', data: makeResponse({ items: [], totalCount: 0, totalPages: 0 }) })

    expect(screen.getByText('No listings match your search')).toBeInTheDocument()
    // Nothing to clear, so no button that would do nothing.
    expect(screen.queryByRole('button', { name: 'Clear filters' })).not.toBeInTheDocument()
  })

  it('offers to clear filters when filters caused the empty result', async () => {
    const { user, onClearFilters } = renderResults(
      { status: 'success', data: makeResponse({ items: [], totalCount: 0, totalPages: 0 }) },
      { hasFilters: true },
    )

    await user.click(screen.getByRole('button', { name: 'Clear filters' }))

    expect(onClearFilters).toHaveBeenCalledOnce()
  })

  it('explains how the score works', async () => {
    const { user } = renderResults({ status: 'success', data: makeResponse() })

    await user.hover(screen.getByRole('button', { name: 'How the relevance score works' }))

    expect(await screen.findByRole('tooltip')).toHaveTextContent(/70% closeness to the budget/)
  })

  it('hides pagination controls when everything fits on one page', () => {
    renderResults({ status: 'success', data: makeResponse() })

    expect(screen.getByText('Page 1 of 1 · 1 result')).toBeInTheDocument()
    expect(screen.queryByRole('navigation')).not.toBeInTheDocument()
  })

  it('offers the last page when the page is past the end', async () => {
    const { user, onPageChange } = renderResults({
      status: 'success',
      data: makeResponse({ items: [], page: 9, totalCount: 12, totalPages: 2 }),
    })

    await user.click(screen.getByRole('button', { name: 'Go to last page' }))

    expect(onPageChange).toHaveBeenCalledWith(2)
  })

  it('shows a retryable error', async () => {
    const { user, onRetry } = renderResults({
      status: 'error',
      error: new ApiError('network', 'Could not reach the server.'),
    })

    expect(screen.getByRole('alert')).toHaveTextContent('Could not reach the server.')
    await user.click(screen.getByRole('button', { name: 'Retry' }))
    expect(onRetry).toHaveBeenCalledOnce()
  })

  it('tells the user how long to wait when rate limited', () => {
    renderResults({
      status: 'error',
      error: new ApiError('rateLimited', 'Too many requests.', { retryAfterSeconds: 20 }),
    })

    expect(screen.getByRole('alert')).toHaveTextContent('Try again in 20 seconds.')
  })

  it('points to the highlighted fields for validation errors', () => {
    renderResults({ status: 'idle' }, { invalid: true })

    expect(screen.getByText(/Check the highlighted fields/)).toBeInTheDocument()
  })
})
