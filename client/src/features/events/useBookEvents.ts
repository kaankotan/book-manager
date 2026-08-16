import { useInfiniteQuery } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { components } from '../../api/schema'

export type BookEvent = components['schemas']['BookEventDto']
export type BookEventPage = components['schemas']['BookEventPageDto']

export const eventsQueryKey = ['events'] as const

const PAGE_SIZE = 50

export function useBookEvents() {
  return useInfiniteQuery({
    queryKey: eventsQueryKey,
    initialPageParam: null as number | null,
    queryFn: ({ pageParam, signal }) =>
      unwrap(
        api.GET('/api/events', {
          params: { query: { before: pageParam ?? undefined, limit: PAGE_SIZE } },
          signal,
        }),
      ),
    getNextPageParam: (lastPage: BookEventPage) => lastPage.nextCursor ?? undefined,
  })
}
