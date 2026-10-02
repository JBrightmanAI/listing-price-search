import Paper from '@mui/material/Paper'
import Skeleton from '@mui/material/Skeleton'
import Stack from '@mui/material/Stack'
import { VisuallyHidden } from '@/components'

/**
 * Placeholder shaped like the results, shown on first load. It reads as faster than a spinner
 * and stops the layout jumping when the real rows arrive.
 */
export function ResultsSkeleton({ rows = 5 }: { rows?: number }) {
  return (
    <Paper role="status" variant="outlined" sx={{ p: 2 }}>
      <VisuallyHidden>Loading listings…</VisuallyHidden>
      <Stack spacing={2} aria-hidden>
        {Array.from({ length: rows }, (_, index) => (
          <Stack key={index} direction="row" spacing={2} sx={{ alignItems: 'center' }}>
            <Stack sx={{ flex: 3 }}>
              <Skeleton width="70%" />
              <Skeleton width="40%" height={16} />
            </Stack>
            <Skeleton sx={{ flex: 1 }} />
            <Skeleton sx={{ flex: 1, display: { xs: 'none', md: 'block' } }} />
            <Skeleton variant="rounded" width={64} height={24} />
          </Stack>
        ))}
      </Stack>
    </Paper>
  )
}
