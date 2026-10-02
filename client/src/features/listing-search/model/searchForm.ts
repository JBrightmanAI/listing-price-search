import type { FieldErrors, SearchParams } from '@/api'

/** Raw input values. Kept as strings so invalid input is visible and can be explained, not silently dropped. */
export interface FormValues {
  minPrice: string
  maxPrice: string
  minBedrooms: string
  city: string
  keyword: string
  targetBudget: string
  pageSize: string
}

export type FormField = keyof FormValues

/** The applied search: what the URL holds and what the results are for. */
export interface SearchState {
  form: FormValues
  page: number
}

export const PAGE_SIZE_OPTIONS = ['5', '10', '20', '50'] as const
export const DEFAULT_PAGE_SIZE = '10'
export const MAX_TEXT_LENGTH = 100

export const emptyForm: Readonly<FormValues> = {
  minPrice: '',
  maxPrice: '',
  minBedrooms: '',
  city: '',
  keyword: '',
  targetBudget: '',
  pageSize: DEFAULT_PAGE_SIZE,
}

export const initialSearch: SearchState = { form: emptyForm, page: 1 }

const FORM_FIELDS = Object.keys(emptyForm) as FormField[]

/** undefined for blank input, NaN for input that is not a plain number. Thousands separators are allowed. */
function parseNumber(text: string): number | undefined {
  const cleaned = text.trim().replaceAll(',', '')
  if (cleaned === '') return undefined
  return /^-?\d+(\.\d+)?$/.test(cleaned) ? Number(cleaned) : Number.NaN
}

/**
 * Mirrors the server rules for fast feedback. The server stays the source of truth and its
 * errors are displayed the same way, so the two never have to agree perfectly.
 */
export function validateForm(values: FormValues): FieldErrors {
  const errors: FieldErrors = {}
  const add = (field: FormField, message: string) => {
    errors[field] = [...(errors[field] ?? []), message]
  }

  const money = (field: 'minPrice' | 'maxPrice' | 'targetBudget') => {
    const value = parseNumber(values[field])
    if (Number.isNaN(value)) add(field, 'Enter a number, e.g. 450000.')
    else if (value !== undefined && value < 0) add(field, 'Cannot be negative.')
    return value
  }

  const minPrice = money('minPrice')
  const maxPrice = money('maxPrice')
  const targetBudget = money('targetBudget')

  if (minPrice !== undefined && maxPrice !== undefined && minPrice >= 0 && maxPrice >= 0 && minPrice > maxPrice) {
    add('minPrice', 'Min price cannot be greater than max price.')
  }

  if (targetBudget === 0) add('targetBudget', 'Must be greater than zero.')

  const minBedrooms = parseNumber(values.minBedrooms)
  if (minBedrooms !== undefined && (!Number.isInteger(minBedrooms) || minBedrooms < 0)) {
    add('minBedrooms', 'Enter a whole number, 0 or more.')
  }

  if (values.city.trim().length > MAX_TEXT_LENGTH) add('city', `Keep it under ${MAX_TEXT_LENGTH} characters.`)

  const keyword = values.keyword.trim()
  if (keyword.length > MAX_TEXT_LENGTH) add('keyword', `Keep it under ${MAX_TEXT_LENGTH} characters.`)
  else if (keyword !== '' && !/[\p{L}\p{N}]/u.test(keyword)) add('keyword', 'Include at least one letter or digit.')

  return errors
}

export const hasErrors = (errors: FieldErrors) => Object.keys(errors).length > 0

const optionalText = (value: string) => value.trim() || undefined

/** Converts a validated search into API parameters. Only call with values that passed validateForm. */
export function toSearchParams({ form, page }: SearchState): SearchParams {
  return {
    minPrice: parseNumber(form.minPrice),
    maxPrice: parseNumber(form.maxPrice),
    minBedrooms: parseNumber(form.minBedrooms),
    city: optionalText(form.city),
    keyword: optionalText(form.keyword),
    targetBudget: parseNumber(form.targetBudget),
    page,
    pageSize: Number(form.pageSize),
  }
}

/** Shareable URL query holding only what differs from the defaults. */
export function toUrlQuery({ form, page }: SearchState): string {
  const query = new URLSearchParams()
  for (const field of FORM_FIELDS) {
    const value = form[field].trim()
    if (value !== '' && value !== emptyForm[field]) query.set(field, value)
  }
  if (page > 1) query.set('page', String(page))
  return query.toString()
}

/** Reads a possibly hand-edited URL. Unusable values fall back to defaults rather than breaking the page. */
export function fromUrlQuery(search: string): SearchState {
  const query = new URLSearchParams(search)
  const form: FormValues = { ...emptyForm }

  for (const field of FORM_FIELDS) {
    const value = query.get(field)
    if (value !== null) form[field] = value
  }

  if (!(PAGE_SIZE_OPTIONS as readonly string[]).includes(form.pageSize)) form.pageSize = DEFAULT_PAGE_SIZE

  const page = Number(query.get('page'))
  return { form, page: Number.isInteger(page) && page >= 1 ? page : 1 }
}
