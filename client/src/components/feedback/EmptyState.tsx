import type { ReactNode } from 'react'
import Box from '@mui/material/Box'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'

interface Props {
  title: string
  description?: string | undefined
  icon?: ReactNode
  action?: ReactNode
}

/** A valid outcome with nothing to show. Announced politely, not as an error. */
export function EmptyState({ title, description, icon, action }: Props) {
  return (
    <Paper role="status" variant="outlined" sx={{ px: 3, py: 6, textAlign: 'center' }}>
      {icon && (
        <Box
          aria-hidden
          sx={{
            display: 'inline-flex',
            p: 1.5,
            mb: 2,
            borderRadius: '50%',
            color: 'primary.main',
            bgcolor: 'action.hover',
            '& svg': { fontSize: 32 },
          }}
        >
          {icon}
        </Box>
      )}
      <Typography variant="h6" component="p">
        {title}
      </Typography>
      {description && (
        <Typography color="text.secondary" sx={{ mt: 1, maxWidth: 420, mx: 'auto' }}>
          {description}
        </Typography>
      )}
      {action && <Box sx={{ mt: 3 }}>{action}</Box>}
    </Paper>
  )
}
