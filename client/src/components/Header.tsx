import { Box, Group, Text, ThemeIcon, UnstyledButton } from '@mantine/core'
import { IconBooks } from '@tabler/icons-react'
import { Link, useLocation } from 'react-router'
import { RealtimeStatusBadge } from '../realtime/RealtimeStatusBadge'
import { ColorSchemeToggle } from './ColorSchemeToggle'
import { NotificationBell } from './NotificationBell'

const NAV_ITEMS = [
  { to: '/books', label: 'Books' },
  { to: '/events', label: 'Activity' },
]

function NavLink({ to, label, active }: { to: string; label: string; active: boolean }) {
  return (
    <UnstyledButton
      component={Link}
      to={to}
      px="sm"
      py={6}
      aria-current={active ? 'page' : undefined}
      style={{
        borderRadius: 'var(--mantine-radius-md)',
        fontSize: 'var(--mantine-font-size-sm)',
        fontWeight: 600,
        color: active ? 'var(--mantine-color-ink-filled)' : 'var(--mantine-color-dimmed)',
        background: active ? 'var(--mantine-color-ink-light)' : 'transparent',
      }}
    >
      {label}
    </UnstyledButton>
  )
}

export function Header() {
  const { pathname } = useLocation()

  return (
    <Group h="100%" px="md" justify="space-between" wrap="nowrap">
      <Group gap="lg" wrap="nowrap">
        <UnstyledButton component={Link} to="/books" aria-label="Book Manager home">
          <Group gap="xs" wrap="nowrap">
            <ThemeIcon
              variant="gradient"
              gradient={{ from: 'ink.6', to: 'ink.4', deg: 135 }}
              size={32}
              radius="md"
            >
              <IconBooks size={19} />
            </ThemeIcon>
            <Box>
              <Text fw={700} size="sm" lh={1.1} c="var(--mantine-color-text)">
                Book Manager
              </Text>
              <Text fz={10} c="dimmed" lh={1.3} visibleFrom="sm">
                Catalogue &amp; activity
              </Text>
            </Box>
          </Group>
        </UnstyledButton>

        <Group gap={4} wrap="nowrap">
          {NAV_ITEMS.map((item) => (
            <NavLink key={item.to} to={item.to} label={item.label} active={pathname === item.to} />
          ))}
        </Group>
      </Group>

      <Group gap="xs" wrap="nowrap">
        <RealtimeStatusBadge />
        <NotificationBell />
        <ColorSchemeToggle />
      </Group>
    </Group>
  )
}
