import { createContext, use, useEffect, useState, type ReactNode } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { notifications } from '@mantine/notifications'
import { booksQueryKey } from '../features/books/useBooks'
import { BOOK_EVENT_CREATED, createBookEventsConnection, type BookEvent } from './connection'

export type RealtimeStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

const RealtimeStatusContext = createContext<RealtimeStatus>('disconnected')

export function useRealtimeStatus(): RealtimeStatus {
  return use(RealtimeStatusContext)
}

export function BookEventsProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [status, setStatus] = useState<RealtimeStatus>('connecting')

  useEffect(() => {
    const connection = createBookEventsConnection()

    connection.on(BOOK_EVENT_CREATED, (event: BookEvent) => {
      // Delivery is at-least-once and unordered, so the payload is only ever treated as a hint that
      // the server has newer data — never as the source of truth. Refetching keeps that honest.
      void queryClient.invalidateQueries({ queryKey: booksQueryKey })

      notifications.show({
        message: `${event.changeType} on book ${event.bookId.slice(0, 8)}`,
        color: 'blue',
        autoClose: 3000,
      })
    })

    connection.onreconnecting(() => setStatus('reconnecting'))
    connection.onreconnected(() => setStatus('connected'))
    connection.onclose(() => setStatus('disconnected'))

    const started = connection
      .start()
      .then(() => setStatus('connected'))
      .catch(() => setStatus('disconnected'))

    return () => {
      // StrictMode mounts twice in dev. Calling stop() while start() is still pending throws, so
      // the teardown always waits for the start attempt to settle first.
      void started.then(() => connection.stop())
    }
  }, [queryClient])

  return <RealtimeStatusContext value={status}>{children}</RealtimeStatusContext>
}
