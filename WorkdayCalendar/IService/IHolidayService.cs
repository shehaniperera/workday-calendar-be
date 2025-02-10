using Microsoft.AspNetCore.Mvc;
using WorkdayCalendar.Models;

namespace WorkdayCalendar.IService
{
    public interface IHolidayService
    {

        Task<bool> AddHolidayAsync(Holiday holiday);
        Task<IEnumerable<Holiday>> GetAllHolidaysAsync();
        Task<Holiday> GetHolidayByIdAsync(Guid id);
        Task<bool> UpdateHolidayAsync(Holiday holiday);
        Task<bool> DeleteHolidayAsync(Guid id);
        Task<IEnumerable<Holiday>> GetFixedHolidaysAsync();
        Task<IEnumerable<Holiday>> GetRecurringHolidaysAsync();

    }
}
