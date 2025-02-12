using WorkdayCalendar.Models;
using WorkdayCalendar.IService;

namespace WorkdayCalendar.Service
{
    public class DateModificationService : IDateModificationService
    {
        private readonly IConfiguration _configuration;
        private readonly IErrorHandlingService _errorHandlingService;

        public DateModificationService(IConfiguration configuration, IErrorHandlingService errorHandlingService)
        {
            _configuration = configuration;
            _errorHandlingService = errorHandlingService;
        }

        // Next working day
        public DateTime MoveToNextWorkingDay(DateTime currentDateTime, List<Holiday> holidays, TimeSpan workingStartTime)
        {
            try
            {

                DateTime nextDay = currentDateTime.AddDays(1).Date + workingStartTime;

                // Skip weekends and holidays
                while (!IsWorkingDay(nextDay, holidays))
                {
                    nextDay = nextDay.AddDays(1);
                }

                return nextDay;

            }
            catch (Exception ex)
            {
                _errorHandlingService.HandleException(ex);
                throw new InvalidOperationException("Error moving to next working day", ex);
            }
        }

        // Previous working day
        public DateTime MoveToPreviousWorkingDay(DateTime currentDateTime, List<Holiday> holidays, TimeSpan workingStartTime, TimeSpan workingEndTime)
        {
            try
            {
                DateTime previousDay = currentDateTime.AddDays(-1).Date + workingEndTime;

                // Skip non-working days
                while (!IsWorkingDay(previousDay, holidays))
                {
                    previousDay = previousDay.AddDays(-1);
                }

                return previousDay.Date + workingEndTime;
            }
            catch (Exception ex)
            {
                _errorHandlingService.HandleException(ex);
                throw new InvalidOperationException("Error moving to previous working day", ex);
            }
        }

        // given date is a working day, exclude weekends and holidays
        public bool IsWorkingDay(DateTime date, List<Holiday> holidays)
        {
            try
            {
                var weekendDays = _configuration.GetSection("WorkdaySettings:WeekendDays").Get<List<int>>();

                // Exclude weekends (Sat and Sun)
                if (weekendDays.Contains((int)date.DayOfWeek))
                {
                    return false;
                }

                // Exclude holidays
                foreach (var holiday in holidays)
                {
                    if ((holiday.IsRecurring && date.Month == holiday.Date.Month && date.Day == holiday.Date.Day) ||
                        (!holiday.IsRecurring && date.Date == holiday.Date.Date))
                    {
                        return false; // holiday
                    }
                }

                return true; // working day
            }
            catch (Exception ex)
            {
                 _errorHandlingService.HandleException(ex);
                throw new InvalidOperationException("Error checking if date is a working day", ex);
            }
        }
    }
}
