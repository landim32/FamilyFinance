namespace FamilyFinance.Services;

public interface IMigrationService
{
    Task<string> GenerateJsonForPersonAsync(int personId);
    Task<string> ExportJsonForPersonAsync(int personId);
    Task<List<string>> ExportAllAsync();
}
