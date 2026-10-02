import Skeleton from '@mui/material/Skeleton'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { type ActiveFilter, type FilterKey } from '@/features/listing-search/model'
import { ActiveFilters } from './ActiveFilters'
import { PageSizeSelect } from './PageSizeSelect'

interface Props {
  /** undefined until the first response arrives. */
  totalCount: number | undefined
  rankedByBudget: boolean
  pageSize: string
  onPageSizeChange: (pageSize: string) => void
  filters: ActiveFilter[]
  onRemoveFilter: (key: FilterKey) => void
  onClearFilters: () => void
}

export function ResultsToolbar({
  totalCount,
  rankedByBudget,
  pageSize,
  onPageSizeChange,
  filters,
  onRemoveFilter,
  onClearFilters,
}: Props) {
  return (
    <Stack spacing={1.5}>
      <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
        <div>
          <Typography variant="h6" component="h2" id="results-heading">
            {totalCount === undefined ? (
              <Skeleton width={120} />
            ) : (
              `${totalCount.toLocaleString('en-US')} ${totalCount === 1 ? 'listing' : 'listings'}`
            )}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {rankedByBudget ? 'Ranked by closeness to your budget, then recency' : 'Ranked by recency'}
          </Typography>
        </div>
        <PageSizeSelect value={pageSize} onChange={onPageSizeChange} />
      </Stack>
      <ActiveFilters filters={filters} onRemove={onRemoveFilter} onClearAll={onClearFilters} />
    </Stack>
  )
}
