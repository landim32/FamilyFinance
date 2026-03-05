using FamilyFinance.Data;
using FamilyFinance.Models;

namespace FamilyFinance.Services;

public class AccountTypeRepository : IAccountTypeRepository
{
    private readonly AppDatabase _database;

    public AccountTypeRepository(AppDatabase database) => _database = database;

    public async Task<List<AccountType>> GetAllAsync() =>
        await _database.Connection.Table<AccountType>().OrderBy(e => e.Name).ToListAsync();

    public async Task<AccountType?> GetByIdAsync(int id) =>
        await _database.Connection.Table<AccountType>().Where(e => e.Id == id).FirstOrDefaultAsync();

    public async Task<int> SaveAsync(AccountType entity) =>
        entity.Id != 0
            ? await _database.Connection.UpdateAsync(entity)
            : await _database.Connection.InsertAsync(entity);

    public async Task<int> DeleteAsync(int id) =>
        await _database.Connection.DeleteAsync<AccountType>(id);

    public async Task<int> GetAccountCountAsync(int accountTypeId) =>
        await _database.Connection.Table<Account>()
            .Where(a => a.AccountTypeId == accountTypeId)
            .CountAsync();
}
