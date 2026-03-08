using FamilyFinance.Data;
using FamilyFinance.Models;

namespace FamilyFinance.Repository;

public class AccountRepository : IAccountRepository
{
    private readonly AppDatabase _database;

    public AccountRepository(AppDatabase database) => _database = database;

    public async Task<List<Account>> GetAllAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Account>().OrderByDescending(e => e.CreatedAt).ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Account>().Where(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Account>> GetByPersonAsync(int personId)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Account>()
            .Where(a => a.PersonId == personId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> SaveAsync(Account entity)
    {
        var db = await _database.GetConnectionAsync();
        return entity.Id != 0
            ? await db.UpdateAsync(entity)
            : await db.InsertAsync(entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.DeleteAsync<Account>(id);
    }
}
