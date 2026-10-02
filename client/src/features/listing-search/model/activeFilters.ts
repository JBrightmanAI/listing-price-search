import { formatPrice } from '@/lib'
import { emptyForm, type FormField, type FormValues } from './searchForm'

/** Every form field except page size, which is a display preference rather than a filter. */
export type FilterKey = Exclude<FormField, 'pageSize'>

export interface ActiveFilter {
  key: FilterKey
  label: string
  /** Display value, e.g. "$450,000" or "3+". */
  value: string
}

// Display order for the chips.
const LABELS: Readonly<Record<FilterKey, string>> = {
  keyword: 'Keyword',
  city: 'City',
  minPrice: 'Min price',
  maxPrice: 'Max price',
  minBedrooms: 'Beds',
  targetBudget: 'Budget',
}

const MONEY_FIELDS: ReadonlySet<FilterKey> = new Set(['minPrice', 'maxPrice', 'targetBudget'])

function displayValue(key: FilterKey, raw: string): string {
  const numeric = Number(raw.replaceAll(',', ''))
  if (MONEY_FIELDS.has(key) && Number.isFinite(numeric)) return formatPrice(numeric)
  if (key === 'minBedrooms' && Number.isFinite(numeric)) return `${numeric}+`
  return raw
}

/** The filters currently applied, ready to show as removable chips. */
export function activeFilters(form: FormValues): ActiveFilter[] {
  return (Object.keys(LABELS) as FilterKey[]).flatMap((key) => {
    const raw = form[key].trim()
    return raw === '' ? [] : [{ key, label: LABELS[key], value: displayValue(key, raw) }]
  })
}

export function withoutFilter(form: FormValues, key: FilterKey): FormValues {
  return { ...form, [key]: '' }
}

/** Clears every filter but keeps the page size: that is a viewing preference, not part of the search. */
export function clearFilters(form: FormValues): FormValues {
  return { ...emptyForm, pageSize: form.pageSize }
}
