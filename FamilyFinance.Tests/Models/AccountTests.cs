using FamilyFinance.Models;

namespace FamilyFinance.Tests.Models;

public class AccountTests
{
    [Fact]
    public void Validate_EmptyTitle_ReturnsError()
    {
        var account = new Account { Title = "", Amount = 100 };
        Assert.NotNull(account.Validate());
    }

    [Fact]
    public void Validate_ZeroAmount_ReturnsError()
    {
        var account = new Account { Title = "Test", Amount = 0 };
        Assert.NotNull(account.Validate());
    }

    [Fact]
    public void Validate_NegativeAmount_ReturnsError()
    {
        var account = new Account { Title = "Test", Amount = -10 };
        Assert.NotNull(account.Validate());
    }

    [Fact]
    public void Validate_Valid_ReturnsNull()
    {
        var account = new Account { Title = "Test", Amount = 100 };
        Assert.Null(account.Validate());
    }

    [Fact]
    public void Create_Valid_ReturnsAccount()
    {
        var account = Account.Create("Groceries", 50.00m, false, "Weekly shopping");
        Assert.Equal("Groceries", account.Title);
        Assert.Equal(50.00m, account.Amount);
        Assert.False(account.IsCredit);
        Assert.Equal("Weekly shopping", account.Notes);
    }

    [Fact]
    public void Create_InvalidTitle_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Account.Create("", 100, true));
    }

    [Fact]
    public void Create_InvalidAmount_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Account.Create("Test", 0, true));
    }

    [Fact]
    public void Update_ChangesFieldsAndUpdatedAt()
    {
        var account = Account.Create("Test", 100, true);
        var originalUpdatedAt = account.UpdatedAt;

        System.Threading.Thread.Sleep(10);
        account.Update("Updated", 200, false, "notes", 1, 2);

        Assert.Equal("Updated", account.Title);
        Assert.Equal(200, account.Amount);
        Assert.False(account.IsCredit);
        Assert.Equal("notes", account.Notes);
        Assert.Equal(1, account.PersonId);
        Assert.Equal(2, account.AccountTypeId);
        Assert.True(account.UpdatedAt > originalUpdatedAt);
    }
}
