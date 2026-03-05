using FamilyFinance.Models;

namespace FamilyFinance.Services;

public interface IAccountRepository
{
    Task<List<Account>> GetAllAsync();
    Task<Account?> GetByIdAsync(int id);
    Task<List<Account>> GetByPersonAsync(int personId);
    Task<int> SaveAsync(Account entity);
    Task<int> DeleteAsync(int id);
}
