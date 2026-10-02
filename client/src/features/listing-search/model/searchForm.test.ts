import { describe, expect, it } from 'vitest'
import {
  emptyForm,
  fromUrlQuery,
  MAX_TEXT_LENGTH,
  toSearchParams,
  toUrlQuery,
  validateForm,
  type FormValues,
} from './searchForm'

const form = (overrides: Partial<FormValues>): FormValues => ({ ...emptyForm, ...overrides })

describe('validateForm', () => {
  it('accepts an empty form', () => {
    expect(validateForm(emptyForm)).toEqual({})
  })

  it('rejects min price above max price on the min price field', () => {
    expect(validateForm(form({ minPrice: '500000', maxPrice: '400000' }))['minPrice']).toEqual([
      'Min price cannot be greater than max price.',
    ])
  })

  it('accepts equal min and max price', () => {
    expect(validateForm(form({ minPrice: '500000', maxPrice: '500000' }))).toEqual({})
  })

  it('accepts thousands separators', () => {
    expect(validateForm(form({ minPrice: '450,000' }))).toEqual({})
  })

  it.each(['abc', '1e5', '12abc', '--1'])('rejects non-numeric price %s', (value) => {
    expect(validateForm(form({ maxPrice: value }))['maxPrice']).toBeDefined()
  })

  it('rejects negative values', () => {
    const errors = validateForm(form({ minPrice: '-1', minBedrooms: '-1' }))

    expect(errors['minPrice']).toBeDefined()
    expect(errors['minBedrooms']).toBeDefined()
  })

  it('rejects fractional bedrooms', () => {
    expect(validateForm(form({ minBedrooms: '2.5' }))['minBedrooms']).toBeDefined()
  })

  it('rejects a zero target budget', () => {
    expect(validateForm(form({ targetBudget: '0' }))['targetBudget']).toEqual(['Must be greater than zero.'])
  })

  it('rejects a keyword with no letters or digits, and overly long text', () => {
    const errors = validateForm(form({ keyword: '!!!', city: 'x'.repeat(MAX_TEXT_LENGTH + 1) }))

    expect(errors['keyword']).toBeDefined()
    expect(errors['city']).toBeDefined()
  })
})

describe('toSearchParams', () => {
  it('omits blank filters and parses numbers', () => {
    expect(toSearchParams({ form: form({ minPrice: '450,000', city: '  Reston ' }), page: 2 })).toEqual({
      minPrice: 450000,
      maxPrice: undefined,
      minBedrooms: undefined,
      city: 'Reston',
      keyword: undefined,
      targetBudget: undefined,
      page: 2,
      pageSize: 10,
    })
  })
})

describe('URL round trip', () => {
  it('writes only non-default values', () => {
    expect(toUrlQuery({ form: form({ city: 'Reston' }), page: 1 })).toBe('city=Reston')
    expect(toUrlQuery({ form: emptyForm, page: 1 })).toBe('')
  })

  it('reads back what it writes', () => {
    const state = { form: form({ city: 'Reston', minPrice: '400000', pageSize: '20' }), page: 3 }

    expect(fromUrlQuery(`?${toUrlQuery(state)}`)).toEqual(state)
  })

  it.each([
    ['?page=0', 1],
    ['?page=-4', 1],
    ['?page=2.5', 1],
    ['?page=abc', 1],
    ['?page=7', 7],
  ])('falls back to page 1 for an unusable page (%s)', (search, expected) => {
    expect(fromUrlQuery(search).page).toBe(expected)
  })

  it('falls back to the default page size for an unsupported value', () => {
    expect(fromUrlQuery('?pageSize=1000').form.pageSize).toBe('10')
  })
})
