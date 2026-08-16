import { AppShell, Container } from '@mantine/core'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { Header } from './components/Header'
import { BooksPage } from './features/books/BooksPage'
import { EventsPage } from './features/events/EventsPage'

export default function App() {
  return (
    <BrowserRouter>
      <AppShell header={{ height: 60 }} padding="md">
        <AppShell.Header>
          <Header />
        </AppShell.Header>

        <AppShell.Main>
          <Container size="lg">
            <Routes>
              <Route path="/books" element={<BooksPage />} />
              <Route path="/events" element={<EventsPage />} />
              <Route path="*" element={<Navigate to="/books" replace />} />
            </Routes>
          </Container>
        </AppShell.Main>
      </AppShell>
    </BrowserRouter>
  )
}
