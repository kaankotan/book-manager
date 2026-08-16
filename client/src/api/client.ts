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

export async function unwrap<T>(result: Promise<{ data?: T; response: Response }>): Promise<T> {
  const { data, response } = await result

  if (!response.ok) {
    throw new ApiError(response.status, `${response.status} ${response.statusText}`)
  }

  if (data === undefined) {
    throw new ApiError(response.status, 'Response body was empty')
  }

  return data
}
