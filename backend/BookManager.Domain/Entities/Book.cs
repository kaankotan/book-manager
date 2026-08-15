namespace BookManager.Domain.Entities;

public class Book
{
    public const int TitleMaxLength = 1024;

    public const int DescriptionMaxLength = 1024;

    private readonly List<Author> _authors = new();

    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public DateOnly PublishedDate { get; private set; }

    public IReadOnlyCollection<Author> Authors => _authors.AsReadOnly();

    private Book() { }

    public Book(string title, string description, DateOnly publishedDate, IEnumerable<Author>? authors = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        PublishedDate = publishedDate;

        if (authors is not null)
        {
            _authors.AddRange(authors);
        }
    }

    public void UpdateDetails(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public void AddAuthor(Author author)
    {
        _authors.Add(author);
    }
}
