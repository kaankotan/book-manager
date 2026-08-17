import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { components } from '../../api/schema'

export type BookEvent = components['schemas']['BookEventDto']
export type BookEventPage = components['schemas']['BookEventPageDto']

export const eventsQueryKey = ['events'] as const

export const EVENTS_PAGE_SIZE = 25

export function useBookEvents(page: number) {
  return useQuery({
    queryKey: [...eventsQueryKey, page, EVENTS_PAGE_SIZE] as const,
    queryFn: ({ signal }) =>
      unwrap(
        api.GET('/api/events', {
          params: { query: { page, pageSize: EVENTS_PAGE_SIZE } },
          signal,
        }),
      ),
    placeholderData: keepPreviousData,
  })
}
