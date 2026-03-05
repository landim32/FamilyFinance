using FamilyFinance.Data;
using FamilyFinance.Models;
using FamilyFinance.Services;

namespace FamilyFinance.Tests.Services;

public class PersonRepositoryTests : IAsyncLifetime
{
    private AppDatabase _database = null!;
    private PersonRepository _repository = null!;
    private string _dbPath = null!;

    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db3");
        _database = new AppDatabase(_dbPath);
        await _database.InitializeAsync();
        _repository = new PersonRepository(_database);
    }

    public async Task DisposeAsync()
    {
        await _database.CloseAsync();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
    }

    [Fact]
    public async Task SaveAsync_Insert_ReturnsOne()
    {
        var result = await _repository.SaveAsync(Person.Create("Test"));
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsInsertedItems()
    {
        await _repository.SaveAsync(Person.Create("Alice"));
        await _repository.SaveAsync(Person.Create("Bob"));

        var all = await _repository.GetAllAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectPerson()
    {
        await _repository.SaveAsync(Person.Create("Alice"));
        var all = await _repository.GetAllAsync();
        var person = await _repository.GetByIdAsync(all[0].Id);

        Assert.NotNull(person);
        Assert.Equal("Alice", person.Name);
    }

    [Fact]
    public async Task SaveAsync_Update_ModifiesExisting()
    {
        await _repository.SaveAsync(Person.Create("Alice"));
        var all = await _repository.GetAllAsync();
        var person = all[0];

        person.Update("Alice Updated", null, null, null);
        await _repository.SaveAsync(person);

        var updated = await _repository.GetByIdAsync(person.Id);
        Assert.Equal("Alice Updated", updated!.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPerson()
    {
        await _repository.SaveAsync(Person.Create("Alice"));
        var all = await _repository.GetAllAsync();

        await _repository.DeleteAsync(all[0].Id);

        var result = await _repository.GetByIdAsync(all[0].Id);
        Assert.Null(result);
    }
}
