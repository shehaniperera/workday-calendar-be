using WorkdayCalendar.IRepository;
using WorkdayCalendar.Models;
using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Data;

namespace WorkdayCalendar.Repository
{
    public class HolidayRepository : GenericRepository<Holiday>, IHolidayRepository

    {

        public HolidayRepository(WorkdayCalendarDBContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Holiday>> GetAllAsync()
        {
            return await dbSet.OrderBy(h => h.Date).ToListAsync();
        }

        public async Task<IEnumerable<Holiday>> GetFixedHolidaysAsync()
        {
            try
            {
                var fixedHolidays = await dbSet
                  .Where(h => !h.IsRecurring)
                  .ToListAsync();

                return fixedHolidays;
            }
            catch (Exception ex)
            {

                throw ex; // Rethrow or handle the error as needed
            }
        }

        public async Task<IEnumerable<Holiday>> GetRecurringHolidaysAsync()
        {
            try
            {
                var recurringHolidays = await dbSet
                  .Where(h => h.IsRecurring)
                  .ToListAsync();

                return recurringHolidays;
            }
            catch (Exception ex)
            {
         
                throw ex; // Rethrow or handle the error as needed
            }
        }

    }
}
