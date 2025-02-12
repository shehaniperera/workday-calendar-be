using WorkdayCalendar.IRepository;
using WorkdayCalendar.IService;
using WorkdayCalendar.Models;
using WorkdayCalendar.Utilities;
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

        public bool IsValidHoliday(Holiday holiday, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Check if holiday is null
            if (holiday == null)
            {
                errorMessage = Constants.ExceptionMessages.HolidayNullError;
                return false;
            }

            else if (IsEmptyHoliday(holiday))
            {
                errorMessage = Constants.ValidationMessages.HolidayIncomplete;
                return false;
            }

            //  holiday date is empty (DateTime.MinValue)
            else if (holiday.Date == DateTime.MinValue)
            {
                errorMessage = Constants.ValidationMessages.HolidayDateEmpty;
                return false;
            }
        
            return true; // valid
        }

        public bool IsEmptyHoliday(Holiday holiday)
        {

            //  if the holiday is empty
            return holiday.Id == Guid.Empty
                && holiday.Date == DateTime.MinValue
                && string.IsNullOrEmpty(holiday.Name);
        }

    }
}
