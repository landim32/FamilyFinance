using FamilyFinance.Data;
using FamilyFinance.Models;

namespace FamilyFinance.Services;

public class PersonRepository : IPersonRepository
{
    private readonly AppDatabase _database;

    public PersonRepository(AppDatabase database) => _database = database;

    public async Task<List<Person>> GetAllAsync() =>
        await _database.Connection.Table<Person>().OrderBy(e => e.Name).ToListAsync();

    public async Task<Person?> GetByIdAsync(int id) =>
        await _database.Connection.Table<Person>().Where(e => e.Id == id).FirstOrDefaultAsync();

    public async Task<int> SaveAsync(Person entity) =>
        entity.Id != 0
            ? await _database.Connection.UpdateAsync(entity)
            : await _database.Connection.InsertAsync(entity);

    public async Task<int> DeleteAsync(int id)
    {
        // Unlink associated accounts
        var linkedAccounts = await _database.Connection.Table<Account>()
            .Where(a => a.PersonId == id)
            .ToListAsync();

        foreach (var account in linkedAccounts)
        {
            account.PersonId = null;
            await _database.Connection.UpdateAsync(account);
        }

        return await _database.Connection.DeleteAsync<Person>(id);
    }

    public async Task<int> GetAccountCountAsync(int personId) =>
        await _database.Connection.Table<Account>()
            .Where(a => a.PersonId == personId)
            .CountAsync();
}
