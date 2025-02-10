using WorkdayCalendar.IService;
using WorkdayCalendar.Models;
namespace WorkdayCalendar.Service
{
    public class WorkdayValidationService : IWorkdayValidationService
    {
        public Task<bool> ValidateWorkdayRequest(WorkdayCalculation request, out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            // Validate StartDateTime
            if (request.StartDateTime == default)
            {
                validationErrors.Add("StartDateTime must be a valid DateTime.");
            }
            else if (request.StartDateTime > DateTime.Now)
            {
                validationErrors.Add("StartDateTime cannot be in the future.");
            }

          
            // Validate Holidays (can be an empty array, so handle accordingly)
            if (request.Holidays == null)
            {
                validationErrors.Add("Holidays list cannot be null.");
            }
            else
            {
                foreach (var holiday in request.Holidays)
                {
                    if (holiday.Date == default)
                    {
                        validationErrors.Add("Holiday Date must be a valid Date.");
                    }
                }
            }

            // Validate WorkingHours
            try
            {
                var workingStart = request?.WorkingHours?.Start;
                var workingEnd = request?.WorkingHours?.End;

                if (workingEnd <= workingStart)
                {
                    validationErrors.Add("Working End time must be after Start time.");
                }
            }
            catch (FormatException)
            {
                validationErrors.Add("WorkingHours Start and End must be valid times.");
            }


            // valid request or not
            return Task.FromResult(validationErrors.Count == 0);
        }
    }
}
