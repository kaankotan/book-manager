import { Button, Paper, Stack, Text, ThemeIcon } from '@mantine/core'
import type { ComponentType, ReactNode } from 'react'

type StatePanelProps = {
  icon: ComponentType<{ size?: number | string; stroke?: number }>
  title: string
  description?: ReactNode
  color?: string
  action?: { label: string; onClick: () => void }
}

export function StatePanel({
  icon: Icon,
  title,
  description,
  color = 'gray',
  action,
}: StatePanelProps) {
  return (
    <Paper withBorder p="xl" radius="lg">
      <Stack align="center" gap="xs" py="lg">
        <ThemeIcon variant="light" color={color} size={52} radius="xl">
          <Icon size={26} />
        </ThemeIcon>

        <Text fw={600} mt="xs">
          {title}
        </Text>

        {description !== undefined && (
          <Text size="sm" c="dimmed" ta="center" maw={420}>
            {description}
          </Text>
        )}

        {action !== undefined && (
          <Button variant="light" size="sm" mt="sm" onClick={action.onClick}>
            {action.label}
          </Button>
        )}
      </Stack>
    </Paper>
  )
}
