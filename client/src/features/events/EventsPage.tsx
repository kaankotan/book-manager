import {
  Box,
  Button,
  Center,
  Group,
  Paper,
  Skeleton,
  Stack,
  Text,
  ThemeIcon,
  Title,
  Tooltip,
} from '@mantine/core'
import { IconAlertTriangle, IconHistory } from '@tabler/icons-react'
import { useMemo } from 'react'
import { StatePanel } from '../../components/StatePanel'
import { useBookTitles } from '../books/useBooks'
import {
  dayKey,
  formatAbsoluteTime,
  formatClockTime,
  formatDayLabel,
  formatRelativeTime,
  useNow,
} from '../../lib/time'
import { changeTypeAppearance } from './changeType'
import { useBookEvents, type BookEvent } from './useBookEvents'

const CLOCK_TICK_MS = 30_000

const SKELETON_ROW_COUNT = 6

type DayGroup = {
  key: string
  sample: string
  items: BookEvent[]
}

function groupByDay(events: BookEvent[]): DayGroup[] {
  const groups: DayGroup[] = []

  for (const event of events) {
    const key = dayKey(event.occurredAt)
    const current = groups.at(-1)

    if (current?.key === key) {
      current.items.push(event)
    } else {
      groups.push({ key, sample: event.occurredAt, items: [event] })
    }
  }

  return groups
}

function EventRow({
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
    <Group
      className="event-row"
      align="flex-start"
      wrap="nowrap"
      gap="md"
      px="md"
      py="sm"
      style={{ transition: 'background 120ms ease' }}
    >
      <ThemeIcon variant="light" color={color} size={34} radius="xl" mt={2}>
        <Icon size={17} />
      </ThemeIcon>

      <Box style={{ flex: 1, minWidth: 0 }}>
        <Group gap={8} wrap="nowrap">
          <Text size="sm" fw={600} c={`${color}.7`}>
            {label}
          </Text>
          <Text size="sm" c="dimmed" lineClamp={1}>
            {bookTitle ?? `Book ${event.bookId.slice(0, 8)}`}
          </Text>
        </Group>

        {event.newValue !== null && event.newValue !== undefined && (
          <Text size="sm" c="dimmed" lineClamp={2} mt={2}>
            {event.newValue}
          </Text>
        )}
      </Box>

      <Tooltip label={formatAbsoluteTime(event.occurredAt)} position="left">
        <Stack gap={0} align="flex-end" style={{ cursor: 'default', flexShrink: 0 }}>
          <Text size="xs" c="dimmed" fw={500}>
            {formatRelativeTime(event.occurredAt, now)}
          </Text>
          <Text fz={10} c="dimmed">
            {formatClockTime(event.occurredAt)}
          </Text>
        </Stack>
      </Tooltip>
    </Group>
  )
}

function EventsSkeleton() {
  return (
    <Stack gap="sm">
      {Array.from({ length: SKELETON_ROW_COUNT }, (_, index) => (
        <Skeleton key={index} height={56} radius="md" />
      ))}
    </Stack>
  )
}

export function EventsPage() {
  const {
    data,
    isPending,
    isError,
    error,
    refetch,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useBookEvents()
  const bookTitles = useBookTitles()
  const now = useNow(CLOCK_TICK_MS)

  const events = useMemo(() => data?.pages.flatMap((page) => page.items) ?? [], [data])
  const groups = useMemo(() => groupByDay(events), [events])

  if (isError) {
    return (
      <StatePanel
        icon={IconAlertTriangle}
        color="red"
        title="Could not load activity"
        description={error.message}
        action={{ label: 'Try again', onClick: () => void refetch() }}
      />
    )
  }

  return (
    <Stack gap="lg">
      <Stack gap={2}>
        <Title order={2}>Activity</Title>
        <Text size="sm" c="dimmed">
          Every change to the catalogue, newest first.
        </Text>
      </Stack>

      {isPending ? (
        <EventsSkeleton />
      ) : events.length === 0 ? (
        <StatePanel
          icon={IconHistory}
          title="Nothing has happened yet"
          description="When a book is created or edited, it will show up here instantly."
        />
      ) : (
        <>
          <Stack gap="lg">
            {groups.map((group) => (
              <Stack key={group.key} gap={6}>
                <Text className="day-heading" size="xs" fw={700} c="dimmed" tt="uppercase" py={4}>
                  {formatDayLabel(group.sample, now)}
                </Text>

                <Paper withBorder radius="lg" style={{ overflow: 'hidden' }}>
                  <Stack gap={0}>
                    {group.items.map((event) => (
                      <EventRow
                        key={event.id}
                        event={event}
                        bookTitle={bookTitles.get(event.bookId)}
                        now={now}
                      />
                    ))}
                  </Stack>
                </Paper>
              </Stack>
            ))}
          </Stack>

          <Center pb="xl">
            {hasNextPage ? (
              <Button
                variant="default"
                onClick={() => void fetchNextPage()}
                loading={isFetchingNextPage}
              >
                Load older
              </Button>
            ) : (
              <Text size="sm" c="dimmed">
                That is the whole history.
              </Text>
            )}
          </Center>
        </>
      )}
    </Stack>
  )
}
