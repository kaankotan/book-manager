import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import { bookQueryKey } from './useBook'
import { booksQueryKey } from './useBooks'
import type { components } from '../../api/schema'

export type NewBook = components['schemas']['AddBookCommand']

export type BookDetails = { title: string; description: string }

export function useAddBook() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (book: NewBook) => unwrap(api.POST('/api/books', { body: book })),
    onSuccess: (book) => {
      queryClient.setQueryData(bookQueryKey(book.id), book)
      void queryClient.invalidateQueries({ queryKey: booksQueryKey })
    },
  })
}

export function useUpdateBook(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ title, description }: BookDetails) =>
      unwrap(
        api.PUT('/api/books/{id}', {
          params: { path: { id } },
          body: { id, title, description },
        }),
      ),
    onSuccess: (book) => {
      queryClient.setQueryData(bookQueryKey(book.id), book)
      void queryClient.invalidateQueries({ queryKey: booksQueryKey })
    },
  })
}
