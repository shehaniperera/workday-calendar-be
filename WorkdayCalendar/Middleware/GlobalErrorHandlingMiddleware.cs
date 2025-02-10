using global::WorkdayCalendar.Exceptions;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace WorkdayCalendar.Middleware
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

        public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException notFoundEx)
            {
                _logger.LogError($"NotFoundException: {notFoundEx.Message}");
                await HandleExceptionAsync(context, notFoundEx, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException argEx)
            {
                _logger.LogError($"ArgumentNullException: {argEx.Message}");
                await HandleExceptionAsync(context, argEx, HttpStatusCode.BadRequest);
            }
            catch (NullReferenceException nullEx)
            {
                _logger.LogError($"NullReferenceException: {nullEx.Message}");
                await HandleExceptionAsync(context, nullEx, HttpStatusCode.BadRequest);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database Update Exception: {dbEx.Message}");
                await HandleExceptionAsync(context, dbEx, HttpStatusCode.InternalServerError);
            }
            catch (RepositoryException repoEx)
            {
                _logger.LogError($"Repository Exception: {repoEx.Message}");
                await HandleExceptionAsync(context, repoEx, HttpStatusCode.InternalServerError);
            }
            catch (ServiceException serviceEx)
            {
                _logger.LogError($"Service Exception: {serviceEx.Message}");
                await HandleExceptionAsync(context, serviceEx, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled exception: {ex.Message}");
                await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var errorResponse = new
            {
                message = exception.Message,
                details = exception.StackTrace
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
