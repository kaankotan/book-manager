using AutoMapper;
using BookManager.Domain.Entities;

namespace BookManager.Application.Books.Dtos;

public class BookMappingProfile : Profile
{
    public BookMappingProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<Author, AuthorDto>();
    }
}
