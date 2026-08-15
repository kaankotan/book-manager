using AutoMapper;
using BookManager.Domain.Entities;

namespace BookManager.Application.Events.Dtos;

public class BookEventMappingProfile : Profile
{
    public BookEventMappingProfile()
    {
        CreateMap<BookEvent, BookEventDto>();
    }
}
