namespace FamilyFinance.Models.Migration;

public class AccountMigration
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsCredit { get; set; }
    public string? AccountType { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
