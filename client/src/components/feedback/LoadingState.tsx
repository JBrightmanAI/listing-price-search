import CircularProgress from '@mui/material/CircularProgress'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

export function LoadingState({ label = 'Loading…' }: { label?: string }) {
  return (
    <Stack role="status" direction="row" spacing={2} sx={{ alignItems: 'center', justifyContent: 'center', py: 6 }}>
      <CircularProgress size={24} aria-hidden />
      <Typography color="text.secondary">{label}</Typography>
    </Stack>
  )
}
