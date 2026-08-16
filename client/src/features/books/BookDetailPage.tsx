import {
  Anchor,
  Badge,
  Box,
  Group,
  Paper,
  Skeleton,
  Stack,
  Text,
  ThemeIcon,
  Title,
} from '@mantine/core'
import {
  IconAlertTriangle,
  IconArrowLeft,
  IconBook,
  IconBookOff,
  IconCalendar,
  IconUsers,
} from '@tabler/icons-react'
import { Link, useParams } from 'react-router'
import { StatePanel } from '../../components/StatePanel'
import { formatPublishedDate } from '../../lib/time'
import { isNotFound, useBook } from './useBook'
import type { Book } from './useBooks'

function BackLink() {
  return (
    <Anchor component={Link} to="/books" size="sm" c="dimmed" w="fit-content">
      <Group gap={6} wrap="nowrap">
        <IconArrowLeft size={15} />
        All books
      </Group>
    </Anchor>
  )
}

function MetaItem({
  icon: Icon,
  label,
  children,
}: {
  icon: typeof IconUsers
  label: string
  children: React.ReactNode
}) {
  return (
    <Group gap="sm" wrap="nowrap" align="flex-start">
      <ThemeIcon variant="light" color="gray" size={34} radius="md">
        <Icon size={17} />
      </ThemeIcon>

      <Box>
        <Text fz={11} tt="uppercase" fw={700} c="dimmed" lh={1.4}>
          {label}
        </Text>
        {children}
      </Box>
    </Group>
  )
}

function BookDetail({ book }: { book: Book }) {
  const published = formatPublishedDate(book.publishedDate)

  return (
    <Stack gap="lg">
      <BackLink />

      <Paper withBorder radius="lg" p="xl">
        <Group gap="lg" wrap="nowrap" align="flex-start">
          <ThemeIcon
            variant="gradient"
            gradient={{ from: 'ink.6', to: 'ink.4', deg: 135 }}
            size={56}
            radius="lg"
            visibleFrom="xs"
          >
            <IconBook size={30} />
          </ThemeIcon>

          <Stack gap="md" style={{ flex: 1, minWidth: 0 }}>
            <Title order={1}>{book.title}</Title>

            <Group gap="xl" wrap="wrap">
              <MetaItem icon={IconUsers} label="Authors">
                {book.authors.length > 0 ? (
                  <Group gap={6} mt={4}>
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
              </MetaItem>

              <MetaItem icon={IconCalendar} label="Published">
                <Text size="sm" c={published === null ? 'dimmed' : undefined} fw={500}>
                  {published ?? 'Unknown'}
                </Text>
              </MetaItem>
            </Group>
          </Stack>
        </Group>
      </Paper>

      <Paper withBorder radius="lg" p="xl">
        <Text fz={11} tt="uppercase" fw={700} c="dimmed" mb="sm">
          Description
        </Text>

        {book.description.trim().length > 0 ? (
          <Text style={{ whiteSpace: 'pre-wrap' }} maw={680}>
            {book.description}
          </Text>
        ) : (
          <Text c="dimmed" fs="italic">
            No description was provided for this book.
          </Text>
        )}
      </Paper>
    </Stack>
  )
}

function BookDetailSkeleton() {
  return (
    <Stack gap="lg">
      <Skeleton height={20} width={100} radius="sm" />
      <Skeleton height={168} radius="lg" />
      <Skeleton height={140} radius="lg" />
    </Stack>
  )
}

export function BookDetailPage() {
  const { id = '' } = useParams<{ id: string }>()
  const { data: book, isPending, isError, error, refetch } = useBook(id)

  if (isPending) {
    return <BookDetailSkeleton />
  }

  if (isError) {
    return (
      <Stack gap="lg">
        <BackLink />
        {isNotFound(error) ? (
          <StatePanel
            icon={IconBookOff}
            title="Book not found"
            description="This book may have been removed, or the link is incorrect."
          />
        ) : (
          <StatePanel
            icon={IconAlertTriangle}
            color="red"
            title="Could not load this book"
            description={error.message}
            action={{ label: 'Try again', onClick: () => void refetch() }}
          />
        )}
      </Stack>
    )
  }

  return <BookDetail book={book} />
}
