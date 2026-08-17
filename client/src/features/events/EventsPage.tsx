import {
  Anchor,
  Badge,
  Button,
  Group,
  Loader,
  MultiSelect,
  Pagination,
  Paper,
  Skeleton,
  Stack,
  Table,
  Text,
  Title,
  Tooltip,
  UnstyledButton,
} from '@mantine/core'
import {
  IconAlertTriangle,
  IconChevronDown,
  IconChevronUp,
  IconFilterOff,
  IconHistory,
  IconSelector,
} from '@tabler/icons-react'
import { useMemo, useState } from 'react'
import { Link } from 'react-router'
import { StatePanel } from '../../components/StatePanel'
import { useBooks, useBookTitles } from '../books/useBooks'
import { formatAbsoluteTime, formatClockTime, formatRelativeTime, useNow } from '../../lib/time'
import { changeTypeAppearance } from './changeType'
import {
  DEFAULT_EVENTS_SORT,
  EVENTS_PAGE_SIZE,
  useBookEvents,
  type BookChangeType,
  type BookEvent,
  type BookEventSort,
  type BookEventSortField,
} from './useBookEvents'

const CLOCK_TICK_MS = 30_000

const SKELETON_ROW_COUNT = 6

const DESCENDING_FIRST: Record<BookEventSortField, boolean> = {
  OccurredAt: true,
  BookTitle: false,
}

const CHANGE_TYPES: BookChangeType[] = ['Created', 'TitleChanged', 'DescriptionChanged']

const CHANGE_TYPE_OPTIONS = CHANGE_TYPES.map((value) => ({
  value,
  label: changeTypeAppearance(value).label,
}))

function SortableTh({
  field,
  label,
  width,
  sort,
  onSort,
}: {
  field: BookEventSortField
  label: string
  width?: string | number
  sort: BookEventSort
  onSort: (field: BookEventSortField) => void
}) {
  const active = sort.field === field
  const Icon = active ? (sort.descending ? IconChevronDown : IconChevronUp) : IconSelector

  return (
    <Table.Th w={width} aria-sort={active ? (sort.descending ? 'descending' : 'ascending') : 'none'}>
      <UnstyledButton onClick={() => onSort(field)} w="100%" aria-label={`Sort by ${label}`}>
        <Group gap={6} wrap="nowrap" justify="space-between">
          <Text fz="sm" fw={700}>
            {label}
          </Text>
          <Icon size={14} opacity={active ? 1 : 0.4} style={{ flexShrink: 0 }} />
        </Group>
      </UnstyledButton>
    </Table.Th>
  )
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
  const [sort, setSort] = useState<BookEventSort>(DEFAULT_EVENTS_SORT)
  const [changeTypes, setChangeTypes] = useState<BookChangeType[]>([])
  const [bookIds, setBookIds] = useState<string[]>([])
  const { data, isPending, isPlaceholderData, isError, error, refetch } = useBookEvents(
    page,
    sort,
    changeTypes,
    bookIds,
  )
  const bookTitles = useBookTitles()
  const { data: books } = useBooks()
  const now = useNow(CLOCK_TICK_MS)

  const bookOptions = useMemo(
    () => (books ?? []).map((book) => ({ value: book.id, label: book.title })),
    [books],
  )

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

  const scrollToTop = () => window.scrollTo({ top: 0, behavior: 'smooth' })

  const goToPage = (next: number) => {
    setPage(next)
    scrollToTop()
  }

  const toggleSort = (field: BookEventSortField) => {
    setSort((current) =>
      current.field === field
        ? { field, descending: !current.descending }
        : { field, descending: DESCENDING_FIRST[field] },
    )
    setPage(1)
    scrollToTop()
  }

  const applyChangeTypes = (selected: string[]) => {
    setChangeTypes(selected as BookChangeType[])
    setPage(1)
  }

  const applyBooks = (selected: string[]) => {
    setBookIds(selected)
    setPage(1)
  }

  const clearFilters = () => {
    setChangeTypes([])
    setBookIds([])
    setPage(1)
  }

  const isFiltered = changeTypes.length > 0 || bookIds.length > 0

  return (
    <Stack gap="lg">
      <Stack gap={2}>
        <Group gap="sm" align="center">
          <Title order={2}>Events</Title>
          {isPlaceholderData && <Loader size="xs" />}
        </Group>
      </Stack>

      <Group gap="sm" align="flex-end" wrap="wrap">
        <MultiSelect
          data={bookOptions}
          value={bookIds}
          onChange={applyBooks}
          placeholder={bookIds.length > 0 ? undefined : 'All books'}
          aria-label="Filter by book"
          searchable
          nothingFoundMessage="No books match"
          w={{ base: '100%', sm: 320 }}
        />

        <MultiSelect
          data={CHANGE_TYPE_OPTIONS}
          value={changeTypes}
          onChange={applyChangeTypes}
          placeholder={changeTypes.length > 0 ? undefined : 'All change types'}
          aria-label="Filter by change type"
          w={{ base: '100%', sm: 280 }}
        />

        {isFiltered && (
          <Button variant="subtle" color="gray" onClick={clearFilters}>
            Clear filters
          </Button>
        )}
      </Group>

      {isPending ? (
        <EventsSkeleton />
      ) : totalCount === 0 ? (
        isFiltered ? (
          <StatePanel
            icon={IconFilterOff}
            title="No matching changes"
            description="No activity matches the selected filters."
            action={{ label: 'Clear filters', onClick: clearFilters }}
          />
        ) : (
          <StatePanel
            icon={IconHistory}
            title="Nothing has happened yet"
            description="When a book is created or edited, it will show up here instantly."
          />
        )
      ) : (
        <>
          <Paper withBorder radius="lg" style={{ overflow: 'hidden' }}>
            <Table.ScrollContainer minWidth={720}>
              <Table striped highlightOnHover stickyHeader verticalSpacing="md">
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th w={190}>Change</Table.Th>
                    <SortableTh
                      field="BookTitle"
                      label="Book"
                      width="26%"
                      sort={sort}
                      onSort={toggleSort}
                    />
                    <Table.Th>Details</Table.Th>
                    <SortableTh
                      field="OccurredAt"
                      label="When"
                      width={150}
                      sort={sort}
                      onSort={toggleSort}
                    />
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
              {`Showing ${firstShown}–${lastShown} of ${totalCount} ${isFiltered ? 'matching ' : ''}${totalCount === 1 ? 'change' : 'changes'}`}
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
