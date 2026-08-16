import {
  ActionIcon,
  Anchor,
  Badge,
  Divider,
  Group,
  Indicator,
  Popover,
  ScrollArea,
  Stack,
  Text,
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconBell } from '@tabler/icons-react'
import { Link } from 'react-router'
import { changeTypeColor } from '../features/events/changeType'
import { useNotificationInbox } from '../realtime/BookEventsProvider'
import type { BookEvent } from '../features/events/useBookEvents'

const MAX_DISPLAYED_COUNT = 99

function formatTime(value: string): string {
  const date = new Date(value)

  return Number.isNaN(date.getTime()) ? value : date.toLocaleTimeString()
}

function NotificationRow({ event }: { event: BookEvent }) {
  return (
    <Group gap="sm" wrap="nowrap" px="md" py="xs">
      <Badge color={changeTypeColor(event.changeType)} variant="light" size="sm">
        {event.changeType}
      </Badge>

      <Text size="sm" lineClamp={1} style={{ flex: 1 }}>
        {event.newValue ?? `Book ${event.bookId.slice(0, 8)}`}
      </Text>

      <Text size="xs" c="dimmed">
        {formatTime(event.occurredAt)}
      </Text>
    </Group>
  )
}

export function NotificationBell() {
  const { items, unreadCount, markAllRead } = useNotificationInbox()
  const [opened, { toggle, close }] = useDisclosure(false, { onOpen: markAllRead })

  return (
    <Popover
      width={380}
      position="bottom-end"
      shadow="md"
      opened={opened}
      onChange={(nextOpened) => {
        if (!nextOpened) {
          close()
        }
      }}
    >
      <Popover.Target>
        <ActionIcon
          variant="subtle"
          size="lg"
          aria-label={`Notifications, ${unreadCount} unread`}
          onClick={toggle}
        >
          <Indicator
            color="red"
            size={16}
            offset={-2}
            disabled={unreadCount === 0}
            label={unreadCount > MAX_DISPLAYED_COUNT ? `${MAX_DISPLAYED_COUNT}+` : unreadCount}
          >
            <IconBell size={20} />
          </Indicator>
        </ActionIcon>
      </Popover.Target>

      <Popover.Dropdown p={0}>
        <Group justify="space-between" px="md" py="xs">
          <Text fw={600} size="sm">
            Notifications
          </Text>
          {items.length > 0 && (
            <Text size="xs" c="dimmed">
              Last {items.length}
            </Text>
          )}
        </Group>

        <Divider />

        {items.length === 0 ? (
          <Text size="sm" c="dimmed" ta="center" px="md" py="lg">
            Nothing yet. New events will appear here.
          </Text>
        ) : (
          <ScrollArea.Autosize mah={320}>
            <Stack gap={0}>
              {items.map((event) => (
                <NotificationRow key={event.id} event={event} />
              ))}
            </Stack>
          </ScrollArea.Autosize>
        )}

        <Divider />

        <Group justify="center" py="xs">
          <Anchor component={Link} to="/events" size="sm" onClick={close}>
            View all activity
          </Anchor>
        </Group>
      </Popover.Dropdown>
    </Popover>
  )
}
