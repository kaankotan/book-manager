using AutoMapper;
using BookManager.Application;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookManager.Tests.Common;

/// <summary>
/// Builds a real <see cref="IMapper"/> from the application's profiles. Mapping is pure in-memory
/// behaviour, so handler tests assert on real DTO values instead of a stubbed mapper.
/// </summary>
public static class TestMapper
{
    public static MapperConfiguration Configuration { get; } =
        new(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly), NullLoggerFactory.Instance);

    public static IMapper Create() => Configuration.CreateMapper();
}
