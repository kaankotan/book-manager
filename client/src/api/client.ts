import createClient from 'openapi-fetch'
import type { paths } from './schema'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export const api = createClient<paths>({ baseUrl: API_BASE_URL })

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

type ProblemDetails = {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

function problemMessage(error: unknown): string | null {
  if (typeof error !== 'object' || error === null) {
    return null
  }

  const { title, detail, errors } = error as ProblemDetails

  return Object.values(errors ?? {}).flat()[0] ?? detail ?? title ?? null
}

export async function unwrap<T>(
  result: Promise<{ data?: T; error?: unknown; response: Response }>,
): Promise<T> {
  const { data, error, response } = await result

  if (!response.ok) {
    throw new ApiError(
      response.status,
      problemMessage(error) ?? `${response.status} ${response.statusText}`,
    )
  }

  if (data === undefined) {
    throw new ApiError(response.status, 'Response body was empty')
  }

  return data
}
