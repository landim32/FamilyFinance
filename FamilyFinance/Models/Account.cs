using SQLite;

namespace FamilyFinance.Models;

[Table("Account")]
public class Account
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Title { get; set; } = string.Empty;

    [NotNull]
    public decimal Amount { get; set; }

    public bool IsCredit { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? PersonId { get; set; }

    public int? AccountTypeId { get; set; }

    [Ignore]
    public Person? Person { get; set; }

    [Ignore]
    public AccountType? AccountType { get; set; }
}
