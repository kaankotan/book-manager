import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { API_BASE_URL } from '../api/client'
import type { components } from '../api/schema'

export type BookEvent = components['schemas']['BookEventDto']

/** The single method the server invokes on clients (SignalRBookEventNotifier.BookEventCreated). */
export const BOOK_EVENT_CREATED = 'BookEventCreated'

export function createBookEventsConnection(): HubConnection {
  return (
    new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/book-events`)
      // Pairs with AllowStatefulReconnects on the server: buffers messages across brief drops.
      // Requires the WebSockets transport, and only works alongside automatic reconnect.
      .withStatefulReconnect()
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()
  )
}
