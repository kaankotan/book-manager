using BookManager.Application.Books.Dtos;
using BookManager.Application.Events.Dtos;
using BookManager.Domain.Entities;
using BookManager.Tests.Common;

namespace BookManager.Tests.Application.Common;

public class MappingProfileTests
{
    private static readonly DateOnly PublishedDate = new(2024, 5, 1);

    private readonly AutoMapper.IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Configuration_IsValid()
    {
        // Catches DTO members that no longer have a matching source property.
        TestMapper.Configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_BookToBookDto_CopiesEveryMember()
    {
        var herbert = new Author("Frank Herbert");
        var book = new Book("Dune", "A desert epic", PublishedDate, [herbert]);

        var dto = _mapper.Map<BookDto>(book);

        Assert.Equal(book.Id, dto.Id);
        Assert.Equal("Dune", dto.Title);
        Assert.Equal("A desert epic", dto.Description);
        Assert.Equal(PublishedDate, dto.PublishedDate);
        Assert.Equal(herbert.Id, Assert.Single(dto.Authors).Id);
    }

    [Fact]
    public void Map_AuthorToAuthorDto_CopiesEveryMember()
    {
        var herbert = new Author("Frank Herbert");

        var dto = _mapper.Map<AuthorDto>(herbert);

        Assert.Equal(herbert.Id, dto.Id);
        Assert.Equal("Frank Herbert", dto.Name);
    }

    [Fact]
    public void Map_BookEventToBookEventDto_RendersTheChangeTypeAsItsName()
    {
        var bookEvent = BookEventFactory.WithId(7, changeType: BookChangeType.DescriptionChanged, newValue: "A sandy epic");

        var dto = _mapper.Map<BookEventDto>(bookEvent);

        Assert.Equal(7L, dto.Id);
        Assert.Equal(bookEvent.BookId, dto.BookId);
        Assert.Equal(nameof(BookChangeType.DescriptionChanged), dto.ChangeType);
        Assert.Equal("A sandy epic", dto.NewValue);
        Assert.Equal(bookEvent.OccurredAt, dto.OccurredAt);
    }
}
