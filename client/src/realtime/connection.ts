import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { API_BASE_URL } from '../api/client'

export const BOOK_EVENT_CREATED = 'BookEventCreated'

export function createBookEventsConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/book-events`)
    .withStatefulReconnect()
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}
