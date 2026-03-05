namespace FamilyFinance.Models.Migration;

public class PersonSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool HasPhoto { get; set; }
}
