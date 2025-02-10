using WorkdayCalendar.Models;

namespace WorkdayCalendar.IService
{
    public interface IDateModificationService
    {
        // moving to the next working day
        Task<DateTime> MoveToNextWorkingDay(DateTime currentDateTime, List<Holiday> holidays, TimeSpan workStart);

        // moving to the previous working day
        Task<DateTime> MoveToPreviousWorkingDay(DateTime currentDateTime, List<Holiday> holidays, TimeSpan workStart, TimeSpan workEnd);

        // date is a working day
        bool IsWorkingDay(DateTime date, List<Holiday> holidays);
    }
}
