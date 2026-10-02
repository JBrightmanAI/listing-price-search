import type { ReactNode } from 'react'
import Box from '@mui/material/Box'

const hidden = {
  border: 0,
  clip: 'rect(0 0 0 0)',
  height: '1px',
  margin: '-1px',
  overflow: 'hidden',
  padding: 0,
  position: 'absolute',
  whiteSpace: 'nowrap',
  width: '1px',
} as const

interface Props {
  children: ReactNode
  as?: 'span' | 'div' | 'h2'
  id?: string
}

/** Content for screen readers only: present in the accessibility tree, invisible on screen. */
export function VisuallyHidden({ children, as = 'span', id }: Props) {
  return (
    <Box component={as} id={id} sx={hidden}>
      {children}
    </Box>
  )
}
