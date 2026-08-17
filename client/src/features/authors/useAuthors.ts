import { useQuery } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { components } from '../../api/schema'

export type Author = components['schemas']['AuthorDto']

export const authorsQueryKey = ['authors'] as const

export function useAuthors() {
  return useQuery({
    queryKey: authorsQueryKey,
    queryFn: ({ signal }) => unwrap(api.GET('/api/authors', { signal })),
  })
}
