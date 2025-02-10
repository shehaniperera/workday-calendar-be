using Microsoft.AspNetCore.Mvc;
using WorkdayCalendar.Utilities;
using System.Net;
using WorkdayCalendar.IService;
using WorkdayCalendar.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;


namespace WorkdayCalendar.Services
{
    
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger;
        }

        public async Task<ActionResult> HandleExceptionAsync(Exception ex)
        {
            // log exceptions
            _logger.LogError($"{Constants.ExceptionMessages.ExceptionError}: {ex.Message}, StackTrace: {ex.StackTrace}");

            // specific exceptions
            if (ex is ArgumentException)
            {
                return new BadRequestObjectResult(new { message = "Invalid input provided" });
            }

            if (ex is NotFoundException notFoundException)
            {
                // Not Found errors
                return new NotFoundObjectResult(new { message = notFoundException.Message });
            }

            // general errors
            string errorText = $"Error Code: {(int)Constants.ErrorCodes.InternalServerError}, Message: {Constants.ExceptionMessages.InternalServerError}.";
            return new ObjectResult(new { message = errorText })
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
