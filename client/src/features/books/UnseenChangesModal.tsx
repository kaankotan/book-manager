import { Button, Group, Modal, Stack, Text, ThemeIcon, Timeline, Tooltip } from '@mantine/core'
import { changeTypeAppearance } from '../events/changeType'
import type { BookEvent } from '../events/useBookEvents'
import { formatAbsoluteTime, formatRelativeTime, useNow } from '../../lib/time'

const CLOCK_TICK_MS = 30_000

function changeSummary(count: number): string {
  return count === 1 ? '1 change since your last visit' : `${count} changes since your last visit`
}

function ChangeItem({ event, now }: { event: BookEvent; now: number }) {
  const { label, color, icon: Icon } = changeTypeAppearance(event.changeType)

  return (
    <Timeline.Item
      bullet={
        <ThemeIcon variant="light" color={color} size={24} radius="xl">
          <Icon size={13} />
        </ThemeIcon>
      }
      title={
        <Group gap={8} justify="space-between" wrap="nowrap">
          <Text size="sm" fw={600} c={`${color}.7`}>
            {label}
          </Text>

          <Tooltip label={formatAbsoluteTime(event.occurredAt)} position="left">
            <Text size="xs" c="dimmed" fw={500} style={{ cursor: 'default', flexShrink: 0 }}>
              {formatRelativeTime(event.occurredAt, now)}
            </Text>
          </Tooltip>
        </Group>
      }
    >
      {event.newValue !== null && event.newValue !== undefined && (
        <Text size="sm" c="dimmed" lineClamp={3}>
          {event.newValue}
        </Text>
      )}
    </Timeline.Item>
  )
}

export function UnseenChangesModal({
  opened,
  onClose,
  bookTitle,
  items,
  hasMore,
}: {
  opened: boolean
  onClose: () => void
  bookTitle: string
  items: BookEvent[]
  hasMore: boolean
}) {
  const now = useNow(CLOCK_TICK_MS)

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={
        <Stack gap={2}>
          <Text fw={700}>{changeSummary(items.length)}</Text>
          <Text size="xs" c="dimmed" lineClamp={1}>
            {bookTitle}
          </Text>
        </Stack>
      }
      radius="lg"
      size="md"
      centered
    >
      <Stack gap="lg">
        <Timeline active={items.length} bulletSize={24} lineWidth={2}>
          {items.map((event) => (
            <ChangeItem key={event.id} event={event} now={now} />
          ))}
        </Timeline>

        {hasMore && (
          <Text size="xs" c="dimmed" fs="italic">
            Showing the {items.length} most recent changes.
          </Text>
        )}

        <Group justify="flex-end">
          <Button onClick={onClose}>Got it</Button>
        </Group>
      </Stack>
    </Modal>
  )
}
