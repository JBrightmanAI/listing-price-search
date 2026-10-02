import TextField from '@mui/material/TextField'
import { PAGE_SIZE_OPTIONS } from '@/features/listing-search/model'

interface Props {
  value: string
  onChange: (value: string) => void
}

/** A native select: accessible and keyboard friendly out of the box, and uses the OS picker on mobile. */
export function PageSizeSelect({ value, onChange }: Props) {
  return (
    <TextField
      select
      id="pageSize"
      name="pageSize"
      label="Per page"
      size="small"
      value={value}
      onChange={(event) => onChange(event.target.value)}
      slotProps={{ select: { native: true } }}
      sx={{ minWidth: 104 }}
    >
      {PAGE_SIZE_OPTIONS.map((size) => (
        <option key={size} value={size}>
          {size}
        </option>
      ))}
    </TextField>
  )
}
