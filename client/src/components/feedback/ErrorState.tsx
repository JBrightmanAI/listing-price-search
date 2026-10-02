import Alert from '@mui/material/Alert'
import AlertTitle from '@mui/material/AlertTitle'
import Button from '@mui/material/Button'

interface Props {
  title?: string
  message: string
  onRetry?: () => void
}

/** MUI's Alert renders role="alert", so the message is announced as soon as it appears. */
export function ErrorState({ title, message, onRetry }: Props) {
  return (
    <Alert
      severity="error"
      action={
        onRetry && (
          <Button color="inherit" size="small" onClick={onRetry}>
            Retry
          </Button>
        )
      }
    >
      {title && <AlertTitle>{title}</AlertTitle>}
      {message}
    </Alert>
  )
}
