import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { components } from '../../api/schema'

export type BookEvent = components['schemas']['BookEventDto']
export type BookEventPage = components['schemas']['BookEventPageDto']
export type BookEventSortField = components['schemas']['BookEventSortField']
export type BookChangeType = components['schemas']['BookChangeType']

export type BookEventSort = {
  field: BookEventSortField
  descending: boolean
}

export const eventsQueryKey = ['events'] as const

export const EVENTS_PAGE_SIZE = 25

export const DEFAULT_EVENTS_SORT: BookEventSort = { field: 'OccurredAt', descending: true }

export function useBookEvents(
  page: number,
  sort: BookEventSort,
  changeTypes: BookChangeType[],
  bookIds: string[],
) {
  const selectedTypes = [...changeTypes].sort()
  const selectedBooks = [...bookIds].sort()

  return useQuery({
    queryKey: [
      ...eventsQueryKey,
      page,
      EVENTS_PAGE_SIZE,
      sort.field,
      sort.descending,
      selectedTypes,
      selectedBooks,
    ] as const,
    queryFn: ({ signal }) =>
      unwrap(
        api.GET('/api/events', {
          params: {
            query: {
              page,
              pageSize: EVENTS_PAGE_SIZE,
              sortBy: sort.field,
              descending: sort.descending,
              changeTypes: selectedTypes,
              bookIds: selectedBooks,
            },
          },
          signal,
        }),
      ),
    placeholderData: keepPreviousData,
  })
}
