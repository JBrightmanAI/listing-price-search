import axios, { AxiosError, type AxiosRequestConfig } from 'axios'
import { type FieldErrors } from './types'

export type ApiErrorKind = 'validation' | 'rateLimited' | 'timeout' | 'network' | 'server'

interface ApiErrorOptions {
  status?: number | undefined
  fieldErrors?: FieldErrors | undefined
  retryAfterSeconds?: number | undefined
}

/** Every failure the UI can see, normalised into one shape it can switch on. */
export class ApiError extends Error {
  readonly kind: ApiErrorKind
  /** HTTP status, when the server answered at all. */
  readonly status: number | undefined
  readonly fieldErrors: FieldErrors
  readonly retryAfterSeconds: number | undefined

  constructor(kind: ApiErrorKind, message: string, options: ApiErrorOptions = {}) {
    super(message)
    this.name = 'ApiError'
    this.kind = kind
    this.status = options.status
    this.fieldErrors = options.fieldErrors ?? {}
    this.retryAfterSeconds = options.retryAfterSeconds
  }
}

export const DEFAULT_TIMEOUT_MS = 10_000

/**
 * Single configured client. By default it calls the same origin (Vite proxy in development, nginx in Docker).
 * VITE_API_BASE_URL points it at a separate API origin, which then needs that origin in the API's CORS list.
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '',
  timeout: DEFAULT_TIMEOUT_MS,
  headers: { Accept: 'application/json' },
  // Report timeouts as ETIMEDOUT instead of the generic ECONNABORTED.
  transitional: { clarifyTimeoutError: true },
})

interface ProblemDetails {
  title?: string
  errors?: FieldErrors
}

/**
 * GET a JSON resource. Cancelling through `signal` rejects with axios's CanceledError, which callers
 * ignore because a newer request replaced this one. Every other failure becomes an ApiError.
 */
export async function getJson<T>(url: string, options: Pick<AxiosRequestConfig, 'params' | 'signal'> = {}): Promise<T> {
  try {
    const response = await apiClient.get<T>(url, options)

    // axios silently hands back a string when a 2xx body is not JSON (e.g. a proxy's HTML page).
    if (typeof response.data !== 'object' || response.data === null) {
      throw new ApiError('server', 'The server sent a response that could not be read.', { status: response.status })
    }

    return response.data
  } catch (error) {
    if (axios.isCancel(error)) throw error
    throw toApiError(error)
  }
}

export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error

  if (!(error instanceof AxiosError)) {
    return new ApiError('server', 'Something went wrong. Please try again.')
  }

  if (error.code === AxiosError.ETIMEDOUT || error.code === AxiosError.ECONNABORTED) {
    return new ApiError('timeout', 'The server took too long to respond.')
  }

  const response = error.response
  if (!response) {
    return new ApiError('network', 'Could not reach the server. Check your connection and try again.')
  }

  // Error bodies are not guaranteed to be JSON, so only trust an object with the expected shape.
  const problem = typeof response.data === 'object' && response.data !== null ? (response.data as ProblemDetails) : null

  if (response.status === 400 && problem?.errors) {
    return new ApiError('validation', problem.title ?? 'Some search values are invalid.', {
      status: 400,
      fieldErrors: problem.errors,
    })
  }

  if (response.status === 429) {
    const retryAfter = Number(response.headers['retry-after'])
    return new ApiError('rateLimited', 'Too many requests. Please wait a moment and try again.', {
      status: 429,
      retryAfterSeconds: Number.isFinite(retryAfter) && retryAfter > 0 ? retryAfter : undefined,
    })
  }

  return new ApiError('server', 'Something went wrong on our side. Please try again.', { status: response.status })
}
