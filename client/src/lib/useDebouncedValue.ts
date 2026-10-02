import { useEffect, useState } from 'react'

/**
 * Returns `value` once it has stopped changing for `delayMs`. Every change restarts the timer,
 * so a burst of keystrokes produces a single update after the user pauses.
 */
export function useDebouncedValue<T>(value: T, delayMs: number): T {
  const [debounced, setDebounced] = useState(value)

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs)
    return () => clearTimeout(timer)
  }, [value, delayMs])

  return debounced
}
