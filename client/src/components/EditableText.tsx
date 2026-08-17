import { ActionIcon, Box, Button, Group, Stack, Textarea, TextInput, Tooltip } from '@mantine/core'
import { IconPencil } from '@tabler/icons-react'
import { useState, type ChangeEvent, type KeyboardEvent, type ReactNode } from 'react'

type EditableTextProps = {
  value: string
  label: string
  multiline?: boolean
  validate?: (value: string) => string | null
  onSave: (value: string) => Promise<unknown>
  children: ReactNode
}

export function EditableText({
  value,
  label,
  multiline = false,
  validate,
  onSave,
  children,
}: EditableTextProps) {
  const [draft, setDraft] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  const cancel = () => {
    setDraft(null)
    setError(null)
  }

  const save = async () => {
    if (draft === null) {
      return
    }

    const next = draft.trim()
    const invalid = validate?.(next) ?? null

    if (invalid !== null) {
      setError(invalid)
      return
    }

    if (next === value) {
      cancel()
      return
    }

    setSaving(true)

    try {
      await onSave(next)
      cancel()
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : `Could not save the ${label.toLowerCase()}`)
    } finally {
      setSaving(false)
    }
  }

  if (draft === null) {
    return (
      <Group gap="xs" wrap="nowrap" align={multiline ? 'flex-start' : 'center'}>
        <Box style={{ flex: 1, minWidth: 0 }}>{children}</Box>

        <Tooltip label={`Edit ${label.toLowerCase()}`} position="left">
          <ActionIcon
            variant="subtle"
            color="gray"
            aria-label={`Edit ${label.toLowerCase()}`}
            onClick={() => setDraft(value)}
          >
            <IconPencil size={16} />
          </ActionIcon>
        </Tooltip>
      </Group>
    )
  }

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    if (event.key === 'Escape') {
      cancel()
    }

    if (event.key === 'Enter' && !multiline) {
      event.preventDefault()
      void save()
    }
  }

  const fieldProps = {
    value: draft,
    onChange: (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
      setDraft(event.currentTarget.value),
    onKeyDown: handleKeyDown,
    error,
    disabled: saving,
    autoFocus: true,
    'aria-label': label,
  }

  return (
    <Stack gap="xs">
      {multiline ? (
        <Textarea {...fieldProps} autosize minRows={4} maxRows={14} />
      ) : (
        <TextInput {...fieldProps} size="md" />
      )}

      <Group gap="xs">
        <Button size="xs" onClick={() => void save()} loading={saving}>
          Save
        </Button>
        <Button size="xs" variant="subtle" color="gray" onClick={cancel} disabled={saving}>
          Cancel
        </Button>
      </Group>
    </Stack>
  )
}
