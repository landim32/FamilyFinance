using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;

namespace FamilyFinance.Tests.Services;

public class AccountTypeRepositoryTests : IAsyncLifetime
{
    private AppDatabase _database = null!;
    private AccountTypeRepository _repository = null!;
    private string _dbPath = null!;

    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db3");
        _database = new AppDatabase(_dbPath);
        await _database.InitializeAsync();
        _repository = new AccountTypeRepository(_database);
    }

    public async Task DisposeAsync()
    {
        await _database.CloseAsync();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
    }

    [Fact]
    public async Task SaveAsync_Insert_ReturnsOne()
    {
        var result = await _repository.SaveAsync(AccountType.Create("Food"));
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsInsertedItems()
    {
        await _repository.SaveAsync(AccountType.Create("Food"));
        await _repository.SaveAsync(AccountType.Create("Transport"));

        var all = await _repository.GetAllAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        await _repository.SaveAsync(AccountType.Create("Food"));
        var all = await _repository.GetAllAsync();

        await _repository.DeleteAsync(all[0].Id);

        var result = await _repository.GetByIdAsync(all[0].Id);
        Assert.Null(result);
    }
}
