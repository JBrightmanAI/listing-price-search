import { screen, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { renderWithProviders } from '@/test'
import { ThemeModeToggle } from './ThemeModeToggle'

describe('ThemeModeToggle', () => {
  it('follows the system theme by default', async () => {
    renderWithProviders(<ThemeModeToggle />)

    expect(await screen.findByRole('button', { name: 'Match system theme' })).toHaveAttribute('aria-pressed', 'true')
  })

  it('switches to dark, applies it to the page and remembers it', async () => {
    const { user } = renderWithProviders(<ThemeModeToggle />)

    await user.click(await screen.findByRole('button', { name: 'Dark theme' }))

    expect(screen.getByRole('button', { name: 'Dark theme' })).toHaveAttribute('aria-pressed', 'true')
    await waitFor(() => expect(document.documentElement).toHaveClass('dark'))
    expect(localStorage.getItem('mui-mode')).toBe('dark')
  })

  it('keeps a theme selected when the active option is clicked again', async () => {
    const { user } = renderWithProviders(<ThemeModeToggle />)
    await user.click(await screen.findByRole('button', { name: 'Light theme' }))

    await user.click(screen.getByRole('button', { name: 'Light theme' }))

    expect(screen.getByRole('button', { name: 'Light theme' })).toHaveAttribute('aria-pressed', 'true')
  })
})
