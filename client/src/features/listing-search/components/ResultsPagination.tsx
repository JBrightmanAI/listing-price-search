import Pagination from '@mui/material/Pagination'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

interface Props {
  page: number
  totalPages: number
  totalCount: number
  onPageChange: (page: number) => void
}

export function ResultsPagination({ page, totalPages, totalCount, onPageChange }: Props) {
  return (
    <Stack
      direction={{ xs: 'column-reverse', sm: 'row' }}
      spacing={1.5}
      sx={{ alignItems: 'center', justifyContent: 'space-between', mt: 3 }}
    >
      {/* Announced on every page change so screen-reader users know the results updated. */}
      <Typography role="status" variant="body2" color="text.secondary">
        Page {page} of {totalPages} · {totalCount} {totalCount === 1 ? 'result' : 'results'}
      </Typography>
      {totalPages > 1 && (
        <Pagination
          count={totalPages}
          page={page}
          onChange={(_, next) => onPageChange(next)}
          showFirstButton
          showLastButton
          color="primary"
          shape="rounded"
          siblingCount={1}
        />
      )}
    </Stack>
  )
}
