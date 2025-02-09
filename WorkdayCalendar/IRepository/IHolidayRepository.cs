using WorkdayCalendar.IRepository;
using WorkdayCalendar.Models;

namespace WorkdayCalendar.IRepository
{
    public interface IHolidayRepository : IGenericRepository<Holiday>
    {
        Task<IEnumerable<Holiday>> GetHolidaysByDateAsync(DateTime date);
        Task<IEnumerable<Holiday>> GetFixedHolidaysAsync();
        Task<IEnumerable<Holiday>> GetRecurringHolidaysAsync();
    }
}
