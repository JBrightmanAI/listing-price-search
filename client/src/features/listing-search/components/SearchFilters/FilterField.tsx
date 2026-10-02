import { memo, type ReactNode } from 'react'
import BedOutlined from '@mui/icons-material/BedOutlined'
import PlaceOutlined from '@mui/icons-material/PlaceOutlined'
import Search from '@mui/icons-material/Search'
import InputAdornment from '@mui/material/InputAdornment'
import TextField from '@mui/material/TextField'
import { type FormField } from '@/features/listing-search/model'
import { type FieldAdornment, type FilterFieldConfig } from './filterFields'

interface Props extends Omit<FilterFieldConfig, 'columns'> {
  value: string
  errors?: string[] | undefined
  onChange: (field: FormField, value: string) => void
}

const ADORNMENTS: Record<FieldAdornment, ReactNode> = {
  currency: '$',
  search: <Search fontSize="small" />,
  place: <PlaceOutlined fontSize="small" />,
  bed: <BedOutlined fontSize="small" />,
}

/**
 * One labelled filter input. MUI wires the label, aria-invalid and the helper text's
 * aria-describedby from `id` and `error`, so the error (or hint) is read out with the field.
 * Memoised so typing in one field does not re-render the others.
 */
export const FilterField = memo(function FilterField({
  field,
  label,
  placeholder,
  hint,
  inputMode,
  adornment,
  value,
  errors,
  onChange,
}: Props) {
  return (
    <TextField
      id={field}
      name={field}
      label={label}
      placeholder={placeholder}
      size="small"
      fullWidth
      autoComplete="off"
      value={value}
      onChange={(event) => onChange(field, event.target.value)}
      error={Boolean(errors)}
      helperText={errors?.join(' ') ?? hint}
      slotProps={{
        htmlInput: { inputMode },
        input: {
          startAdornment: (
            <InputAdornment position="start" sx={{ color: 'text.secondary' }}>
              {ADORNMENTS[adornment]}
            </InputAdornment>
          ),
        },
        // Keep the label above the field so placeholders like "Any" stay visible.
        inputLabel: { shrink: true },
      }}
    />
  )
})
