import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { components } from '../../api/schema'

export type Book = components['schemas']['BookDto']

export const booksQueryKey = ['books'] as const

export function useBooks() {
  return useQuery({
    queryKey: booksQueryKey,
    queryFn: ({ signal }) => unwrap(api.GET('/api/books', { signal })),
  })
}

export function useBookTitles(): ReadonlyMap<string, string> {
  const { data } = useBooks()

  return useMemo(() => new Map((data ?? []).map((book) => [book.id, book.title])), [data])
}
