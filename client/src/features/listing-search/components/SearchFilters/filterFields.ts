import { type FilterKey } from '@/features/listing-search/model'

export type FieldAdornment = 'currency' | 'search' | 'place' | 'bed'

export interface FilterFieldConfig {
  field: FilterKey
  label: string
  inputMode: 'text' | 'decimal' | 'numeric'
  placeholder?: string
  /** Shown under the field when it has no error. */
  hint?: string
  adornment: FieldAdornment
  /** Columns out of 12 on desktop; the form is a 12-column grid. */
  columns: 3 | 6
}

/**
 * The filter inputs, in display order: two rows on desktop, one column on phones.
 * Adding a filter is one entry here plus the model change.
 * Numeric inputs are text with a numeric keyboard rather than type="number": the browser would
 * turn "abc" into an empty value and the search would silently ignore it instead of explaining.
 */
export const FILTER_FIELDS: readonly FilterFieldConfig[] = [
  {
    field: 'keyword',
    label: 'Keyword',
    placeholder: 'e.g. garage, pool, metro',
    inputMode: 'text',
    adornment: 'search',
    columns: 6,
  },
  { field: 'city', label: 'City', placeholder: 'e.g. Reston', inputMode: 'text', adornment: 'place', columns: 3 },
  {
    field: 'minBedrooms',
    label: 'Min bedrooms',
    placeholder: 'Any',
    inputMode: 'numeric',
    adornment: 'bed',
    columns: 3,
  },
  {
    field: 'minPrice',
    label: 'Min price',
    placeholder: 'No min',
    inputMode: 'decimal',
    adornment: 'currency',
    columns: 3,
  },
  {
    field: 'maxPrice',
    label: 'Max price',
    placeholder: 'No max',
    inputMode: 'decimal',
    adornment: 'currency',
    columns: 3,
  },
  {
    field: 'targetBudget',
    label: 'Target budget',
    placeholder: 'Optional',
    hint: 'Ranks prices near it higher',
    inputMode: 'decimal',
    adornment: 'currency',
    columns: 3,
  },
]
