import { Badge } from '@mantine/core'
import { useRealtimeStatus, type RealtimeStatus } from './BookEventsProvider'

const APPEARANCE: Record<RealtimeStatus, { color: string; label: string }> = {
  connecting: { color: 'gray', label: 'Connecting' },
  connected: { color: 'green', label: 'Live' },
  reconnecting: { color: 'yellow', label: 'Reconnecting' },
  disconnected: { color: 'red', label: 'Offline' },
}

export function RealtimeStatusBadge() {
  const status = useRealtimeStatus()
  const { color, label } = APPEARANCE[status]

  return (
    <Badge color={color} variant="light">
      {label}
    </Badge>
  )
}
