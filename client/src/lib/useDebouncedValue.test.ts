import { act, renderHook } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useDebouncedValue } from './useDebouncedValue'

describe('useDebouncedValue', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })
  afterEach(() => {
    vi.useRealTimers()
  })

  it('starts with the initial value', () => {
    const { result } = renderHook(() => useDebouncedValue('a', 400))

    expect(result.current).toBe('a')
  })

  it('updates only after the value has been stable for the delay', () => {
    const { result, rerender } = renderHook(({ value }) => useDebouncedValue(value, 400), {
      initialProps: { value: 'a' },
    })

    rerender({ value: 'ab' })
    act(() => {
      vi.advanceTimersByTime(399)
    })
    expect(result.current).toBe('a')

    act(() => {
      vi.advanceTimersByTime(1)
    })
    expect(result.current).toBe('ab')
  })

  it('restarts the timer on every change, so a burst produces one update', () => {
    const { result, rerender } = renderHook(({ value }) => useDebouncedValue(value, 400), {
      initialProps: { value: 'R' },
    })

    for (const value of ['Re', 'Res', 'Rest', 'Resto', 'Reston']) {
      rerender({ value })
      act(() => {
        vi.advanceTimersByTime(300)
      })
      expect(result.current).toBe('R')
    }

    act(() => {
      vi.advanceTimersByTime(400)
    })
    expect(result.current).toBe('Reston')
  })
})
