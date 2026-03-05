using AutoMapper;
using FamilyFinance.DTOs;
using FamilyFinance.Mappers;
using FamilyFinance.Models;

namespace FamilyFinance.Tests.Mappers;

public class MapperTests
{
    private readonly IMapper _mapper;

    public MapperTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PersonProfile>();
            cfg.AddProfile<AccountTypeProfile>();
            cfg.AddProfile<AccountProfile>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void MapperConfiguration_IsValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PersonProfile>();
            cfg.AddProfile<AccountTypeProfile>();
            cfg.AddProfile<AccountProfile>();
        });
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_PersonToPersonInfo()
    {
        var person = Person.Create("John", "555-1234", "john@test.com");
        var info = _mapper.Map<PersonInfo>(person);

        Assert.Equal(person.Name, info.Name);
        Assert.Equal(person.Phone, info.Phone);
        Assert.Equal(person.Email, info.Email);
        Assert.Equal(person.CreatedAt, info.CreatedAt);
    }

    [Fact]
    public void Map_PersonInfoToPerson_IgnoresTimestamps()
    {
        var info = new PersonInfo
        {
            Id = 1,
            Name = "John",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var person = _mapper.Map<Person>(info);

        Assert.Equal(info.Id, person.Id);
        Assert.Equal(info.Name, person.Name);
        // CreatedAt and UpdatedAt should be ignored (default values)
        Assert.NotEqual(info.CreatedAt, person.CreatedAt);
    }

    [Fact]
    public void Map_AccountTypeToAccountTypeInfo()
    {
        var type = AccountType.Create("Food", "Food expenses");
        var info = _mapper.Map<AccountTypeInfo>(type);

        Assert.Equal(type.Name, info.Name);
        Assert.Equal(type.Description, info.Description);
    }

    [Fact]
    public void Map_AccountToAccountInfo()
    {
        var account = Account.Create("Test", 100, true, "notes", 1, 2);
        var info = _mapper.Map<AccountInfo>(account);

        Assert.Equal(account.Title, info.Title);
        Assert.Equal(account.Amount, info.Amount);
        Assert.Equal(account.IsCredit, info.IsCredit);
        Assert.Equal(account.Notes, info.Notes);
        Assert.Equal(account.PersonId, info.PersonId);
        Assert.Equal(account.AccountTypeId, info.AccountTypeId);
    }
}
