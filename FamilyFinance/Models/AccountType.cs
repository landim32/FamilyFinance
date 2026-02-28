using SQLite;

namespace FamilyFinance.Models;

[Table("AccountType")]
public class AccountType
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
