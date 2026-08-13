namespace BookManager.Domain.Entities;

public class Book
{
    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string Author { get; private set; }

    public string? Isbn { get; private set; }

    public int? PublishedYear { get; private set; }

    public Book(
        string title,
        string author,
        string? isbn = null,
        int? publishedYear = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        Author = author;
        Isbn = isbn;
        PublishedYear = publishedYear;
    }
}