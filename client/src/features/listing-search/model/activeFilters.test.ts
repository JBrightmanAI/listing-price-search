import { describe, expect, it } from 'vitest'
import { activeFilters, clearFilters, withoutFilter } from './activeFilters'
import { emptyForm, type FormValues } from './searchForm'

const form = (overrides: Partial<FormValues>): FormValues => ({ ...emptyForm, ...overrides })

describe('activeFilters', () => {
  it('is empty when nothing is set', () => {
    expect(activeFilters(emptyForm)).toEqual([])
  })

  it('formats values for display, in a stable order', () => {
    const filters = activeFilters(
      form({ city: ' Reston ', minPrice: '450,000', minBedrooms: '3', keyword: 'garage', targetBudget: '500000' }),
    )

    expect(filters).toEqual([
      { key: 'keyword', label: 'Keyword', value: 'garage' },
      { key: 'city', label: 'City', value: 'Reston' },
      { key: 'minPrice', label: 'Min price', value: '$450,000' },
      { key: 'minBedrooms', label: 'Beds', value: '3+' },
      { key: 'targetBudget', label: 'Budget', value: '$500,000' },
    ])
  })

  it('does not treat page size as a filter', () => {
    expect(activeFilters(form({ pageSize: '50' }))).toEqual([])
  })
})

describe('withoutFilter / clearFilters', () => {
  it('removes one filter and keeps the rest', () => {
    expect(withoutFilter(form({ city: 'Reston', keyword: 'pool' }), 'city')).toEqual(form({ keyword: 'pool' }))
  })

  it('clears every filter but keeps the page size', () => {
    expect(clearFilters(form({ city: 'Reston', minPrice: '1', pageSize: '20' }))).toEqual(form({ pageSize: '20' }))
  })
})
