namespace FamilyFinance.DTOs;

public class AccountInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsCredit { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? PersonId { get; set; }
    public int? AccountTypeId { get; set; }
    public string? PersonName { get; set; }
    public string? AccountTypeName { get; set; }
}
