import { AppShell, Container, Group, Title } from '@mantine/core'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { BooksPage } from './features/books/BooksPage'
import { RealtimeStatusBadge } from './realtime/RealtimeStatusBadge'

export default function App() {
  return (
    <BrowserRouter>
      <AppShell header={{ height: 60 }} padding="md">
        <AppShell.Header>
          <Group h="100%" px="md" justify="space-between">
            <Title order={3}>Book Manager</Title>
            <RealtimeStatusBadge />
          </Group>
        </AppShell.Header>

        <AppShell.Main>
          <Container size="lg">
            <Routes>
              <Route path="/" element={<Navigate to="/books" replace />} />
              <Route path="/books" element={<BooksPage />} />
            </Routes>
          </Container>
        </AppShell.Main>
      </AppShell>
    </BrowserRouter>
  )
}
