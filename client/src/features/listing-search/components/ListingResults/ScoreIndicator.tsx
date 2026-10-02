import LinearProgress from '@mui/material/LinearProgress'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { formatScore } from '@/lib'

function colorFor(score: number) {
  if (score >= 75) return 'success'
  if (score >= 50) return 'primary'
  return 'warning'
}

/** The 0–100 relevance score as a number plus a bar for quick visual comparison down a column. */
export function ScoreIndicator({ score }: { score: number }) {
  return (
    <Stack direction="row" spacing={1} sx={{ alignItems: 'center', justifyContent: 'flex-end' }}>
      {/* Decorative: the number next to it is the accessible value. */}
      <LinearProgress
        aria-hidden
        variant="determinate"
        value={Math.min(100, Math.max(0, score))}
        color={colorFor(score)}
        sx={{ width: 56, height: 6, borderRadius: 3 }}
      />
      <Typography
        variant="body2"
        component="span"
        sx={{ fontWeight: 600, fontVariantNumeric: 'tabular-nums', minWidth: 44, textAlign: 'right' }}
      >
        {formatScore(score)}
      </Typography>
    </Stack>
  )
}
