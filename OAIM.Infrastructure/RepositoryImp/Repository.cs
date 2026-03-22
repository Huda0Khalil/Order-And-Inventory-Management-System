using Microsoft.EntityFrameworkCore;
using OAIM.Domain.Interfaces;
using OAIM.Infrastructure.Data;
using System.Linq.Expressions;

namespace OAIM.Infrastructure.RepositoryImp
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context ) 
        {
            _context = context;
        }
        public async Task<T> AddAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity ;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await GetByIdAsync(id);

            if (entity == null)
                return false;

            _context.Set<T>().Remove(entity);
            
            return await _context.SaveChangesAsync() > 0; 
        }

        public async Task<bool> DeleteAll()
        {
            var entities = await _context.Set<T>().ToListAsync();

            if (!entities.Any())
                return false;

            _context.Set<T>().RemoveRange(entities);          
            return await _context.SaveChangesAsync() > 0;
        }

        //public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        //{
        //    IQueryable<T> query = _context.Set<T>();
        //    if (includes != null)
        //    {
        //        foreach (var include in includes)
        //        {
        //            query = query.Include(include);
        //        }
        //    }

        //    return await query.ToListAsync();
        //}
        public IQueryable<T> GetAll()
        {
            return _context.Set<T>();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> Update(T entity)
        {
            var existingEntity = await GetByIdAsync(entity.Id);

            if (existingEntity == null)
                throw new KeyNotFoundException($"{typeof(T).Name} with Id {entity.Id} was not found.");

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);

            await _context.SaveChangesAsync();

            return existingEntity;
        }
        public IEnumerable<T> FindAll(Func<T, bool> predicate, string[]? includes)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query.Where(predicate).ToList();
        }
        public T Find(Func<T, bool> predicate, params Expression<Func<T, Object>>[]? includes)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query.FirstOrDefault(predicate);
        }
    }
}
