using FamilyFinance.Models;
using SQLite;

namespace FamilyFinance.Data;

public class AppDatabase
{
    private readonly SQLiteAsyncConnection _database;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public SQLiteAsyncConnection Connection => _database;

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        await InitializeAsync();
        return _database;
    }

    public AppDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;
            await _database.CreateTableAsync<AccountType>();
            await _database.CreateTableAsync<Person>();
            await _database.CreateTableAsync<Account>();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task CloseAsync()
    {
        await _database.CloseAsync();
    }
}
