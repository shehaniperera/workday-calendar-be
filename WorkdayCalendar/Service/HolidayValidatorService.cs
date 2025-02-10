using WorkdayCalendar.IRepository;
using WorkdayCalendar.IService;
using WorkdayCalendar.Models;

namespace WorkdayCalendar.Service
{
    public class HolidayValidatorService : IHolidayValidatorService
    {
        private readonly IHolidayRepository _holidayRepository;

        public HolidayValidatorService(IHolidayRepository holidayRepository)
        {
            _holidayRepository = holidayRepository;
        }

        public async Task<bool> HolidayExistsAsync(Holiday holiday)
        {

            if (holiday == null || string.IsNullOrWhiteSpace(holiday.Name))
            {
                return false;
            }
            var existingHoliday = await _holidayRepository.GetByDateNameAsync(holiday.Date, holiday.Name);
            return existingHoliday != null;
        }
    }
}
