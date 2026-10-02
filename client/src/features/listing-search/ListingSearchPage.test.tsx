import { screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiError, searchListings } from '@/api'
import { makeListing, makeResponse, renderWithProviders } from '@/test'
import { ListingSearchPage } from './ListingSearchPage'

// Mock the endpoint module itself; the @/api barrel re-exports from it, so every consumer sees the mock.
vi.mock('@/api/listingsApi', () => ({ searchListings: vi.fn() }))

const search = vi.mocked(searchListings)

const lastParams = () => search.mock.lastCall?.[0]

describe('ListingSearchPage', () => {
  beforeEach(() => {
    search.mockReset()
    search.mockResolvedValue(makeResponse())
  })

  it('searches on load and shows ranked results', async () => {
    renderWithProviders(<ListingSearchPage />)

    expect(await screen.findByText('123 Main St, Apt 4B')).toBeInTheDocument()
    expect(lastParams()).toMatchObject({ page: 1, pageSize: 10 })
  })

  it('sends the entered filters and starts from page one', async () => {
    window.history.replaceState(null, '', '/?page=3')
    const { user } = renderWithProviders(<ListingSearchPage />)

    await user.type(screen.getByLabelText('City'), 'Reston')
    await user.type(screen.getByLabelText('Target budget'), '450,000')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    await waitFor(() => expect(lastParams()).toMatchObject({ city: 'Reston', targetBudget: 450000, page: 1 }))
    expect(window.location.search).toBe('?city=Reston&targetBudget=450%2C000')
  })

  it('explains invalid filters without calling the API', async () => {
    const { user } = renderWithProviders(<ListingSearchPage />)
    await screen.findByText('123 Main St, Apt 4B')

    await user.type(screen.getByLabelText('Min price'), '500000')
    await user.type(screen.getByLabelText('Max price'), '100')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    expect(screen.getByText('Min price cannot be greater than max price.')).toBeInTheDocument()
    expect(screen.getByLabelText('Min price')).toHaveAttribute('aria-invalid', 'true')
    expect(search).toHaveBeenCalledOnce()
  })

  it('clears a field error as soon as the user edits that field', async () => {
    const { user } = renderWithProviders(<ListingSearchPage />)

    await user.type(screen.getByLabelText('Min bedrooms'), 'two')
    await user.click(screen.getByRole('button', { name: 'Search' }))
    expect(screen.getByText('Enter a whole number, 0 or more.')).toBeInTheDocument()

    await user.type(screen.getByLabelText('Min bedrooms'), 'x')
    expect(screen.queryByText('Enter a whole number, 0 or more.')).not.toBeInTheDocument()
  })

  it('shows server validation errors next to the field', async () => {
    search.mockResolvedValueOnce(makeResponse())
    search.mockRejectedValueOnce(
      new ApiError('validation', 'Invalid', {
        status: 400,
        fieldErrors: { city: ['city cannot be longer than 100 characters.'] },
      }),
    )
    const { user } = renderWithProviders(<ListingSearchPage />)
    await screen.findByText('123 Main St, Apt 4B')

    await user.type(screen.getByLabelText('City'), 'Reston')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    expect(await screen.findByText('city cannot be longer than 100 characters.')).toBeInTheDocument()
    expect(screen.getByLabelText('City')).toHaveAttribute('aria-invalid', 'true')
  })

  it('recovers from a failed request with retry', async () => {
    search.mockRejectedValueOnce(new ApiError('server', 'Something went wrong on our side.'))
    const { user } = renderWithProviders(<ListingSearchPage />)

    expect(await screen.findByRole('alert')).toHaveTextContent('Something went wrong on our side.')
    await user.click(screen.getByRole('button', { name: 'Retry' }))

    expect(await screen.findByText('123 Main St, Apt 4B')).toBeInTheDocument()
  })

  it('moves between pages and keeps the page in the URL', async () => {
    search.mockResolvedValue(makeResponse({ totalCount: 25, totalPages: 3 }))
    const { user } = renderWithProviders(<ListingSearchPage />)
    await screen.findByText('Page 1 of 3 · 25 results')

    await user.click(screen.getByRole('button', { name: 'Go to next page' }))

    await waitFor(() => expect(lastParams()).toMatchObject({ page: 2 }))
    expect(window.location.search).toBe('?page=2')
  })

  it('restores the search from the URL', async () => {
    window.history.replaceState(null, '', '/?city=Vienna&page=2&pageSize=20')
    renderWithProviders(<ListingSearchPage />)

    await waitFor(() => expect(lastParams()).toMatchObject({ city: 'Vienna', page: 2, pageSize: 20 }))
    expect(screen.getByLabelText('City')).toHaveValue('Vienna')
  })

  it('does not send a hand-edited URL with invalid values', async () => {
    window.history.replaceState(null, '', '/?minPrice=abc')
    renderWithProviders(<ListingSearchPage />)

    expect(await screen.findByText('Enter a number, e.g. 450000.')).toBeInTheDocument()
    expect(search).not.toHaveBeenCalled()
  })

  it('aborts the in-flight request when a newer search starts', async () => {
    let firstSignal: AbortSignal | undefined
    search.mockImplementationOnce((_, signal) => {
      firstSignal = signal
      return new Promise(() => {
        // Never settles, like a server that is still working.
      })
    })
    search.mockResolvedValue(makeResponse({ items: [makeListing({ address: '22 Birch Ln' })] }))
    const { user } = renderWithProviders(<ListingSearchPage />)

    await user.type(screen.getByLabelText('City'), 'Reston')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    expect(await screen.findByText('22 Birch Ln')).toBeInTheDocument()
    expect(firstSignal?.aborted).toBe(true)
  })

  it('clear empties the filters but keeps the page size', async () => {
    window.history.replaceState(null, '', '/?city=Vienna&pageSize=20')
    const { user } = renderWithProviders(<ListingSearchPage />)

    await user.click(screen.getByRole('button', { name: 'Clear' }))

    expect(screen.getByLabelText('City')).toHaveValue('')
    expect(window.location.search).toBe('?pageSize=20')
  })

  it('shows applied filters as chips that can be removed one at a time', async () => {
    window.history.replaceState(null, '', '/?city=Vienna&minPrice=400000')
    const { user } = renderWithProviders(<ListingSearchPage />)

    const chip = await screen.findByRole('button', { name: 'Remove filter City: Vienna' })
    expect(screen.getByRole('button', { name: 'Remove filter Min price: $400,000' })).toBeInTheDocument()

    chip.focus()
    await user.keyboard('{Delete}')

    await waitFor(() => expect(lastParams()).toMatchObject({ city: undefined, minPrice: 400000, page: 1 }))
    expect(screen.queryByRole('button', { name: 'Remove filter City: Vienna' })).not.toBeInTheDocument()
  })

  it('changing the page size goes back to page one', async () => {
    window.history.replaceState(null, '', '/?page=3')
    const { user } = renderWithProviders(<ListingSearchPage />)

    await user.selectOptions(screen.getByLabelText('Per page'), '20')

    await waitFor(() => expect(lastParams()).toMatchObject({ pageSize: 20, page: 1 }))
    expect(window.location.search).toBe('?pageSize=20')
  })

  it('shows the result count and how results are ranked', async () => {
    search.mockResolvedValue(makeResponse({ totalCount: 25, totalPages: 3 }))
    window.history.replaceState(null, '', '/?targetBudget=500000')
    renderWithProviders(<ListingSearchPage />)

    expect(await screen.findByRole('heading', { name: '25 listings' })).toBeInTheDocument()
    expect(screen.getByText('Ranked by closeness to your budget, then recency')).toBeInTheDocument()
  })

  it('scrolls the results into view on page change', async () => {
    const scrollIntoView = vi.spyOn(Element.prototype, 'scrollIntoView')
    search.mockResolvedValue(makeResponse({ totalCount: 25, totalPages: 3 }))
    const { user } = renderWithProviders(<ListingSearchPage />)
    await screen.findByText('Page 1 of 3 · 25 results')

    await user.click(screen.getByRole('button', { name: 'Go to next page' }))

    expect(scrollIntoView).toHaveBeenCalledWith({ behavior: 'smooth', block: 'start' })
  })
})
