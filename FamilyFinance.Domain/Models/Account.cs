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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Indexed]
    public int? PersonId { get; set; }

    [Indexed]
    public int? AccountTypeId { get; set; }

    // --- Domain Logic ---

    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Title)) return "Title is required.";
        if (Title.Length > 200) return "Title cannot exceed 200 characters.";
        if (Amount <= 0) return "Amount must be greater than zero.";
        return null;
    }

    public void Update(string title, decimal amount, bool isCredit, string? notes, int? personId, int? accountTypeId)
    {
        Title = title;
        Amount = amount;
        IsCredit = isCredit;
        Notes = notes;
        PersonId = personId;
        AccountTypeId = accountTypeId;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Account Create(string title, decimal amount, bool isCredit, string? notes = null, int? personId = null, int? accountTypeId = null)
    {
        var entity = new Account
        {
            Title = title,
            Amount = amount,
            IsCredit = isCredit,
            Notes = notes,
            PersonId = personId,
            AccountTypeId = accountTypeId
        };
        var error = entity.Validate();
        if (error != null) throw new InvalidOperationException(error);
        return entity;
    }
}
