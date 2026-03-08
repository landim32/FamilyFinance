using FamilyFinance.Data;
using FamilyFinance.Models;

namespace FamilyFinance.Repository;

public class AccountTypeRepository : IAccountTypeRepository
{
    private readonly AppDatabase _database;

    public AccountTypeRepository(AppDatabase database) => _database = database;

    public async Task<List<AccountType>> GetAllAsync()
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<AccountType>().OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<AccountType?> GetByIdAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<AccountType>().Where(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveAsync(AccountType entity)
    {
        var db = await _database.GetConnectionAsync();
        return entity.Id != 0
            ? await db.UpdateAsync(entity)
            : await db.InsertAsync(entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        var db = await _database.GetConnectionAsync();
        return await db.DeleteAsync<AccountType>(id);
    }

    public async Task<int> GetAccountCountAsync(int accountTypeId)
    {
        var db = await _database.GetConnectionAsync();
        return await db.Table<Account>()
            .Where(a => a.AccountTypeId == accountTypeId)
            .CountAsync();
    }
}
