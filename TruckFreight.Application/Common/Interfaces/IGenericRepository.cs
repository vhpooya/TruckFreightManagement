// In TruckFreightSystem.Application/Interfaces/Persistence/IGenericRepository.cs
using TruckFreight.Domain.Entities;


namespace TruckFreightSystem.Application.Interfaces.Persistence
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<IReadOnlyList<T>> GetPagedReponseAsync(int pageNumber, int pageSize);
        Task<int> CountAsync();
    }
}