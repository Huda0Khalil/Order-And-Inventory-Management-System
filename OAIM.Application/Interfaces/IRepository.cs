using System.Linq.Expressions;
using System.Security.Cryptography;

namespace OAIM.Domain.Interfaces
{
    public interface IRepository<T,TKey> where T : IEntity<TKey>
    {
        IQueryable<T> GetAll();
        //Task<T> GetByIdAsync(int id);
        //Task<T> GetByIdAsync(Guid id);
        Task<T> GetByIdAsync(Object id);


        //Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includess);
        Task<T> AddAsync(T entity);
        Task<T> Update(T entity);
        Task<bool> Delete(int id);
        Task<bool> Delete(T entity);

        Task<bool> DeleteAll();
        IEnumerable<T> FindAll(Func<T, bool> predicate, string[]? includes);
        //T Find(Func<T, bool> predicate, params Expression<Func<T, Object>>[]? includes);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, Object>>[]? includes);
    }
}
