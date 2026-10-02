import { memo, type ReactNode } from 'react'
import BathtubOutlined from '@mui/icons-material/BathtubOutlined'
import BedOutlined from '@mui/icons-material/BedOutlined'
import SquareFootOutlined from '@mui/icons-material/SquareFootOutlined'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { type Listing } from '@/api'
import { formatDate, formatNumber, formatPrice } from '@/lib'
import { ScoreIndicator } from './ScoreIndicator'
import { StatusChip } from './StatusChip'

function Fact({ icon, label }: { icon: ReactNode; label: string }) {
  return (
    <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center', color: 'text.secondary' }}>
      {icon}
      <Typography variant="body2">{label}</Typography>
    </Stack>
  )
}

/** The phone layout of one listing: the same facts as a table row, stacked for a narrow screen. */
export const ListingCard = memo(function ListingCard({ listing }: { listing: Listing }) {
  return (
    <Card variant="outlined">
      <CardContent sx={{ '&:last-child': { pb: 2 } }}>
        <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}>
          <Typography variant="h6" component="p">
            {formatPrice(listing.price)}
          </Typography>
          <ScoreIndicator score={listing.relevanceScore} />
        </Stack>

        <Typography variant="body1" component="h3" sx={{ fontWeight: 600, mt: 0.5 }}>
          {listing.address}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {listing.city}, {listing.state} {listing.zip}
        </Typography>

        <Stack direction="row" sx={{ gap: 2, mt: 1.5, flexWrap: 'wrap' }}>
          <Fact icon={<BedOutlined fontSize="small" />} label={`${listing.bedrooms} bd`} />
          <Fact icon={<BathtubOutlined fontSize="small" />} label={`${listing.bathrooms} ba`} />
          <Fact icon={<SquareFootOutlined fontSize="small" />} label={`${formatNumber(listing.sqft)} sq ft`} />
        </Stack>

        <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mt: 1.5 }}>
          <StatusChip status={listing.status} />
          <Typography variant="caption" color="text.secondary">
            {listing.source} · listed {formatDate(listing.listedDate)}
          </Typography>
        </Stack>
      </CardContent>
    </Card>
  )
})
