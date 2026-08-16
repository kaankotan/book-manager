import {
  Alert,
  Badge,
  Button,
  Center,
  Group,
  Loader,
  Stack,
  Table,
  Text,
  Title,
} from '@mantine/core'
import { changeTypeColor } from './changeType'
import { useBookEvents } from './useBookEvents'

function formatTimestamp(value: string): string {
  const date = new Date(value)

  return Number.isNaN(date.getTime()) ? value : date.toLocaleString()
}

export function EventsPage() {
  const { data, isPending, isError, error, fetchNextPage, hasNextPage, isFetchingNextPage } =
    useBookEvents()

  if (isPending) {
    return (
      <Center h={240}>
        <Loader />
      </Center>
    )
  }

  if (isError) {
    return (
      <Alert color="red" title="Could not load events">
        {error.message}
      </Alert>
    )
  }

  const events = data.pages.flatMap((page) => page.items)

  return (
    <Stack gap="md">
      <Title order={2}>Activity</Title>

      {events.length === 0 ? (
        <Text c="dimmed">Nothing has happened yet.</Text>
      ) : (
        <>
          <Table striped highlightOnHover withTableBorder>
            <Table.Thead>
              <Table.Tr>
                <Table.Th w={200}>When</Table.Th>
                <Table.Th w={190}>Change</Table.Th>
                <Table.Th>New value</Table.Th>
                <Table.Th w={110}>Book</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {events.map((event) => (
                <Table.Tr key={event.id}>
                  <Table.Td>{formatTimestamp(event.occurredAt)}</Table.Td>
                  <Table.Td>
                    <Badge color={changeTypeColor(event.changeType)} variant="light">
                      {event.changeType}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm" lineClamp={1}>
                      {event.newValue ?? '—'}
                    </Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="xs" c="dimmed" ff="monospace">
                      {event.bookId.slice(0, 8)}
                    </Text>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>

          <Group justify="center">
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
          </Group>
        </>
      )}
    </Stack>
  )
}
