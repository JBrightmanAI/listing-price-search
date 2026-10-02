import { ListingSearchPage } from '@/features/listing-search'
import { AppProviders } from './AppProviders'
import { AppShell } from './AppShell'

export function App() {
  return (
    <AppProviders>
      <AppShell>
        <ListingSearchPage />
      </AppShell>
    </AppProviders>
  )
}
