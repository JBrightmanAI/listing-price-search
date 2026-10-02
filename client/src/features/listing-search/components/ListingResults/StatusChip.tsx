import Chip from '@mui/material/Chip'
import { type ListingStatus } from '@/api'

const COLOR: Record<ListingStatus, 'success' | 'warning' | 'default'> = {
  active: 'success',
  pending: 'warning',
  sold: 'default',
}

export function StatusChip({ status }: { status: ListingStatus }) {
  return (
    <Chip label={status} color={COLOR[status]} size="small" variant="outlined" sx={{ textTransform: 'capitalize' }} />
  )
}
