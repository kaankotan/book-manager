namespace BookManager.Application.Books.Dtos;

public record BookDto(Guid Id, string Title, string Description, DateOnly PublishedDate, IReadOnlyList<AuthorDto> Authors);
