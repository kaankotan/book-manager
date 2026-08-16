import { createContext, use, useEffect, useState, type ReactNode } from 'react'
import { useQueryClient, type InfiniteData } from '@tanstack/react-query'
import { notifications } from '@mantine/notifications'
import { changeTypeColor } from '../features/events/changeType'
import {
  eventsQueryKey,
  type BookEvent,
  type BookEventPage,
} from '../features/events/useBookEvents'
import { BOOK_EVENT_CREATED, createBookEventsConnection } from './connection'

export type RealtimeStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

type EventsCache = InfiniteData<BookEventPage, number | null>

const RealtimeStatusContext = createContext<RealtimeStatus>('disconnected')

export function useRealtimeStatus(): RealtimeStatus {
  return use(RealtimeStatusContext)
}

function alreadyContainsEvent(cache: EventsCache, eventId: number): boolean {
  return cache.pages.some((page) => page.items.some((item) => item.id === eventId))
}

function withEventPrepended(
  cache: EventsCache | undefined,
  event: BookEvent,
): EventsCache | undefined {
  const feedNotLoadedYet = !cache || cache.pages.length === 0

  if (feedNotLoadedYet || alreadyContainsEvent(cache, event.id)) {
    return cache
  }

  const [newestPage, ...olderPages] = cache.pages

  return {
    ...cache,
    pages: [{ ...newestPage, items: [event, ...newestPage.items] }, ...olderPages],
  }
}

export function BookEventsProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [status, setStatus] = useState<RealtimeStatus>('connecting')

  useEffect(() => {
    const connection = createBookEventsConnection()

    connection.on(BOOK_EVENT_CREATED, (event: BookEvent) => {
      queryClient.setQueryData<EventsCache>(eventsQueryKey, (cache) =>
        withEventPrepended(cache, event),
      )

      notifications.show({
        title: event.changeType,
        message: event.newValue ?? `Book ${event.bookId.slice(0, 8)}`,
        color: changeTypeColor(event.changeType),
        autoClose: 4000,
      })
    })

    connection.onreconnecting(() => setStatus('reconnecting'))
    connection.onreconnected(() => setStatus('connected'))
    connection.onclose(() => setStatus('disconnected'))

    const startAttempt = connection
      .start()
      .then(() => setStatus('connected'))
      .catch(() => setStatus('disconnected'))

    return () => {
      void startAttempt.then(() => connection.stop())
    }
  }, [queryClient])

  return <RealtimeStatusContext value={status}>{children}</RealtimeStatusContext>
}
