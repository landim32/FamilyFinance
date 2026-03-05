using FamilyFinance.Models;

namespace FamilyFinance.Services;

public interface IPersonRepository
{
    Task<List<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(int id);
    Task<int> SaveAsync(Person entity);
    Task<int> DeleteAsync(int id);
    Task<int> GetAccountCountAsync(int personId);
}
