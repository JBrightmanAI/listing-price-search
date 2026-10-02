import Paper from '@mui/material/Paper'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableContainer from '@mui/material/TableContainer'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import { type Listing } from '@/api'
import { VisuallyHidden } from '@/components'
import { ListingRow } from './ListingRow'
import { ScoreHelp } from './ScoreHelp'

const NUMERIC_COLUMNS = ['Price', 'Beds', 'Baths', 'Sq ft'] as const

export function ResultsTable({ listings }: { listings: Listing[] }) {
  return (
    <TableContainer component={Paper} variant="outlined">
      <Table size="small" sx={{ '& td, & th': { py: 1.25 } }}>
        <caption>
          <VisuallyHidden>Listings ranked by relevance</VisuallyHidden>
        </caption>
        <TableHead>
          <TableRow>
            <TableCell>Address</TableCell>
            {NUMERIC_COLUMNS.map((label) => (
              <TableCell key={label} align="right">
                {label}
              </TableCell>
            ))}
            <TableCell>Status</TableCell>
            <TableCell>Listed</TableCell>
            <TableCell align="right">
              Score
              <ScoreHelp />
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {listings.map((listing) => (
            // ids are only unique per feed, so the key includes the source.
            <ListingRow key={`${listing.source}:${listing.id}`} listing={listing} />
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  )
}
