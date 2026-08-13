using BookManager.Domain.Entities;

namespace BookManager.Application.Books.Dtos;

public static class BookMappingExtensions
{
    public static BookDto ToDto(this Book book)
    {
        return new BookDto(book.Id, book.Title, book.Author, book.Isbn, book.PublishedYear);
    }
}
