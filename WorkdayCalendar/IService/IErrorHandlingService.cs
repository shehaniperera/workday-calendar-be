using Microsoft.AspNetCore.Mvc;

namespace WorkdayCalendar.IService
{
    public interface IErrorHandlingService
    {
        Task<ActionResult> HandleExceptionAsync(Exception ex);
    }
}
