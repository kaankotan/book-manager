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
import { booksQueryKey, type Book } from '../features/books/useBooks'
import { changeTypeAppearance } from '../features/events/changeType'
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

const SEEN_EVENT_CAPACITY = 500

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

function createSeenEventLog() {
  const ids = new Set<number>()
  const order: number[] = []

  return {
    accept(id: number): boolean {
      if (ids.has(id)) {
        return false
      }

      ids.add(id)
      order.push(id)

      if (order.length > SEEN_EVENT_CAPACITY) {
        const evicted = order.shift()

        if (evicted !== undefined) {
          ids.delete(evicted)
        }
      }

      return true
    },
  }
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
    const seenEvents = createSeenEventLog()

    connection.on(BOOK_EVENT_CREATED, (event: BookEvent) => {
      if (!seenEvents.accept(event.id)) {
        return
      }

      queryClient.setQueryData<EventsCache>(eventsQueryKey, (cache) =>
        withEventPrepended(cache, event),
      )

      void queryClient.invalidateQueries({ queryKey: booksQueryKey })

      setInbox((current) => withEventBuffered(current, event))

      const { label, color, icon: Icon } = changeTypeAppearance(event.changeType)
      const bookTitle = queryClient
        .getQueryData<Book[]>(booksQueryKey)
        ?.find((book) => book.id === event.bookId)?.title

      notifications.show({
        title: label,
        message: bookTitle ?? event.newValue ?? `Book ${event.bookId.slice(0, 8)}`,
        color,
        icon: <Icon size={17} />,
        autoClose: 4500,
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
