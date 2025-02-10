using WorkdayCalendar.Models;

namespace WorkdayCalendar.IService
{
    public interface IWorkdayCalculationService
    {
        Task<DateTime?> CalculateWorkday(WorkdayCalculation request);
    }
}
