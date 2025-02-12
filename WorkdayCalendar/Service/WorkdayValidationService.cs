using WorkdayCalendar.IService;
using WorkdayCalendar.Models;
using WorkdayCalendar.Utilities;
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
                validationErrors.Add(Constants.ValidationMessages.ValidStartDate);
            }
            else if (request.StartDateTime > DateTime.Now)
            {
                validationErrors.Add(Constants.ValidationMessages.FutureStart);
            }

            // Validate Holidays
            if (request.Holidays == null)
            {
                validationErrors.Add(Constants.ValidationMessages.HolidayList);
            }
            else
            {
                foreach (var holiday in request.Holidays)
                {
                    if (holiday.Date == default)
                    {
                        validationErrors.Add(Constants.ValidationMessages.ValidHoliday);
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
                    validationErrors.Add(Constants.ValidationMessages.WorkEndTime);
                }
            }
            catch (FormatException)
            {
                validationErrors.Add(Constants.ValidationMessages.ValidWorkTimes);
            }


            // valid request or not
            return Task.FromResult(validationErrors.Count == 0);
        }
    }
}
