namespace FamilyFinance.Models.Migration;

public class MigrationPerson
{
    public string Version { get; set; } = "1.0";
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public PersonSummary Person { get; set; } = new();
    public FinancialSummary Summary { get; set; } = new();
    public List<AccountMigration> Accounts { get; set; } = new();
}
