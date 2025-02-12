using Microsoft.AspNetCore.Mvc;

namespace WorkdayCalendar.IService
{
    public interface IErrorHandlingService
    {
        ActionResult HandleException(Exception ex);
    }
}
