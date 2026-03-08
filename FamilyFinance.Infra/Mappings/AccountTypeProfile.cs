using AutoMapper;
using FamilyFinance.DTOs;
using FamilyFinance.Models;

namespace FamilyFinance.Mappings;

public class AccountTypeProfile : Profile
{
    public AccountTypeProfile()
    {
        CreateMap<AccountType, AccountTypeInfo>();
        CreateMap<AccountTypeInfo, AccountType>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
