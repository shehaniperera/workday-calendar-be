using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Data;
using WorkdayCalendar.IRepository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly WorkdayCalendarDBContext context;
    internal readonly DbSet<T> dbSet;

    public GenericRepository(WorkdayCalendarDBContext context)
    {
        this.context = context;
        this.dbSet = context.Set<T>();
    }

    public virtual async Task<bool> Add(T entity)
    {
        var existingEntity = await dbSet.FirstOrDefaultAsync(e => e.Equals(entity));
        if (existingEntity != null) return false;

        await dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await dbSet.FindAsync(id);
        if (entity == null) return false;

        dbSet.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<T> GetByIdAsync(Guid id)
    {
        return await dbSet.FindAsync(id);
    }

    public virtual async Task<bool> Update(T entity)
    {
        dbSet.Update(entity);
        await context.SaveChangesAsync();
        return true;
    }
}
