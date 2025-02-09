
using Microsoft.AspNetCore.Mvc;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.Models;
using WorkdayCalendar.Utilities;

namespace WorkdayCalendar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayRepository _holidayRepository;
        private readonly ILogger<HolidayController> _logger;

        // Dependency Injection for HolidayService and ILogger
        public HolidayController(IHolidayRepository holidayRepository, ILogger<HolidayController> logger)
        {
            _holidayRepository = holidayRepository;
            _logger = logger;
        }



        [HttpPost("AddHoliday")]
        public async Task<ActionResult> AddHoliday([FromBody] Holiday holiday)
        {
            // Check if the request is null
            if (holiday == null)
                return BadRequest("Invalid request.");

            // Check for validation errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            try
            {
                await _holidayRepository.Add(holiday);
                return Ok(new { message = "Holiday added successfully!" });
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


        [HttpGet("GetHolidays")]
        public async Task<ActionResult> GetAllHolidays()
        {
           
            try
            {
                var result = await _holidayRepository.GetAllAsync();
                return Ok(new
                {
                    Result = result,
                    Count = result.Count()  // Return the count of holidays
                });
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


        [HttpGet("GetHolidaysById")]
        public async Task<ActionResult> GetHolidaysById(Guid id)
        {

            try
            {

                var result = await _holidayRepository.GetByIdAsync(id);
                return Ok(new { Result = result });
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


        [HttpPatch("UpdateHoliday")]
        public async Task<ActionResult> UpdateHoliday([FromBody] Holiday holiday)
        {
            // Check if the request is null
            if (holiday == null)
                return BadRequest("Invalid request.");

            // Check for validation errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            try
            {
                await _holidayRepository.Update(holiday);
                return Ok(new { message = "Holiday updated successfully!" });
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


        [HttpGet("GetRecurringHolidays")]
        public async Task<ActionResult> GetRecurringHolidays()
        {

            try
            {

                var result = await _holidayRepository.GetRecurringHolidaysAsync();
                return Ok(new { Result = result });
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
