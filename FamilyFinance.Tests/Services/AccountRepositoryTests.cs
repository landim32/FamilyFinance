using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;

namespace FamilyFinance.Tests.Services;

public class AccountRepositoryTests : IAsyncLifetime
{
    private AppDatabase _database = null!;
    private AccountRepository _repository = null!;
    private string _dbPath = null!;

    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db3");
        _database = new AppDatabase(_dbPath);
        await _database.InitializeAsync();
        _repository = new AccountRepository(_database);
    }

    public async Task DisposeAsync()
    {
        await _database.CloseAsync();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
    }

    [Fact]
    public async Task SaveAsync_Insert_ReturnsOne()
    {
        var result = await _repository.SaveAsync(Account.Create("Test", 100, true));
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsInsertedItems()
    {
        await _repository.SaveAsync(Account.Create("Account 1", 100, true));
        await _repository.SaveAsync(Account.Create("Account 2", 200, false));

        var all = await _repository.GetAllAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetByPersonAsync_FiltersCorrectly()
    {
        await _repository.SaveAsync(Account.Create("A1", 100, true, personId: 1));
        await _repository.SaveAsync(Account.Create("A2", 200, false, personId: 2));
        await _repository.SaveAsync(Account.Create("A3", 300, true, personId: 1));

        var person1Accounts = await _repository.GetByPersonAsync(1);
        Assert.Equal(2, person1Accounts.Count);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAccount()
    {
        await _repository.SaveAsync(Account.Create("Test", 100, true));
        var all = await _repository.GetAllAsync();

        await _repository.DeleteAsync(all[0].Id);

        var result = await _repository.GetByIdAsync(all[0].Id);
        Assert.Null(result);
    }
}
