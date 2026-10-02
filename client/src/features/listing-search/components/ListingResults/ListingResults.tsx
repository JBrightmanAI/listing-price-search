import Box from '@mui/material/Box'
import LinearProgress from '@mui/material/LinearProgress'
import Stack from '@mui/material/Stack'
import { useTheme } from '@mui/material/styles'
import useMediaQuery from '@mui/material/useMediaQuery'
import { type Listing } from '@/api'
import { ListingCard } from './ListingCard'
import { ResultsTable } from './ResultsTable'

interface Props {
  listings: Listing[]
  /** True while a newer page is loading; the current results stay visible but dimmed. */
  busy?: boolean
}

/** Table on wide screens, cards on phones: a nine-column table does not fit a phone without sideways scrolling. */
export function ListingResults({ listings, busy = false }: Props) {
  const theme = useTheme()
  const compact = useMediaQuery(theme.breakpoints.down('md'))

  return (
    <Box aria-busy={busy}>
      {/* Fixed-height slot so the results do not shift when the bar appears. */}
      <Box sx={{ height: 4, mb: 0.5 }}>{busy && <LinearProgress aria-label="Updating results" />}</Box>

      <Box sx={{ opacity: busy ? 0.55 : 1, transition: 'opacity 150ms' }}>
        {compact ? (
          <Stack component="ul" spacing={1.5} sx={{ listStyle: 'none', m: 0, p: 0 }} aria-label="Listings">
            {listings.map((listing) => (
              <li key={`${listing.source}:${listing.id}`}>
                <ListingCard listing={listing} />
              </li>
            ))}
          </Stack>
        ) : (
          <ResultsTable listings={listings} />
        )}
      </Box>
    </Box>
  )
}
