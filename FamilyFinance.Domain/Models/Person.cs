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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // --- Domain Logic ---

    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) return "Name is required.";
        if (Name.Length > 200) return "Name cannot exceed 200 characters.";
        if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains('@')) return "Invalid email format.";
        return null;
    }

    public void Update(string name, string? phone, string? email, string? photoBase64)
    {
        Name = name;
        Phone = phone;
        Email = email;
        PhotoBase64 = photoBase64;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Person Create(string name, string? phone = null, string? email = null, string? photoBase64 = null)
    {
        var entity = new Person
        {
            Name = name,
            Phone = phone,
            Email = email,
            PhotoBase64 = photoBase64
        };
        var error = entity.Validate();
        if (error != null) throw new InvalidOperationException(error);
        return entity;
    }
}
