namespace WorkdayCalendar.IRepository
{
    public interface IGenericRepository<T> 
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
        Task<bool> Add(T entity);
        Task<bool> Update(T entity);
        Task<bool> DeleteAsync(Guid id);
    }
}
