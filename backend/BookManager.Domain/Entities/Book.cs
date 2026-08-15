namespace BookManager.Domain.Entities;

public class Book
{
    public const int TitleMaxLength = 1024;

    public const int DescriptionMaxLength = 1024;

    private readonly List<Author> _authors = new();

    private readonly List<BookChange> _changes = new();

    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public DateOnly PublishedDate { get; private set; }

    public IReadOnlyCollection<Author> Authors => _authors.AsReadOnly();

    public IReadOnlyCollection<BookChange> PendingChanges => _changes.AsReadOnly();

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

        _changes.Add(new BookChange(BookChangeType.Created, title));
    }

    public void UpdateDetails(string title, string description)
    {
        if (!string.Equals(Title, title, StringComparison.Ordinal))
        {
            Title = title;
            _changes.Add(new BookChange(BookChangeType.TitleChanged, title));
        }

        if (!string.Equals(Description, description, StringComparison.Ordinal))
        {
            Description = description;
            _changes.Add(new BookChange(BookChangeType.DescriptionChanged, description));
        }
    }

    public void ClearPendingChanges()
    {
        _changes.Clear();
    }

    public void AddAuthor(Author author)
    {
        _authors.Add(author);
    }
}
