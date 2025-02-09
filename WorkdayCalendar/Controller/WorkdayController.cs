using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkdayCalendar.Models;
using WorkdayCalendar.Service;
using WorkdayCalendar.Utilities; 

namespace WorkdayCalendar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkdayController : ControllerBase
    {
        private readonly WorkdayService _workdayService;
        private readonly ILogger<WorkdayController> _logger;
        private readonly IConfiguration _configuration;

        // Dependency Injection for WorkdayService and ILogger
        public WorkdayController(WorkdayService workdayService, ILogger<WorkdayController> logger, IConfiguration configuration)
        {
            _workdayService = workdayService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("CalculateWorkDay")]
        public async Task<IActionResult> CalculateWorkday([FromBody] WorkdayCalculation request)
        {
            // Check if the request is null
            if (request == null)
                return BadRequest("Invalid request.");

            // Check for validation errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            try
            {
                // Invoke the service to calculate the result
                var result = await _workdayService.CalculateWorkday(request);

                if (result == null)
                {
                    // In case the result is null
                    return StatusCode(500, Constants.ExceptionMessages.WorkdayCalculationError);
                }

                // Get the date format from appsettings.json
                var dateFormat = _configuration.GetValue<string>("DateFormat");
                // Return the result as a formatted string
                return Ok(new { Result = result?.ToString(dateFormat) });
            }
            catch (Exception ex)
            {
                // Log the exception details for troubleshooting
                _logger.LogError($"{Constants.ExceptionMessages.CalculationException}: {ex.Message}, StackTrace: {ex.StackTrace}");

                // Return a 500 status code with a detailed error message
                string errorText = $"Error Code: {(int)Constants.ErrorCodes.InternalServerError}, Message: {Constants.ExceptionMessages.InternalServerError}.";
                return StatusCode(500, errorText);
            }
        }
    }
}
