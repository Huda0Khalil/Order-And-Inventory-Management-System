using System.Linq.Expressions;

namespace OAIM.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        Task<T> GetByIdAsync(int id);
        //Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includess);
        Task<T> AddAsync(T entity);
        Task<T> Update(T entity);
        Task<bool> Delete(int id);
        Task<bool> DeleteAll();
        IEnumerable<T> FindAll(Func<T, bool> predicate, string[]? includes);
        T Find(Func<T, bool> predicate, params Expression<Func<T, Object>>[]? includes);
    }
}
