using FamilyFinance.Data;
using FamilyFinance.Models;

namespace FamilyFinance.Repository;

public class PersonRepository : IPersonRepository
{
    private readonly AppDatabase _database;

    public PersonRepository(AppDatabase database) => _database = database;

    public async Task<List<Person>> GetAllAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Person>().OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Person>().Where(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveAsync(Person entity)
    {
        var db = await _database.GetConnectionAsync();
        return entity.Id != 0
            ? await db.UpdateAsync(entity)
            : await db.InsertAsync(entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        var db = await _database.GetConnectionAsync();

        // Unlink associated accounts
        var linkedAccounts = await db.Table<Account>()
            .Where(a => a.PersonId == id)
            .ToListAsync();

        foreach (var account in linkedAccounts)
        {
            account.PersonId = null;
            await db.UpdateAsync(account);
        }

        return await db.DeleteAsync<Person>(id);
    }

    public async Task<int> GetAccountCountAsync(int personId)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Account>()
            .Where(a => a.PersonId == personId)
            .CountAsync();
    }
}
