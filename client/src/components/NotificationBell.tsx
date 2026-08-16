import {
  ActionIcon,
  Anchor,
  Box,
  Divider,
  Group,
  Indicator,
  Popover,
  ScrollArea,
  Stack,
  Text,
  ThemeIcon,
} from '@mantine/core'
import { useState } from 'react'
import { IconBell, IconBellOff } from '@tabler/icons-react'
import { Link } from 'react-router'
import { changeTypeAppearance } from '../features/events/changeType'
import { useBookTitles } from '../features/books/useBooks'
import { formatRelativeTime, useNow } from '../lib/time'
import { useNotificationInbox } from '../realtime/BookEventsProvider'
import type { BookEvent } from '../features/events/useBookEvents'

const MAX_DISPLAYED_COUNT = 99

const CLOCK_TICK_MS = 30_000

function NotificationRow({
  event,
  bookTitle,
  now,
}: {
  event: BookEvent
  bookTitle: string | undefined
  now: number
}) {
  const { label, color, icon: Icon } = changeTypeAppearance(event.changeType)

  return (
    <Group gap="sm" wrap="nowrap" align="flex-start" px="md" py="xs">
      <ThemeIcon variant="light" color={color} size={28} radius="xl" mt={2}>
        <Icon size={15} />
      </ThemeIcon>

      <Box style={{ flex: 1, minWidth: 0 }}>
        <Group gap={6} wrap="nowrap">
          <Text size="sm" fw={600} c={`${color}.7`} style={{ flexShrink: 0 }}>
            {label}
          </Text>
          <Text size="sm" c="dimmed" lineClamp={1}>
            {bookTitle ?? `Book ${event.bookId.slice(0, 8)}`}
          </Text>
        </Group>

        {event.newValue !== null && event.newValue !== undefined && (
          <Text size="xs" c="dimmed" lineClamp={2}>
            {event.newValue}
          </Text>
        )}
      </Box>

      <Text size="xs" c="dimmed" style={{ flexShrink: 0 }}>
        {formatRelativeTime(event.occurredAt, now)}
      </Text>
    </Group>
  )
}

export function NotificationBell() {
  const { items, unreadCount, markAllRead } = useNotificationInbox()
  const [opened, setOpened] = useState(false)
  const bookTitles = useBookTitles()
  const now = useNow(CLOCK_TICK_MS)

  const label = unreadCount === 0 ? 'Notifications' : `Notifications, ${unreadCount} unread`

  const handleToggle = () => {
    if (!opened) {
      markAllRead()
    }

    setOpened(!opened)
  }

  return (
    <Popover
      width={360}
      position="bottom-end"
      shadow="lg"
      radius="lg"
      opened={opened}
      onChange={setOpened}
    >
      <Popover.Target>
        <ActionIcon
          variant="subtle"
          color="gray"
          size="lg"
          aria-label={label}
          onClick={handleToggle}
        >
          <Indicator
            color="red"
            size={16}
            offset={-2}
            disabled={unreadCount === 0}
            label={unreadCount > MAX_DISPLAYED_COUNT ? `${MAX_DISPLAYED_COUNT}+` : unreadCount}
          >
            <IconBell size={19} />
          </Indicator>
        </ActionIcon>
      </Popover.Target>

      <Popover.Dropdown p={0}>
        <Group justify="space-between" px="md" py="sm">
          <Text fw={700} size="sm">
            Notifications
          </Text>
          {items.length > 0 && (
            <Text size="xs" c="dimmed">
              Latest {items.length}
            </Text>
          )}
        </Group>

        <Divider />

        {items.length === 0 ? (
          <Stack align="center" gap={6} px="md" py="xl">
            <ThemeIcon variant="light" color="gray" size={40} radius="xl">
              <IconBellOff size={20} />
            </ThemeIcon>
            <Text size="sm" fw={500} mt={4}>
              All caught up
            </Text>
            <Text size="xs" c="dimmed" ta="center">
              New changes will appear here as they happen.
            </Text>
          </Stack>
        ) : (
          <ScrollArea.Autosize mah={340}>
            <Stack gap={0} py={4}>
              {items.map((event) => (
                <NotificationRow
                  key={event.id}
                  event={event}
                  bookTitle={bookTitles.get(event.bookId)}
                  now={now}
                />
              ))}
            </Stack>
          </ScrollArea.Autosize>
        )}

        <Divider />

        <Group justify="center" py="xs">
          <Anchor component={Link} to="/events" size="sm" fw={500} onClick={() => setOpened(false)}>
            View all activity
          </Anchor>
        </Group>
      </Popover.Dropdown>
    </Popover>
  )
}
