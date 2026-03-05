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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // --- Domain Logic ---

    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) return "Name is required.";
        if (Name.Length > 200) return "Name cannot exceed 200 characters.";
        return null;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public static AccountType Create(string name, string? description = null)
    {
        var entity = new AccountType
        {
            Name = name,
            Description = description
        };
        var error = entity.Validate();
        if (error != null) throw new InvalidOperationException(error);
        return entity;
    }
}
