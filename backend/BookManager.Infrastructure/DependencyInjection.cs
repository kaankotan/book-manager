using BookManager.Application.Repositories.Authors;
using BookManager.Application.Repositories.BookEvents;
using BookManager.Application.Repositories.Books;
using BookManager.Application.Repositories.BookViews;
using BookManager.Infrastructure.Events;
using BookManager.Infrastructure.Persistence;
using BookManager.Infrastructure.Persistence.Interceptors;
using BookManager.Infrastructure.Repositories.Authors;
using BookManager.Infrastructure.Repositories.BookEvents;
using BookManager.Infrastructure.Repositories.Books;
using BookManager.Infrastructure.Repositories.BookViews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<BookEventInterceptor>();

        services.AddDbContext<AppDbContext>(
            (serviceProvider, options) =>
                options.UseNpgsql(connectionString).AddInterceptors(serviceProvider.GetRequiredService<BookEventInterceptor>())
        );

        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IBookEventRepository, BookEventRepository>();
        services.AddScoped<IBookViewRepository, BookViewRepository>();

        services.AddHostedService<BookEventDispatcher>();

        return services;
    }
}
