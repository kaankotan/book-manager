import { Button, Group, Title } from '@mantine/core'
import { Link, useLocation } from 'react-router'
import { RealtimeStatusBadge } from '../realtime/RealtimeStatusBadge'
import { NotificationBell } from './NotificationBell'

const NAV_ITEMS = [
  { to: '/books', label: 'Books' },
  { to: '/events', label: 'Events' },
]

export function Header() {
  const { pathname } = useLocation()

  return (
    <Group h="100%" px="md" justify="space-between">
      <Group gap="xl">
        <Title order={3}>Book Manager</Title>

        <Group gap="xs">
          {NAV_ITEMS.map((item) => (
            <Button
              key={item.to}
              component={Link}
              to={item.to}
              size="xs"
              variant={pathname === item.to ? 'light' : 'subtle'}
            >
              {item.label}
            </Button>
          ))}
        </Group>
      </Group>

      <Group gap="sm">
        <NotificationBell />
        <RealtimeStatusBadge />
      </Group>
    </Group>
  )
}
