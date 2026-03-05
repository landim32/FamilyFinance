using FamilyFinance.Data;
using FamilyFinance.Models;

namespace FamilyFinance.Services;

public class AccountRepository : IAccountRepository
{
    private readonly AppDatabase _database;

    public AccountRepository(AppDatabase database) => _database = database;

    public async Task<List<Account>> GetAllAsync() =>
        await _database.Connection.Table<Account>().OrderByDescending(e => e.CreatedAt).ToListAsync();

    public async Task<Account?> GetByIdAsync(int id) =>
        await _database.Connection.Table<Account>().Where(e => e.Id == id).FirstOrDefaultAsync();

    public async Task<List<Account>> GetByPersonAsync(int personId) =>
        await _database.Connection.Table<Account>()
            .Where(a => a.PersonId == personId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<int> SaveAsync(Account entity) =>
        entity.Id != 0
            ? await _database.Connection.UpdateAsync(entity)
            : await _database.Connection.InsertAsync(entity);

    public async Task<int> DeleteAsync(int id) =>
        await _database.Connection.DeleteAsync<Account>(id);
}
