using AutoMapper;
using FamilyFinance.DTOs;
using FamilyFinance.Models;

namespace FamilyFinance.Mappers;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<Account, AccountInfo>()
            .ForMember(dest => dest.PersonName, opt => opt.Ignore())
            .ForMember(dest => dest.AccountTypeName, opt => opt.Ignore());
        CreateMap<AccountInfo, Account>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
