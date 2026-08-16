import {
  createContext,
  use,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
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

export type NotificationInbox = {
  items: BookEvent[]
  unreadCount: number
  markAllRead: () => void
}

type InboxState = Omit<NotificationInbox, 'markAllRead'>

type EventsCache = InfiniteData<BookEventPage, number | null>

const MAX_BUFFERED_NOTIFICATIONS = 20

const EMPTY_INBOX: InboxState = { items: [], unreadCount: 0 }

const RealtimeStatusContext = createContext<RealtimeStatus>('disconnected')

const NotificationInboxContext = createContext<NotificationInbox>({
  ...EMPTY_INBOX,
  markAllRead: () => {},
})

export function useRealtimeStatus(): RealtimeStatus {
  return use(RealtimeStatusContext)
}

export function useNotificationInbox(): NotificationInbox {
  return use(NotificationInboxContext)
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

function withEventBuffered(current: InboxState, event: BookEvent): InboxState {
  if (current.items.some((item) => item.id === event.id)) {
    return current
  }

  return {
    items: [event, ...current.items].slice(0, MAX_BUFFERED_NOTIFICATIONS),
    unreadCount: current.unreadCount + 1,
  }
}

export function BookEventsProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [status, setStatus] = useState<RealtimeStatus>('connecting')
  const [inbox, setInbox] = useState<InboxState>(EMPTY_INBOX)

  const markAllRead = useCallback(() => {
    setInbox((current) => (current.unreadCount === 0 ? current : { ...current, unreadCount: 0 }))
  }, [])

  const inboxValue = useMemo<NotificationInbox>(
    () => ({ ...inbox, markAllRead }),
    [inbox, markAllRead],
  )

  useEffect(() => {
    const connection = createBookEventsConnection()

    connection.on(BOOK_EVENT_CREATED, (event: BookEvent) => {
      queryClient.setQueryData<EventsCache>(eventsQueryKey, (cache) =>
        withEventPrepended(cache, event),
      )

      setInbox((current) => withEventBuffered(current, event))

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

  return (
    <RealtimeStatusContext value={status}>
      <NotificationInboxContext value={inboxValue}>{children}</NotificationInboxContext>
    </RealtimeStatusContext>
  )
}
