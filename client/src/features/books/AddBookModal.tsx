import { Alert, Button, Group, Modal, MultiSelect, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { IconAlertTriangle } from '@tabler/icons-react'
import { useMemo } from 'react'
import { useAuthors } from '../authors/useAuthors'
import { todayAsDateInputValue } from '../../lib/time'
import { DESCRIPTION_MAX_LENGTH, TITLE_MAX_LENGTH, textFieldError } from './bookFields'
import { useAddBook } from './useBookMutations'

type NewBookForm = {
  title: string
  description: string
  publishedDate: string
  authorIds: string[]
}

export function AddBookModal({ opened, onClose }: { opened: boolean; onClose: () => void }) {
  const { data: authors, isPending: authorsPending, isError: authorsFailed } = useAuthors()
  const { mutate, isPending, error, reset: resetMutation } = useAddBook()

  const form = useForm<NewBookForm>({
    mode: 'uncontrolled',
    initialValues: {
      title: '',
      description: '',
      publishedDate: todayAsDateInputValue(),
      authorIds: [],
    },
    validate: {
      title: (value) => textFieldError('Title', value, TITLE_MAX_LENGTH),
      description: (value) => textFieldError('Description', value, DESCRIPTION_MAX_LENGTH),
      publishedDate: (value) => (value.length === 0 ? 'Published date is required' : null),
    },
  })

  const authorOptions = useMemo(
    () => (authors ?? []).map((author) => ({ value: author.id, label: author.name })),
    [authors],
  )

  const close = () => {
    form.reset()
    resetMutation()
    onClose()
  }

  const submit = form.onSubmit((values) => {
    mutate(
      {
        title: values.title.trim(),
        description: values.description.trim(),
        publishedDate: values.publishedDate,
        authorIds: values.authorIds,
      },
      { onSuccess: close },
    )
  })

  return (
    <Modal opened={opened} onClose={close} title="Add a book" radius="lg" size="md" centered>
      <form onSubmit={submit}>
        <Stack gap="md">
          {error !== null && (
            <Alert
              color="red"
              radius="md"
              icon={<IconAlertTriangle size={16} />}
              title="Could not add the book"
            >
              {error.message}
            </Alert>
          )}

          <TextInput
            label="Title"
            placeholder="Book title"
            withAsterisk
            data-autofocus
            key={form.key('title')}
            {...form.getInputProps('title')}
          />

          <Textarea
            label="Description"
            placeholder="What is this book about?"
            withAsterisk
            autosize
            minRows={3}
            maxRows={8}
            key={form.key('description')}
            {...form.getInputProps('description')}
          />

          <TextInput
            type="date"
            label="Published"
            withAsterisk
            key={form.key('publishedDate')}
            {...form.getInputProps('publishedDate')}
          />

          <MultiSelect
            label="Authors"
            placeholder={authorsPending ? 'Loading authors…' : 'Pick one or more authors'}
            data={authorOptions}
            searchable
            clearable
            disabled={authorsPending || authorsFailed}
            key={form.key('authorIds')}
            {...form.getInputProps('authorIds')}
            error={authorsFailed ? 'Authors could not be loaded' : undefined}
          />

          <Group justify="flex-end" gap="sm" mt="xs">
            <Button variant="subtle" color="gray" onClick={close} disabled={isPending}>
              Cancel
            </Button>
            <Button type="submit" loading={isPending}>
              Add book
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
