import InfoOutlined from '@mui/icons-material/InfoOutlined'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'

const EXPLANATION =
  'Relevance from 0 to 100. With a target budget: 70% closeness to the budget and 30% recency. ' +
  'Without one: recency only. Recency halves every 30 days on the market.'

/** Explains the score where it is shown. A button, so keyboard and touch users can open it too. */
export function ScoreHelp() {
  return (
    <Tooltip title={EXPLANATION} describeChild>
      <IconButton size="small" aria-label="How the relevance score works" sx={{ ml: 0.25, p: 0.25 }}>
        <InfoOutlined sx={{ fontSize: 16 }} />
      </IconButton>
    </Tooltip>
  )
}
