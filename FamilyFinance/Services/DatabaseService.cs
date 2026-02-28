using FamilyFinance.Models;
using SQLite;

namespace FamilyFinance.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "familyfinance.db");
        _database = new SQLiteAsyncConnection(dbPath);
        await InitAsync();
        return _database;
    }

    public async Task InitAsync()
    {
        if (_database is null)
            return;

        await _database.CreateTableAsync<AccountType>();
        await _database.CreateTableAsync<Person>();
        await _database.CreateTableAsync<Account>();
    }

    // ---- Account ----

    public async Task<List<Account>> GetAccountsAsync()
    {
        var db = await GetDatabaseAsync();
        var accounts = await db.Table<Account>().ToListAsync();
        await PopulateAccountRelationsAsync(accounts);
        return accounts;
    }

    public async Task<List<Account>> GetAccountsByPersonAsync(int personId)
    {
        var db = await GetDatabaseAsync();
        var accounts = await db.Table<Account>()
            .Where(a => a.PersonId == personId)
            .ToListAsync();
        await PopulateAccountRelationsAsync(accounts);
        return accounts;
    }

    public async Task<int> SaveAccountAsync(Account account)
    {
        var db = await GetDatabaseAsync();
        if (account.Id != 0)
            return await db.UpdateAsync(account);
        else
            return await db.InsertAsync(account);
    }

    public async Task<int> DeleteAccountAsync(Account account)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(account);
    }

    // ---- Person ----

    public async Task<List<Person>> GetPeopleAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Person>().ToListAsync();
    }

    public async Task<Person?> GetPersonByIdAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Person>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SavePersonAsync(Person person)
    {
        var db = await GetDatabaseAsync();
        if (person.Id != 0)
            return await db.UpdateAsync(person);
        else
            return await db.InsertAsync(person);
    }

    public async Task<int> DeletePersonAsync(Person person)
    {
        var db = await GetDatabaseAsync();

        // Set PersonId to null on all linked accounts
        var linkedAccounts = await db.Table<Account>()
            .Where(a => a.PersonId == person.Id)
            .ToListAsync();

        foreach (var account in linkedAccounts)
        {
            account.PersonId = null;
            await db.UpdateAsync(account);
        }

        return await db.DeleteAsync(person);
    }

    // ---- AccountType ----

    public async Task<List<AccountType>> GetAccountTypesAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<AccountType>().ToListAsync();
    }

    public async Task<int> SaveAccountTypeAsync(AccountType accountType)
    {
        var db = await GetDatabaseAsync();
        if (accountType.Id != 0)
            return await db.UpdateAsync(accountType);
        else
            return await db.InsertAsync(accountType);
    }

    public async Task<int> DeleteAccountTypeAsync(AccountType accountType)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(accountType);
    }

    public async Task<int> GetAccountCountByTypeAsync(int accountTypeId)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Account>()
            .Where(a => a.AccountTypeId == accountTypeId)
            .CountAsync();
    }

    public async Task<int> GetAccountCountByPersonAsync(int personId)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Account>()
            .Where(a => a.PersonId == personId)
            .CountAsync();
    }

    // ---- Helpers ----

    private async Task PopulateAccountRelationsAsync(List<Account> accounts)
    {
        var db = await GetDatabaseAsync();
        var people = await db.Table<Person>().ToListAsync();
        var types = await db.Table<AccountType>().ToListAsync();

        var peopleLookup = people.ToDictionary(p => p.Id);
        var typesLookup = types.ToDictionary(t => t.Id);

        foreach (var account in accounts)
        {
            if (account.PersonId.HasValue && peopleLookup.TryGetValue(account.PersonId.Value, out var person))
                account.Person = person;

            if (account.AccountTypeId.HasValue && typesLookup.TryGetValue(account.AccountTypeId.Value, out var type))
                account.AccountType = type;
        }
    }
}
