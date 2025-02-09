using WorkdayCalendar.IRepository;
using WorkdayCalendar.Models;

namespace WorkdayCalendar.IRepository
{
    public interface IHolidayRepository : IGenericRepository<Holiday>
    {
        Task<IEnumerable<Holiday>> GetFixedHolidaysAsync();
        Task<IEnumerable<Holiday>> GetRecurringHolidaysAsync();
    }
}
