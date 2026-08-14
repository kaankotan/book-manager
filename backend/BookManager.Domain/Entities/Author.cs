namespace BookManager.Domain.Entities;

public class Author
{
    private readonly List<Book> _books = new();

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<Book> Books => _books.AsReadOnly();

    private Author() { }

    public Author(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}
