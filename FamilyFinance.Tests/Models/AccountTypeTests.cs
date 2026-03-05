using FamilyFinance.Models;

namespace FamilyFinance.Tests.Models;

public class AccountTypeTests
{
    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var type = new AccountType { Name = "" };
        Assert.NotNull(type.Validate());
    }

    [Fact]
    public void Validate_ValidName_ReturnsNull()
    {
        var type = new AccountType { Name = "Food" };
        Assert.Null(type.Validate());
    }

    [Fact]
    public void Create_Valid_ReturnsAccountType()
    {
        var type = AccountType.Create("Food", "Food expenses");
        Assert.Equal("Food", type.Name);
        Assert.Equal("Food expenses", type.Description);
    }

    [Fact]
    public void Create_InvalidName_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => AccountType.Create(""));
    }

    [Fact]
    public void Update_ChangesFields()
    {
        var type = AccountType.Create("Food");
        type.Update("Transport", "Transport expenses");

        Assert.Equal("Transport", type.Name);
        Assert.Equal("Transport expenses", type.Description);
    }
}
