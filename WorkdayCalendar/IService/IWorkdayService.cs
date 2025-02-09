using WorkdayCalendar.Models;

namespace WorkdayCalendar.IService
{
    public interface IWorkdayService
    {
        Task<DateTime?> CalculateWorkday(WorkdayCalculation workdayCalculation);
        DateTime MoveToNextWorkingDay(DateTime currentDateTime, List<Holiday> holidays, TimeSpan workingStartTime);
        DateTime MoveToPreviousWorkingDay(DateTime currentDateTime, List<Holiday> holidays, TimeSpan workingStartTime, TimeSpan workingEndTime);
        bool IsWorkingDay(DateTime date, List<Holiday> holidays);
    }
}
