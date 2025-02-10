using WorkdayCalendar.Models;

namespace WorkdayCalendar.IService
{
    public interface IHolidayValidatorService
    {
        Task<bool> HolidayExistsAsync(Holiday holiday);
    }
}
