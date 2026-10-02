import type { ReactNode } from 'react'
import HomeWorkOutlined from '@mui/icons-material/HomeWorkOutlined'
import AppBar from '@mui/material/AppBar'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import { ThemeModeToggle } from '@/components'

const MAIN_ID = 'main-content'

/** Page chrome: skip link, sticky header with the theme switch, and the main landmark. */
export function AppShell({ children }: { children: ReactNode }) {
  return (
    <>
      {/* First focusable element: keyboard users can jump past the header. Visible only when focused. */}
      <Box
        component="a"
        href={`#${MAIN_ID}`}
        sx={{
          position: 'absolute',
          left: 16,
          top: -48,
          zIndex: 'tooltip',
          px: 2,
          py: 1,
          borderRadius: 1,
          bgcolor: 'primary.main',
          color: 'primary.contrastText',
          '&:focus': { top: 16 },
        }}
      >
        Skip to content
      </Box>

      <AppBar
        position="sticky"
        color="inherit"
        elevation={0}
        sx={{ borderBottom: 1, borderColor: 'divider', bgcolor: 'background.paper' }}
      >
        <Container maxWidth="lg">
          <Toolbar disableGutters sx={{ gap: 1.5 }}>
            <HomeWorkOutlined color="primary" aria-hidden />
            <Typography variant="h6" component="span">
              Listing Search
            </Typography>
            <Box sx={{ flexGrow: 1 }} />
            <ThemeModeToggle />
          </Toolbar>
        </Container>
      </AppBar>

      <Container component="main" id={MAIN_ID} tabIndex={-1} maxWidth="lg" sx={{ py: { xs: 3, md: 5 }, outline: 0 }}>
        {children}
      </Container>
    </>
  )
}
