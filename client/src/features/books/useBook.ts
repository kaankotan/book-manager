import { useQuery } from '@tanstack/react-query'
import { ApiError, api, unwrap } from '../../api/client'
import { booksQueryKey } from './useBooks'

const MAX_RETRIES = 1

export function bookQueryKey(id: string) {
  return [...booksQueryKey, id] as const
}

export function isNotFound(error: unknown): boolean {
  return error instanceof ApiError && error.status === 404
}

export function useBook(id: string) {
  return useQuery({
    queryKey: bookQueryKey(id),
    queryFn: ({ signal }) =>
      unwrap(api.GET('/api/books/{id}', { params: { path: { id } }, signal })),
    retry: (failureCount, error) => !isNotFound(error) && failureCount < MAX_RETRIES,
  })
}
