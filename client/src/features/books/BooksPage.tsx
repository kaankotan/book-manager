import { Alert, Center, Group, Loader, Stack, Table, Text, Title } from '@mantine/core'
import { useBooks, type Book } from './useBooks'

function formatPublishedDate(value: string): string {
  const date = new Date(value)

  return Number.isNaN(date.getTime()) ? value : date.toLocaleDateString()
}

function BookRow({ book }: { book: Book }) {
  return (
    <Table.Tr>
      <Table.Td>{book.title}</Table.Td>
      <Table.Td>
        <Text size="sm" lineClamp={1}>
          {book.description}
        </Text>
      </Table.Td>
      <Table.Td>
        {book.authors.length > 0 ? (
          book.authors.map((author) => author.name).join(', ')
        ) : (
          <Text c="dimmed" size="sm">
            No authors
          </Text>
        )}
      </Table.Td>
      <Table.Td>{formatPublishedDate(book.publishedDate)}</Table.Td>
    </Table.Tr>
  )
}

export function BooksPage() {
  const { data: books, isPending, isError, error, isFetching } = useBooks()

  if (isPending) {
    return (
      <Center h={240}>
        <Loader />
      </Center>
    )
  }

  if (isError) {
    return (
      <Alert color="red" title="Could not load books">
        {error.message}
      </Alert>
    )
  }

  return (
    <Stack gap="md">
      <Group justify="space-between" align="center">
        <Title order={2}>Books</Title>
        {isFetching && <Loader size="xs" />}
      </Group>

      {books.length === 0 ? (
        <Text c="dimmed">No books yet.</Text>
      ) : (
        <Table striped highlightOnHover withTableBorder>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Title</Table.Th>
              <Table.Th>Description</Table.Th>
              <Table.Th>Authors</Table.Th>
              <Table.Th>Published</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {books.map((book) => (
              <BookRow key={book.id} book={book} />
            ))}
          </Table.Tbody>
        </Table>
      )}
    </Stack>
  )
}
