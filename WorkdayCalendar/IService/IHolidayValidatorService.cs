using System.ComponentModel.DataAnnotations;
using WorkdayCalendar.Models;

namespace WorkdayCalendar.IService
{
    public interface IHolidayValidatorService
    {
        Task<bool> HolidayExistsAsync(Holiday holiday);

        bool IsEmptyHoliday(Holiday holiday);

        bool IsValidHoliday(Holiday holiday, out string errorMessage);
    }
}
