namespace FamilyFinance.Models.Migration;

public class FinancialSummary
{
    public int TotalAccounts { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal NetBalance => TotalCredit - TotalDebit;
}
