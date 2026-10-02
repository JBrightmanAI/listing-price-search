const currency = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })
const integer = new Intl.NumberFormat('en-US')
// Calendar dates are formatted in UTC; in local time "2026-08-29" would show as Aug 28 across the Americas.
const calendarDate = new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeZone: 'UTC' })

export const formatPrice = (value: number) => currency.format(value)

export const formatNumber = (value: number) => integer.format(value)

export const formatScore = (value: number) => value.toFixed(2)

/** Formats an ISO calendar date ("2026-08-29"); returns the input unchanged if it cannot be parsed. */
export function formatDate(isoDate: string) {
  const parsed = new Date(`${isoDate}T00:00:00Z`)
  return Number.isNaN(parsed.getTime()) ? isoDate : calendarDate.format(parsed)
}
