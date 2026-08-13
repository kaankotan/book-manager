namespace BookManager.Application.Books.Dtos;

public record BookDto(Guid Id, string Title, string Author, string? Isbn, int? PublishedYear);
