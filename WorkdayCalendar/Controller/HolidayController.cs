using Microsoft.AspNetCore.Mvc;
using WorkdayCalendar.IService;
using WorkdayCalendar.Models;
using WorkdayCalendar.Utilities;

namespace WorkdayCalendar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _holidayService;
        private readonly IHolidayValidatorService _holidayValidatorService;

        // Constructor to inject the HolidayService
        public HolidayController(IHolidayService holidayService, IHolidayValidatorService holidayValidatorService)
        {
            _holidayService = holidayService;
            _holidayValidatorService = holidayValidatorService;
        }

        [HttpPost("AddHoliday")]
        public async Task<ActionResult> AddHoliday([FromBody] Holiday holiday)
        {

            if (!_holidayValidatorService.IsValidHoliday(holiday, out string errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            var result = await _holidayService.AddHolidayAsync(holiday);

            if (result)
            {
                return Ok(new { message = Constants.SucccessMessages.AddHolidaySucessMessage });
            }

            return Conflict(new { message = Constants.ExceptionMessages.HolidayExistsError });
        }

        [HttpGet("GetHolidays")]
        public async Task<ActionResult> GetAllHolidays()
        {
            var result = await _holidayService.GetAllHolidaysAsync();

            if (result == null || !result.Any())
            {
                return NoContent();
            }

            // Return holidays with count
            return Ok(new { Result = result, Count = result.Count() });
        }

        [HttpGet("GetHolidaysById")]
        public async Task<ActionResult> GetHolidaysById(Guid id)
        {
            var result = await _holidayService.GetHolidayByIdAsync(id);
            return Ok(new { Result = result });
        }

        [HttpDelete("DeleteHoliday")]
        public async Task<ActionResult> DeleteHoliday(Guid id)
        {
            var result = await _holidayService.DeleteHolidayAsync(id);

            if (result)
            {
                return Ok(new { message = Constants.SucccessMessages.DeleteHolidaySucessMessage });
            }

            return NotFound(new { message = Constants.ExceptionMessages.HolidayNotFoundError });
        }

        [HttpPatch("UpdateHoliday")]
        public async Task<ActionResult> UpdateHoliday([FromBody] Holiday holiday)
        {
            if (!_holidayValidatorService.IsValidHoliday(holiday, out string errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            var result = await _holidayService.UpdateHolidayAsync(holiday);

            if (result)
            {
                return Ok(new { message = Constants.SucccessMessages.UpdateHolidaySucessMessage });
            }
            else
            {
                return NotFound(new { message = Constants.ExceptionMessages.HolidayNotFoundError });
            }
        }

        [HttpGet("GetRecurringHolidays")]
        public async Task<ActionResult> GetRecurringHolidays()
        {
            var result = await _holidayService.GetRecurringHolidaysAsync();

            if (result == null || !result.Any())
            {
                return NoContent();
            }

            return new OkObjectResult(new { Result = result, Count = result.Count() });
        }

        [HttpGet("GetFixedHolidays")]
        public async Task<ActionResult> GetFixedHolidays()
        {
            var result = await _holidayService.GetFixedHolidaysAsync();
            return new OkObjectResult(new { Result = result, Count = result.Count() });
        }
    }
}
