using WorkdayCalendar.Exceptions;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.IService;
using WorkdayCalendar.Models;
using Microsoft.AspNetCore.Mvc;
using WorkdayCalendar.Utilities;

namespace WorkdayCalendar.Service
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _holidayRepository;
        private readonly IHolidayValidatorService _validatorService;

        // Injecting repo
        public HolidayService(IHolidayRepository holidayRepository, IHolidayValidatorService validatorService)
        {
            _holidayRepository = holidayRepository;
            _validatorService = validatorService;
        }

        // Add a new holiday
        public async Task<bool> AddHolidayAsync(Holiday holiday)
        {
            if (holiday == null)
            {
                throw new ArgumentNullException(nameof(holiday), Constants.ExceptionMessages.HolidayNullError);
            }

            if (_validatorService.IsEmptyHoliday(holiday))
            {
                throw new ArgumentNullException(nameof(holiday), Constants.ExceptionMessages.HolidayNullError);
            }

            if (await _validatorService.HolidayExistsAsync(holiday))
            {
                return false;  // Holiday exists
            }

            return await _holidayRepository.Add(holiday);
        }

        // Delete a holiday by its Id
        public async Task<bool> DeleteHolidayAsync(Guid id)
        {
            var result = await _holidayRepository.DeleteAsync(id);
            if (!result)
            {
                throw new NotFoundException(Constants.ExceptionMessages.HolidayNotFoundError);
            }
            return result;
        }

        // Get all holidays
        public async Task<IEnumerable<Holiday>> GetAllHolidaysAsync()
        {
            var holidays = await _holidayRepository.GetAllAsync();
            if (holidays == null || !holidays.Any())
            {
                throw new NotFoundException(Constants.ExceptionMessages.HolidayNotFoundError);
            }
            return holidays;
        }

        // Get holiday by Id
        public async Task<Holiday> GetHolidayByIdAsync(Guid id)
        {
            var holiday = await _holidayRepository.GetByIdAsync(id);
            if (holiday == null)
            {
                throw new NotFoundException(Constants.ExceptionMessages.HolidayNotFoundError);
            }
            return holiday;
        }

        // Get recurring holidays
        public async Task<IEnumerable<Holiday>> GetRecurringHolidaysAsync()
        {
            var result = await _holidayRepository.GetRecurringHolidaysAsync();
            if (result == null || !result.Any())
            {
                throw new NotFoundException(Constants.ExceptionMessages.RecurringHolidayRetrievalError);
            }
            return result;
        }

        // Get fixed holidays
        public async Task<IEnumerable<Holiday>> GetFixedHolidaysAsync()
        {
            var result = await _holidayRepository.GetFixedHolidaysAsync();
            if (result == null || !result.Any())
            {
                throw new NotFoundException(Constants.ExceptionMessages.FixedHolidayRetrievalError);
            }
            return result;
        }

        // Update an existing holiday
        public async Task<bool> UpdateHolidayAsync(Holiday holiday)
        {
            if (holiday == null)
            {
                throw new ArgumentNullException(nameof(holiday), Constants.ExceptionMessages.HolidayNullError);
            }

            var result = await _holidayRepository.Update(holiday);
            if (!result)
            {
                throw new NotFoundException(Constants.ExceptionMessages.HolidayNotFoundError);
            }
            return result;
        }
    }
}
