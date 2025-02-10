using Microsoft.AspNetCore.Mvc;
using WorkdayCalendar.IService;
using WorkdayCalendar.Models;
using WorkdayCalendar.Utilities; 

namespace WorkdayCalendar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkdayController : ControllerBase
    {
        private readonly IWorkdayCalculationService _workdayService;
        private readonly ILogger<WorkdayController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWorkdayValidationService _validationService;
        private readonly IErrorHandlingService _handleExceptionAsync;

        // WorkdayService ,  ILogger , Validation service , Error Handling service
        public WorkdayController(IWorkdayCalculationService workdayService, ILogger<WorkdayController> logger, IConfiguration configuration, IWorkdayValidationService validationService, IErrorHandlingService handleExceptionAsync)
        {
            _workdayService = workdayService;
            _logger = logger;
            _configuration = configuration;
            _validationService = validationService;
            _handleExceptionAsync = handleExceptionAsync;
        }


        [HttpPost("CalculateWorkDay")]
        public async Task<IActionResult> CalculateWorkday([FromBody] WorkdayCalculation request)
        {

            var valid = await _validationService.ValidateWorkdayRequest(request, out var validationErrors);

            // Validate the request
            if (!valid)
            {
                return BadRequest(validationErrors); // Return validation errors
            }

            try
            {
                // calculate workday
                var result = await _workdayService.CalculateWorkday(request);

                if (result == null)
                {
                    return StatusCode(500, Constants.ExceptionMessages.WorkdayCalculationError);
                }

                var dateFormat = _configuration.GetValue<string>("DateFormat");
                return Ok(new { Result = result?.ToString(dateFormat) });
            }
            catch (Exception ex)
            {
                return await _handleExceptionAsync.HandleExceptionAsync(ex);
            }
        }
    }
}
