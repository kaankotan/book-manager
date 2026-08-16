import {
  ActionIcon,
  Badge,
  Group,
  Loader,
  Paper,
  Skeleton,
  Stack,
  Table,
  Text,
  TextInput,
  Title,
} from '@mantine/core'
import { IconAlertTriangle, IconBook, IconSearch, IconX } from '@tabler/icons-react'
import { useDeferredValue, useMemo, useState } from 'react'
import { StatePanel } from '../../components/StatePanel'
import { formatPublishedDate } from '../../lib/time'
import { useBooks, type Book } from './useBooks'

const SKELETON_ROW_COUNT = 5

function matchesQuery(book: Book, query: string): boolean {
  const haystack = [book.title, book.description, ...book.authors.map((author) => author.name)]

  return haystack.some((value) => value.toLowerCase().includes(query))
}

function BookRow({ book }: { book: Book }) {
  const published = formatPublishedDate(book.publishedDate)

  return (
    <Table.Tr>
      <Table.Td>
        <Text fw={600} size="sm" lineClamp={1}>
          {book.title}
        </Text>
      </Table.Td>

      <Table.Td>
        <Text size="sm" c="dimmed" lineClamp={2}>
          {book.description}
        </Text>
      </Table.Td>

      <Table.Td>
        {book.authors.length > 0 ? (
          <Group gap={6}>
            {book.authors.map((author) => (
              <Badge key={author.id} variant="light" color="ink" size="sm">
                {author.name}
              </Badge>
            ))}
          </Group>
        ) : (
          <Text size="sm" c="dimmed" fs="italic">
            Unattributed
          </Text>
        )}
      </Table.Td>

      <Table.Td>
        <Text size="sm" c={published === null ? 'dimmed' : undefined}>
          {published ?? '—'}
        </Text>
      </Table.Td>
    </Table.Tr>
  )
}

function BooksSkeleton() {
  return (
    <Stack gap="sm">
      {Array.from({ length: SKELETON_ROW_COUNT }, (_, index) => (
        <Skeleton key={index} height={44} radius="sm" />
      ))}
    </Stack>
  )
}

export function BooksPage() {
  const { data: books, isPending, isError, error, isFetching, refetch } = useBooks()
  const [query, setQuery] = useState('')
  const deferredQuery = useDeferredValue(query)

  const visibleBooks = useMemo(() => {
    const normalized = deferredQuery.trim().toLowerCase()

    if (normalized.length === 0) {
      return books ?? []
    }

    return (books ?? []).filter((book) => matchesQuery(book, normalized))
  }, [books, deferredQuery])

  if (isError) {
    return (
      <StatePanel
        icon={IconAlertTriangle}
        color="red"
        title="Could not load books"
        description={error.message}
        action={{ label: 'Try again', onClick: () => void refetch() }}
      />
    )
  }

  const total = books?.length ?? 0
  const isFiltered = deferredQuery.trim().length > 0

  return (
    <Stack gap="lg">
      <Group justify="space-between" align="flex-end" wrap="wrap" gap="sm">
        <Stack gap={2}>
          <Group gap="sm" align="center">
            <Title order={2}>Books</Title>
            {isFetching && !isPending && <Loader size="xs" />}
          </Group>
          <Text size="sm" c="dimmed">
            {isPending
              ? 'Loading the catalogue…'
              : isFiltered
                ? `${visibleBooks.length} of ${total} ${total === 1 ? 'title' : 'titles'}`
                : `${total} ${total === 1 ? 'title' : 'titles'} in the catalogue`}
          </Text>
        </Stack>

        <TextInput
          value={query}
          onChange={(event) => setQuery(event.currentTarget.value)}
          placeholder="Search titles, descriptions, authors"
          aria-label="Search books"
          leftSection={<IconSearch size={16} />}
          rightSection={
            query.length > 0 ? (
              <ActionIcon
                variant="subtle"
                color="gray"
                onClick={() => setQuery('')}
                aria-label="Clear search"
              >
                <IconX size={14} />
              </ActionIcon>
            ) : null
          }
          w={{ base: '100%', sm: 300 }}
        />
      </Group>

      {isPending ? (
        <BooksSkeleton />
      ) : total === 0 ? (
        <StatePanel
          icon={IconBook}
          title="No books yet"
          description="Once a book is added it will show up here, and the change will appear in the activity feed."
        />
      ) : visibleBooks.length === 0 ? (
        <StatePanel
          icon={IconSearch}
          title="No matches"
          description={`Nothing in the catalogue matches “${deferredQuery.trim()}”.`}
          action={{ label: 'Clear search', onClick: () => setQuery('') }}
        />
      ) : (
        <Paper withBorder radius="lg" style={{ overflow: 'hidden' }}>
          <Table.ScrollContainer minWidth={720}>
            <Table
              striped
              highlightOnHover
              stickyHeader
              stickyHeaderOffset={60}
              verticalSpacing="md"
            >
              <Table.Thead>
                <Table.Tr>
                  <Table.Th w="26%">Title</Table.Th>
                  <Table.Th>Description</Table.Th>
                  <Table.Th w="22%">Authors</Table.Th>
                  <Table.Th w={140}>Published</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {visibleBooks.map((book) => (
                  <BookRow key={book.id} book={book} />
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        </Paper>
      )}
    </Stack>
  )
}
