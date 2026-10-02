import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import Stack from '@mui/material/Stack'
import { type ActiveFilter, type FilterKey } from '@/features/listing-search/model'

interface Props {
  filters: ActiveFilter[]
  onRemove: (key: FilterKey) => void
  onClearAll: () => void
}

/**
 * The applied filters at a glance, each removable on its own. A chip is a button for assistive tech;
 * Delete or Backspace removes it from the keyboard, and the accessible name says what it does.
 */
export function ActiveFilters({ filters, onRemove, onClearAll }: Props) {
  if (filters.length === 0) return null

  return (
    <Stack
      component="ul"
      aria-label="Active filters"
      direction="row"
      sx={{ flexWrap: 'wrap', gap: 1, alignItems: 'center', listStyle: 'none', m: 0, p: 0 }}
    >
      {filters.map(({ key, label, value }) => (
        <li key={key}>
          <Chip
            label={`${label}: ${value}`}
            aria-label={`Remove filter ${label}: ${value}`}
            onDelete={() => onRemove(key)}
            size="small"
            variant="outlined"
            color="primary"
          />
        </li>
      ))}
      {filters.length > 1 && (
        <li>
          <Button size="small" onClick={onClearAll}>
            Clear all
          </Button>
        </li>
      )}
    </Stack>
  )
}
