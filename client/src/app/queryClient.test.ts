import { describe, expect, it } from 'vitest'
import { ApiError } from '@/api'
import { shouldRetry } from './queryClient'

describe('shouldRetry', () => {
  it.each([
    ['network', undefined],
    ['timeout', undefined],
    ['server', 503],
  ] as const)('retries transient %s failures', (kind, status) => {
    expect(shouldRetry(0, new ApiError(kind, 'x', { status }))).toBe(true)
  })

  it.each([
    ['validation', 400],
    ['rateLimited', 429],
    ['server', 404],
  ] as const)('does not retry %s (%i)', (kind, status) => {
    expect(shouldRetry(0, new ApiError(kind, 'x', { status }))).toBe(false)
  })

  it('stops after two retries', () => {
    expect(shouldRetry(2, new ApiError('network', 'x'))).toBe(false)
  })
})
