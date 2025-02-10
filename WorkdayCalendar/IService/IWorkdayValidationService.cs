using WorkdayCalendar.Models;

namespace WorkdayCalendar.IService
{
    public interface IWorkdayValidationService
    {
        Task<bool> ValidateWorkdayRequest(WorkdayCalculation request, out List<string> validationErrors);
    }
}
