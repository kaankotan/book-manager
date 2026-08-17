import {
  Anchor,
  Badge,
  Group,
  Loader,
  Pagination,
  Paper,
  Skeleton,
  Stack,
  Table,
  Text,
  Title,
  Tooltip,
} from '@mantine/core'
import { IconAlertTriangle, IconHistory } from '@tabler/icons-react'
import { useState } from 'react'
import { Link } from 'react-router'
import { StatePanel } from '../../components/StatePanel'
import { useBookTitles } from '../books/useBooks'
import { formatAbsoluteTime, formatClockTime, formatRelativeTime, useNow } from '../../lib/time'
import { changeTypeAppearance } from './changeType'
import { EVENTS_PAGE_SIZE, useBookEvents, type BookEvent } from './useBookEvents'

const CLOCK_TICK_MS = 30_000

const SKELETON_ROW_COUNT = 6

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
    <Table.Tr>
      <Table.Td>
        <Badge variant="light" color={color} size="sm" leftSection={<Icon size={12} />}>
          {label}
        </Badge>
      </Table.Td>

      <Table.Td>
        <Anchor component={Link} to={`/books/${event.bookId}`} fw={600} size="sm" lineClamp={1}>
          {bookTitle ?? `Book ${event.bookId.slice(0, 8)}`}
        </Anchor>
      </Table.Td>

      <Table.Td>
        {event.newValue !== null && event.newValue !== undefined ? (
          <Text size="sm" c="dimmed" lineClamp={2}>
            {event.newValue}
          </Text>
        ) : (
          <Text size="sm" c="dimmed">
            —
          </Text>
        )}
      </Table.Td>

      <Table.Td>
        <Tooltip label={formatAbsoluteTime(event.occurredAt)} position="left">
          <Stack gap={0} style={{ cursor: 'default' }}>
            <Text size="sm" fw={500}>
              {formatRelativeTime(event.occurredAt, now)}
            </Text>
            <Text fz={10} c="dimmed">
              {formatClockTime(event.occurredAt)}
            </Text>
          </Stack>
        </Tooltip>
      </Table.Td>
    </Table.Tr>
  )
}

function EventsSkeleton() {
  return (
    <Stack gap="sm">
      {Array.from({ length: SKELETON_ROW_COUNT }, (_, index) => (
        <Skeleton key={index} height={48} radius="md" />
      ))}
    </Stack>
  )
}

export function EventsPage() {
  const [page, setPage] = useState(1)
  const { data, isPending, isPlaceholderData, isError, error, refetch } = useBookEvents(page)
  const bookTitles = useBookTitles()
  const now = useNow(CLOCK_TICK_MS)

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

  const events = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.ceil(totalCount / EVENTS_PAGE_SIZE)
  const firstShown = (page - 1) * EVENTS_PAGE_SIZE + 1
  const lastShown = firstShown + events.length - 1

  const goToPage = (next: number) => {
    setPage(next)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <Stack gap="lg">
      <Stack gap={2}>
        <Group gap="sm" align="center">
          <Title order={2}>Activity</Title>
          {isPlaceholderData && <Loader size="xs" />}
        </Group>
        <Text size="sm" c="dimmed">
          Every change to the catalogue, newest first.
        </Text>
      </Stack>

      {isPending ? (
        <EventsSkeleton />
      ) : totalCount === 0 ? (
        <StatePanel
          icon={IconHistory}
          title="Nothing has happened yet"
          description="When a book is created or edited, it will show up here instantly."
        />
      ) : (
        <>
          <Paper withBorder radius="lg" style={{ overflow: 'hidden' }}>
            <Table.ScrollContainer minWidth={720}>
              <Table striped highlightOnHover stickyHeader verticalSpacing="md">
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th w={190}>Change</Table.Th>
                    <Table.Th w="26%">Book</Table.Th>
                    <Table.Th>Details</Table.Th>
                    <Table.Th w={150}>When</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {events.map((event) => (
                    <EventRow
                      key={event.id}
                      event={event}
                      bookTitle={bookTitles.get(event.bookId)}
                      now={now}
                    />
                  ))}
                </Table.Tbody>
              </Table>
            </Table.ScrollContainer>
          </Paper>

          <Group justify="space-between" align="center" wrap="wrap" gap="sm" pb="xl">
            <Text size="sm" c="dimmed">
              {`Showing ${firstShown}–${lastShown} of ${totalCount} ${totalCount === 1 ? 'change' : 'changes'}`}
            </Text>

            {totalPages > 1 && (
              <Pagination value={page} onChange={goToPage} total={totalPages} withEdges />
            )}
          </Group>
        </>
      )}
    </Stack>
  )
}
