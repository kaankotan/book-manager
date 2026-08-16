import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiError, api, unwrap } from '../../api/client'
import { useNotificationInbox } from '../../realtime/BookEventsProvider'
import type { components } from '../../api/schema'

export type UnseenBookChanges = components['schemas']['UnseenBookChangesDto']

const CHANGE_LIMIT = 50

export function unseenChangesQueryKey(bookId: string) {
  return ['book-unseen-changes', bookId] as const
}

export function useUnseenChanges(bookId: string) {
  return useQuery({
    queryKey: unseenChangesQueryKey(bookId),
    queryFn: ({ signal }) =>
      unwrap(
        api.GET('/api/books/{bookId}/unseen-changes', {
          params: { path: { bookId }, query: { limit: CHANGE_LIMIT } },
          signal,
        }),
      ),
    staleTime: 0,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  })
}

export function useMarkBookSeen(bookId: string) {
  const highestMarked = useRef(0)

  const { mutate } = useMutation({
    mutationFn: async (lastSeenEventId: number) => {
      const { response } = await api.PUT('/api/books/{bookId}/view', {
        params: { path: { bookId } },
        body: { bookId, lastSeenEventId },
      })

      if (!response.ok) {
        throw new ApiError(response.status, `${response.status} ${response.statusText}`)
      }
    },
  })

  return useCallback(
    (lastSeenEventId: number | null | undefined) => {
      if (lastSeenEventId == null || lastSeenEventId <= highestMarked.current) {
        return
      }

      highestMarked.current = lastSeenEventId
      mutate(lastSeenEventId)
    },
    [mutate],
  )
}

export function useUnseenChangesAnnouncement(bookId: string) {
  const { data } = useUnseenChanges(bookId)
  const markSeen = useMarkBookSeen(bookId)
  const { items: notifications } = useNotificationInbox()
  const [opened, setOpened] = useState(false)
  const alreadyDecided = useRef(false)

  const bufferedBeforeMount = useRef<Set<number> | null>(null)

  if (bufferedBeforeMount.current === null) {
    bufferedBeforeMount.current = new Set(notifications.map((event) => event.id))
  }

  const liveLatestEventId = useMemo(() => {
    const arrivedWhileOpen = notifications.filter(
      (event) => event.bookId === bookId && !bufferedBeforeMount.current!.has(event.id),
    )

    return arrivedWhileOpen.length > 0 ? Math.max(...arrivedWhileOpen.map((e) => e.id)) : null
  }, [notifications, bookId])

  useEffect(() => {
    if (data === undefined || alreadyDecided.current) {
      return
    }

    alreadyDecided.current = true

    if (!data.firstView && data.items.length > 0) {
      setOpened(true)
    }

    markSeen(data.latestEventId)
  }, [data, markSeen])

  useEffect(() => {
    markSeen(liveLatestEventId)
  }, [liveLatestEventId, markSeen])

  const close = useCallback(() => setOpened(false), [])

  return { opened, close, items: data?.items ?? [], hasMore: data?.hasMore ?? false }
}
