using BookManager.Application.Authors.Commands.AddAuthor;
using BookManager.Application.Authors.Queries.GetAllAuthors;
using BookManager.Application.Authors.Queries.GetAuthorById;
using BookManager.Application.Repositories.Authors;
using BookManager.Domain.Entities;
using BookManager.Tests.Common;
using NSubstitute;

namespace BookManager.Tests.Application.Authors;

public class AddAuthorCommandHandlerTests
{
    private readonly IAuthorRepository _authorRepository = Substitute.For<IAuthorRepository>();
    private readonly AddAuthorCommandHandler _handler;

    public AddAuthorCommandHandlerTests()
    {
        _handler = new AddAuthorCommandHandler(_authorRepository, TestMapper.Create());
    }

    [Fact]
    public async Task Handle_ReturnsTheMappedAuthorWithAGeneratedId()
    {
        var result = await _handler.Handle(new AddAuthorCommand("Frank Herbert"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Frank Herbert", result.Name);
    }

    [Fact]
    public async Task Handle_PersistsTheAuthor()
    {
        await _handler.Handle(new AddAuthorCommand("Frank Herbert"), CancellationToken.None);

        await _authorRepository
            .Received(1)
            .AddAsync(Arg.Is<Author>(author => author.Name == "Frank Herbert"), Arg.Any<CancellationToken>());
        await _authorRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReturnsTheIdThatWasPersisted()
    {
        Author? persisted = null;
        await _authorRepository.AddAsync(Arg.Do<Author>(author => persisted = author), Arg.Any<CancellationToken>());

        var result = await _handler.Handle(new AddAuthorCommand("Frank Herbert"), CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal(persisted.Id, result.Id);
    }

    [Fact]
    public async Task Handle_PassesTheCancellationTokenThrough()
    {
        using var cts = new CancellationTokenSource();

        await _handler.Handle(new AddAuthorCommand("Frank Herbert"), cts.Token);

        await _authorRepository.Received(1).AddAsync(Arg.Any<Author>(), cts.Token);
        await _authorRepository.Received(1).SaveChangesAsync(cts.Token);
    }
}

public class GetAllAuthorsQueryHandlerTests
{
    private readonly IAuthorRepository _authorRepository = Substitute.For<IAuthorRepository>();
    private readonly GetAllAuthorsQueryHandler _handler;

    public GetAllAuthorsQueryHandlerTests()
    {
        _handler = new GetAllAuthorsQueryHandler(_authorRepository, TestMapper.Create());
    }

    [Fact]
    public async Task Handle_MapsEveryAuthorInOrder()
    {
        var herbert = new Author("Frank Herbert");
        var anderson = new Author("Kevin J. Anderson");
        _authorRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([herbert, anderson]);

        var result = await _handler.Handle(new GetAllAuthorsQuery(), CancellationToken.None);

        Assert.Equal(["Frank Herbert", "Kevin J. Anderson"], result.Select(author => author.Name));
        Assert.Equal([herbert.Id, anderson.Id], result.Select(author => author.Id));
    }

    [Fact]
    public async Task Handle_WithNoAuthors_ReturnsAnEmptyList()
    {
        _authorRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await _handler.Handle(new GetAllAuthorsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_PassesTheCancellationTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        _authorRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        await _handler.Handle(new GetAllAuthorsQuery(), cts.Token);

        await _authorRepository.Received(1).GetAllAsync(cts.Token);
    }
}

public class GetAuthorByIdQueryHandlerTests
{
    private readonly IAuthorRepository _authorRepository = Substitute.For<IAuthorRepository>();
    private readonly GetAuthorByIdQueryHandler _handler;

    public GetAuthorByIdQueryHandlerTests()
    {
        _handler = new GetAuthorByIdQueryHandler(_authorRepository, TestMapper.Create());
    }

    [Fact]
    public async Task Handle_WithAnExistingAuthor_ReturnsTheMappedAuthor()
    {
        var herbert = new Author("Frank Herbert");
        _authorRepository.GetByIdAsync(herbert.Id, Arg.Any<CancellationToken>()).Returns(herbert);

        var result = await _handler.Handle(new GetAuthorByIdQuery(herbert.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(herbert.Id, result.Id);
        Assert.Equal("Frank Herbert", result.Name);
    }

    [Fact]
    public async Task Handle_WithAMissingAuthor_ReturnsNull()
    {
        _authorRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Author?)null);

        var result = await _handler.Handle(new GetAuthorByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_PassesTheRequestedIdAndTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var id = Guid.NewGuid();
        _authorRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Author?)null);

        await _handler.Handle(new GetAuthorByIdQuery(id), cts.Token);

        await _authorRepository.Received(1).GetByIdAsync(id, cts.Token);
    }
}
