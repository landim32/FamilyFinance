using AutoMapper;
using FamilyFinance.DTOs;
using FamilyFinance.Models;

namespace FamilyFinance.Mappers;

public class PersonProfile : Profile
{
    public PersonProfile()
    {
        CreateMap<Person, PersonInfo>();
        CreateMap<PersonInfo, Person>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
