
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
                return BadRequest(Constants.ExceptionMessages.InvalidRequest);

            // Check for validation errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            try
            {
                var result = await _holidayRepository.Add(holiday);

                if (result)
                {
                    // Return success message
                    return Ok(new { message = Constants.SucccessMessages.AddHolidaySucessMessage });
                }
                else
                {
                    return BadRequest();
                }

               
            }
            catch (Exception ex)
            {

                // Log the exception details
                _logger.LogError($"{Constants.ExceptionMessages.ExceptionError}: {ex.Message}, StackTrace: {ex.StackTrace}");

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

                // Check if the result is null or empty
                if (result == null || !result.Any())
                {
                    // Return 204 No Content if no holidays are found
                    return NoContent();  // No holidays, but the request was successful
                }

                // Return result
                return Ok(new
                {
                    Result = result,
                    Count = result.Count()  // Return the count of holidays
                });
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError($"{Constants.ExceptionMessages.ExceptionError}: {ex.Message}, StackTrace: {ex.StackTrace}");

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

                // Check if the result is null or empty
                if (result == null)
                {
                    // Return 204 No Content if no holidays are found
                    return NoContent();  // No holidays, but the request was successful
                }

                // Return result
                return Ok(new { Result = result });
            }
            catch (Exception ex)
            {

                // Log the exception details
                _logger.LogError($"{Constants.ExceptionMessages.ExceptionError}: {ex.Message}, StackTrace: {ex.StackTrace}");

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
                return BadRequest(Constants.ExceptionMessages.InvalidRequest);

            // Check for validation errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            try
            {
                await _holidayRepository.Update(holiday);

                // Return success message
                return Ok(new { message = Constants.SucccessMessages.UpdateHolidaySucessMessage });
            }
            catch (Exception ex)
            {
                // Log the exception details for troubleshooting
                _logger.LogError($"{Constants.ExceptionMessages.ExceptionError}: {ex.Message}, StackTrace: {ex.StackTrace}");

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

                // Check if result is null or empty
                if (result == null || !result.Any())
                {
                    // Return 404 Not Found if no recurring holidays exist
                    return NotFound(new { message = Constants.ExceptionMessages.RecurringHolidayRetrievalError });
                }

                // Return result
                return Ok(new { Result = result });
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError($"{Constants.ExceptionMessages.ExceptionError}: {ex.Message}, StackTrace: {ex.StackTrace}");

                // Return a 500 status code with a detailed error message
                string errorText = $"Error Code: {(int)Constants.ErrorCodes.InternalServerError}, Message: {Constants.ExceptionMessages.InternalServerError}.";
                return StatusCode(500, errorText);

            }
        }



        [HttpGet("GetFixedHolidays")]
        public async Task<ActionResult> GetFixedHolidays()
        {

            try
            {

                var result = await _holidayRepository.GetFixedHolidaysAsync();

                // Check if result is null or empty
                if (result == null || !result.Any())
                {
                    // Return 404 Not Found if no recurring holidays exist
                    return NotFound(new { message = Constants.ExceptionMessages.FixedHolidayRetrievalError });
                }

                // Return result
                return Ok(new { Result = result });
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError($"{Constants.ExceptionMessages.ExceptionError}: {ex.Message}, StackTrace: {ex.StackTrace}");

                // Return a 500 status code with a detailed error message
                string errorText = $"Error Code: {(int)Constants.ErrorCodes.InternalServerError}, Message: {Constants.ExceptionMessages.InternalServerError}.";
                return StatusCode(500, errorText);

            }
        }


        [HttpDelete("DeleteHoliday")]
        public async Task<ActionResult> DeleteHoliday(Guid id)
        {

            try
            {
                var result = await _holidayRepository.DeleteAsync(id);

                if (result)
                {
                    // Return success message
                    return Ok(new { message = Constants.SucccessMessages.DeleteHolidaySucessMessage });
                }
                else
                {
                    return BadRequest();
                }

               
            }
            catch (Exception ex)
            {
                // Log the exception details for troubleshooting
                _logger.LogError($"{Constants.ExceptionMessages.ExceptionError}: {ex.Message}, StackTrace: {ex.StackTrace}");

                // Return a 500 status code with a detailed error message
                string errorText = $"Error Code: {(int)Constants.ErrorCodes.InternalServerError}, Message: {Constants.ExceptionMessages.InternalServerError}.";
                return StatusCode(500, errorText);

            }
        }

    }
}
