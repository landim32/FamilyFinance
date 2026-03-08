using FamilyFinance.Models;

namespace FamilyFinance.Repository;

public interface IAccountTypeRepository
{
    Task<List<AccountType>> GetAllAsync();
    Task<AccountType?> GetByIdAsync(int id);
    Task<int> SaveAsync(AccountType entity);
    Task<int> DeleteAsync(int id);
    Task<int> GetAccountCountAsync(int accountTypeId);
}
