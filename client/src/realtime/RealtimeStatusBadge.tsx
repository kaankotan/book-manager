import { Box, Group, Text, Tooltip } from '@mantine/core'
import { useRealtimeStatus, type RealtimeStatus } from './BookEventsProvider'

const APPEARANCE: Record<RealtimeStatus, { color: string; label: string; hint: string }> = {
  connecting: {
    color: 'var(--mantine-color-gray-5)',
    label: 'Connecting',
    hint: 'Opening the live connection',
  },
  connected: {
    color: 'var(--mantine-color-forest-5)',
    label: 'Live',
    hint: 'Changes appear here the moment they happen',
  },
  reconnecting: {
    color: 'var(--mantine-color-gold-4)',
    label: 'Reconnecting',
    hint: 'Connection dropped, trying again',
  },
  disconnected: {
    color: 'var(--mantine-color-red-6)',
    label: 'Offline',
    hint: 'Not receiving live updates, refresh to retry',
  },
}

export function RealtimeStatusBadge() {
  const status = useRealtimeStatus()
  const { color, label, hint } = APPEARANCE[status]

  return (
    <Tooltip label={hint} position="bottom-end">
      <Group gap={7} wrap="nowrap" style={{ cursor: 'default' }}>
        <Box
          w={8}
          h={8}
          style={{
            borderRadius: '50%',
            background: color,
            boxShadow:
              status === 'connected'
                ? `0 0 0 3px color-mix(in srgb, ${color} 25%, transparent)`
                : undefined,
          }}
        />
        <Text size="xs" c="dimmed" fw={500} visibleFrom="xs">
          {label}
        </Text>
      </Group>
    </Tooltip>
  )
}
