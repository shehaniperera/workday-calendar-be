using WorkdayCalendar.IRepository;
using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Data;

namespace WorkdayCalendar.Repository
{
    /// <summary>
    /// Class implementation for generic repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {

        protected WorkdayCalendarDBContext context;
        internal DbSet<T> dbSet;

        public GenericRepository(WorkdayCalendarDBContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();

        }

        public virtual async Task<bool> Add(T entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await dbSet.FindAsync(id);
            if (entity == null)
            {
                return false; // Entity not found
            }

            dbSet.Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task<T> Update(T entity)
        {
            if (entity == null)
            {
                return null; // Return null if entity is invalid
            }

            dbSet.Update(entity);
            await context.SaveChangesAsync();

            return entity; // Return the updated entity

        }
    }
}
