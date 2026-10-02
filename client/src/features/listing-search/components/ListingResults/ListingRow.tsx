import { memo } from 'react'
import TableCell from '@mui/material/TableCell'
import TableRow from '@mui/material/TableRow'
import Typography from '@mui/material/Typography'
import { type Listing } from '@/api'
import { formatDate, formatNumber, formatPrice } from '@/lib'
import { ScoreIndicator } from './ScoreIndicator'
import { StatusChip } from './StatusChip'

export const ListingRow = memo(function ListingRow({ listing }: { listing: Listing }) {
  return (
    <TableRow hover>
      <TableCell>
        <Typography variant="body2" sx={{ fontWeight: 600 }}>
          {listing.address}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {listing.city}, {listing.state} {listing.zip}
        </Typography>
      </TableCell>
      <TableCell align="right" sx={{ fontWeight: 700, whiteSpace: 'nowrap' }}>
        {formatPrice(listing.price)}
      </TableCell>
      <TableCell align="right">{listing.bedrooms}</TableCell>
      <TableCell align="right">{listing.bathrooms}</TableCell>
      <TableCell align="right">{formatNumber(listing.sqft)}</TableCell>
      <TableCell>
        <StatusChip status={listing.status} />
      </TableCell>
      <TableCell sx={{ whiteSpace: 'nowrap' }}>
        <Typography variant="body2">{formatDate(listing.listedDate)}</Typography>
        <Typography variant="caption" color="text.secondary">
          {listing.source}
        </Typography>
      </TableCell>
      <TableCell align="right">
        <ScoreIndicator score={listing.relevanceScore} />
      </TableCell>
    </TableRow>
  )
})
