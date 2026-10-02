import { AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import { describe, expect, it } from 'vitest'
import { ApiError, toApiError } from './http'

const config = { headers: {} } as InternalAxiosRequestConfig

function httpError(status: number, data: unknown, headers: Record<string, string> = {}) {
  const response = { status, statusText: '', data, headers, config } as AxiosResponse
  return new AxiosError(`Request failed with status ${status}`, AxiosError.ERR_BAD_RESPONSE, config, {}, response)
}

describe('toApiError', () => {
  it('maps a 400 ValidationProblemDetails to field errors', () => {
    const error = toApiError(httpError(400, { title: 'Invalid', errors: { minPrice: ['Too high.'] } }))

    expect(error.kind).toBe('validation')
    expect(error.fieldErrors).toEqual({ minPrice: ['Too high.'] })
  })

  it('treats a 400 without field errors as a server error', () => {
    expect(toApiError(httpError(400, 'Bad Request')).kind).toBe('server')
  })

  it('maps 429 and reads Retry-After', () => {
    const error = toApiError(httpError(429, {}, { 'retry-after': '30' }))

    expect(error.kind).toBe('rateLimited')
    expect(error.retryAfterSeconds).toBe(30)
  })

  it('ignores an unusable Retry-After value', () => {
    expect(toApiError(httpError(429, {}, { 'retry-after': 'soon' })).retryAfterSeconds).toBeUndefined()
  })

  it('maps a 500 with a non-JSON body (e.g. a proxy error page) to a server error', () => {
    const error = toApiError(httpError(502, '<html>Bad gateway</html>'))

    expect(error.kind).toBe('server')
    expect(error.status).toBe(502)
  })

  it('maps a timeout', () => {
    expect(toApiError(new AxiosError('timeout', AxiosError.ETIMEDOUT, config)).kind).toBe('timeout')
  })

  it('maps a request with no response to a network error', () => {
    expect(toApiError(new AxiosError('Network Error', AxiosError.ERR_NETWORK, config, {})).kind).toBe('network')
  })

  it('passes an ApiError through and wraps anything else', () => {
    const existing = new ApiError('timeout', 'slow')

    expect(toApiError(existing)).toBe(existing)
    expect(toApiError(new Error('boom')).kind).toBe('server')
  })
})
