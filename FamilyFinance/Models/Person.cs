using SQLite;

namespace FamilyFinance.Models;

[Table("Person")]
public class Person
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? PhotoBase64 { get; set; }
}
