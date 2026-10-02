import { type SubmitEvent } from 'react'
import Search from '@mui/icons-material/Search'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import { type FieldErrors } from '@/api'
import { type FormField, type FormValues } from '@/features/listing-search/model'
import { FilterField } from './FilterField'
import { FILTER_FIELDS } from './filterFields'

interface Props {
  values: FormValues
  errors: FieldErrors
  onChange: (field: FormField, value: string) => void
  onSubmit: () => void
  onClear: () => void
}

export function SearchFilters({ values, errors, onChange, onSubmit, onClear }: Props) {
  const handleSubmit = (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault()
    onSubmit()
  }

  return (
    // noValidate: validation is ours, so messages are consistent with the server's.
    <Box component="form" onSubmit={handleSubmit} noValidate aria-label="Search listings">
      <Box sx={{ display: 'grid', gap: 2, gridTemplateColumns: 'repeat(12, 1fr)' }}>
        {FILTER_FIELDS.map(({ columns, ...config }) => (
          <Box key={config.field} sx={{ gridColumn: { xs: 'span 12', sm: 'span 6', md: `span ${columns}` } }}>
            <FilterField {...config} value={values[config.field]} errors={errors[config.field]} onChange={onChange} />
          </Box>
        ))}

        <Box
          sx={{
            gridColumn: { xs: 'span 12', sm: 'span 6', md: 'span 3' },
            display: 'flex',
            gap: 1,
            alignItems: 'flex-start',
            justifyContent: { sm: 'flex-end' },
          }}
        >
          <Button type="button" variant="text" onClick={onClear}>
            Clear
          </Button>
          <Button type="submit" variant="contained" startIcon={<Search />} sx={{ flexGrow: { xs: 1, sm: 0 } }}>
            Search
          </Button>
        </Box>
      </Box>
    </Box>
  )
}
