using System.Text.Json;
using FamilyFinance.Models;
using FamilyFinance.Models.Migration;

namespace FamilyFinance.Services;

public class MigrationService
{
    private readonly DatabaseService _db;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public MigrationService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<string> GenerateJsonForPersonAsync(int personId)
    {
        var person = await _db.GetPersonByIdAsync(personId);
        if (person is null)
            throw new InvalidOperationException($"Person with Id {personId} not found.");

        var accounts = await _db.GetAccountsByPersonAsync(personId);

        var migrationAccounts = accounts.Select(a => new AccountMigration
        {
            Id = a.Id,
            Title = a.Title,
            Amount = a.Amount,
            IsCredit = !a.IsCredit, // INVERTED
            AccountType = a.AccountType?.Name,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt
        }).ToList();

        var totalCredit = migrationAccounts.Where(a => a.IsCredit).Sum(a => a.Amount);
        var totalDebit = migrationAccounts.Where(a => !a.IsCredit).Sum(a => a.Amount);

        var migration = new MigrationPerson
        {
            Version = "1.0",
            GeneratedAt = DateTime.Now,
            Person = new PersonSummary
            {
                Id = person.Id,
                Name = person.Name,
                Phone = person.Phone,
                Email = person.Email,
                HasPhoto = !string.IsNullOrEmpty(person.PhotoBase64)
            },
            Summary = new FinancialSummary
            {
                TotalAccounts = migrationAccounts.Count,
                TotalCredit = totalCredit,
                TotalDebit = totalDebit
            },
            Accounts = migrationAccounts
        };

        return JsonSerializer.Serialize(migration, JsonOptions);
    }

    public async Task<string> ExportJsonForPersonAsync(int personId)
    {
        var person = await _db.GetPersonByIdAsync(personId);
        if (person is null)
            throw new InvalidOperationException($"Person with Id {personId} not found.");

        var json = await GenerateJsonForPersonAsync(personId);
        var sanitizedName = string.Concat(person.Name.Where(char.IsLetterOrDigit));
        var fileName = $"migration_{sanitizedName}_{person.Id}.json";
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        await File.WriteAllTextAsync(filePath, json);
        return filePath;
    }

    public async Task<List<string>> ExportAllAsync()
    {
        var people = await _db.GetPeopleAsync();
        var paths = new List<string>();

        foreach (var person in people)
        {
            var path = await ExportJsonForPersonAsync(person.Id);
            paths.Add(path);
        }

        return paths;
    }
}
