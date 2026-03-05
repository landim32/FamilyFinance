using FamilyFinance.Models;
using SQLite;

namespace FamilyFinance.Data;

public class AppDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public SQLiteAsyncConnection Connection => _database;

    public AppDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await _database.CreateTableAsync<AccountType>();
        await _database.CreateTableAsync<Person>();
        await _database.CreateTableAsync<Account>();
    }

    public async Task CloseAsync()
    {
        await _database.CloseAsync();
    }
}
