using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Data;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.Models;
public class HolidayRepository : GenericRepository<Holiday>, IHolidayRepository
{
    // Inject DBContext
    public HolidayRepository(WorkdayCalendarDBContext context)
        : base(context)
    {
    }

    // Get holiday by date and name
    public async Task<Holiday> GetByDateNameAsync(DateTime date, string name)
    {
        return await dbSet.AsNoTracking()
                          .Where(h => h.Date == date && h.Name.ToLower().Trim().Equals(name.ToLower().Trim()))
                          .SingleOrDefaultAsync();
    }


    // Get all fixed holidays
    public async Task<IEnumerable<Holiday>> GetFixedHolidaysAsync()
    {
        return await dbSet
            .AsNoTracking()
            .Where(h => !h.IsRecurring)
            .ToListAsync();
    }

    // Get all recurring holidays
    public async Task<IEnumerable<Holiday>> GetRecurringHolidaysAsync()
    {
        return await dbSet
            .AsNoTracking()
            .Where(h => h.IsRecurring)
            .ToListAsync();
    }
}
