using WorkdayCalendar.Models;
using WorkdayCalendar.IService;

namespace WorkdayCalendar.Service
{
    public class WorkdayCalculationService : IWorkdayCalculationService
    {
        private readonly IDateModificationService _dateModificationService;
        private readonly ILogger<WorkdayCalculationService> _logger;
        private readonly IConfiguration _configuration;

        public WorkdayCalculationService(
            IDateModificationService dateModificationService,
            ILogger<WorkdayCalculationService> logger,
            IConfiguration configuration)
        {
            _dateModificationService = dateModificationService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<DateTime?> CalculateWorkday(WorkdayCalculation request)
        {
            DateTime resultDateTime = request.StartDateTime;
            List<Holiday> holidays = request.Holidays ?? new List<Holiday>();

            try
            {
                // If working hours are not defined, get the default values from configs
                if (request.WorkingHours == null)
                {
                    request.WorkingHours = new WorkingHours();
                }

                if (request.WorkingHours.Start == TimeSpan.Zero) // Default Start time
                {
                    var workHoursStartString = _configuration.GetValue<string>("WorkdaySettings:WorkingHours:Start");
                    request.WorkingHours.Start = TimeSpan.Parse(workHoursStartString);
                }

                if (request.WorkingHours.End == TimeSpan.Zero) // Default End time
                {
                    var workHoursEndString = _configuration.GetValue<string>("WorkdaySettings:WorkingHours:End");
                    request.WorkingHours.End = TimeSpan.Parse(workHoursEndString);
                }

                if (resultDateTime.TimeOfDay < request.WorkingHours.Start)
                {
                    resultDateTime = resultDateTime.Date + request.WorkingHours.Start;
                }
                // if working hours, move to the next work day
                else if (resultDateTime.TimeOfDay >= request.WorkingHours.End)
                {
                    resultDateTime = await _dateModificationService.MoveToNextWorkingDay(resultDateTime, holidays, request.WorkingHours.Start);
                }

                double remainingDays = request.WorkingDays;

                while (Math.Abs(remainingDays) > 0.01) 
                {
                    try
                    {
                        if (_dateModificationService.IsWorkingDay(resultDateTime, holidays))
                        {
                            if (remainingDays > 0) // add working days
                            {
                                // Calculate remaining work hours for current day
                                double remainingWorkHours = (request.WorkingHours.End - resultDateTime.TimeOfDay).TotalHours;
                                double workingHours = Math.Min(remainingWorkHours, remainingDays * 8);

                                resultDateTime = resultDateTime.AddHours(workingHours);
                                remainingDays -= workingHours / 8;
                            }
                            else if (remainingDays < 0) 
                            {
                                // Calculate remaining work hours from the start of current day
                                double remainingWorkHours = (resultDateTime.TimeOfDay - request.WorkingHours.Start).TotalHours;

                                if (remainingWorkHours < 0) 
                                {

                                 
                                    resultDateTime = await _dateModificationService.MoveToPreviousWorkingDay(resultDateTime, holidays, request.WorkingHours.Start, request.WorkingHours.End);
                                    remainingWorkHours = (resultDateTime.TimeOfDay - request.WorkingHours.Start).TotalHours;
                                }

                                double workingHours = Math.Min(remainingWorkHours, -remainingDays * 8);
                                resultDateTime = resultDateTime.AddHours(-workingHours);
                                remainingDays += workingHours / 8; // Add full days to remaining days

                                if (remainingWorkHours <= 0)
                                {
                                    resultDateTime = await _dateModificationService.MoveToPreviousWorkingDay(resultDateTime, holidays, request.WorkingHours.Start, request.WorkingHours.End);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Not a working day. Skipping to next day.");
                        }

                        // If adding working days and the current time is above working hours, move to the next working day
                        if (resultDateTime.TimeOfDay >= request.WorkingHours.End && remainingDays > 0)
                        {
                            resultDateTime = await _dateModificationService.MoveToNextWorkingDay(resultDateTime, holidays, request.WorkingHours.Start);
                        }
                        else if (resultDateTime.TimeOfDay < request.WorkingHours.Start && remainingDays < 0)
                        {
                            // If subtracting working days and time is before working hours, move to the previous working day
                            resultDateTime = await _dateModificationService.MoveToPreviousWorkingDay(resultDateTime, holidays, request.WorkingHours.Start, request.WorkingHours.End);
                        }

                        if (Math.Abs(remainingDays) < 0.1) 
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during workday calculation");
                        return DateTime.MinValue; // Return invalid date  error
                    }
                }

                return resultDateTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while calculating workday");
                return DateTime.MinValue; // Return an invalid date to  error
            }
        }
    }
}
