using WorkdayCalendar.Models;
using WorkdayCalendar.IService;
using WorkdayCalendar.Utilities;

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

                request.WorkingHours ??= new WorkingHours();

                // Set default times, get from consigs
                request.WorkingHours.Start = SetDefaultWorkingTime(request.WorkingHours.Start, "WorkdaySettings:WorkingHours:Start");
                request.WorkingHours.End = SetDefaultWorkingTime(request.WorkingHours.End, "WorkdaySettings:WorkingHours:End");

                // adjust working hours
                resultDateTime = AdjustToWorkingHours(resultDateTime, request.WorkingHours.Start, request.WorkingHours.End, holidays);

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
                                (resultDateTime, remainingDays) = AddWorkingDays(resultDateTime, holidays, remainingDays, request.WorkingHours);
                            }
                            else if (remainingDays < 0) 
                            {
                                (resultDateTime, remainingDays) = SubtractWorkingDays(resultDateTime, holidays, remainingDays, request.WorkingHours);
                            }
                        }
                        else
                        {
                            _logger.LogInformation(Constants.ValidationMessages.WorkdaySkip);
                        }

                        // If adding working days and the current time is above working hours, move to the next working day
                        if (resultDateTime.TimeOfDay >= request.WorkingHours.End && remainingDays > 0)
                        {
                            resultDateTime =  _dateModificationService.MoveToNextWorkingDay(resultDateTime, holidays, request.WorkingHours.Start);
                        }
                        else if (resultDateTime.TimeOfDay < request.WorkingHours.Start && remainingDays < 0)
                        {
                            // If subtracting working days and time is before working hours, move to the previous working day
                            resultDateTime = _dateModificationService.MoveToPreviousWorkingDay(resultDateTime, holidays, request.WorkingHours.Start, request.WorkingHours.End);
                        }

                        if (Math.Abs(remainingDays) < 0.1) 
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, Constants.ExceptionMessages.WorkdayCalculationError);
                        return DateTime.MinValue; // Return invalid date  error
                    }
                }

                return resultDateTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Constants.ExceptionMessages.UnexpectedWorkdayCalculationError);
                return DateTime.MinValue; // Return an invalid date to  error
            }
        }

        private TimeSpan SetDefaultWorkingTime(TimeSpan currentTime, string configKey)
        {
            if (currentTime == TimeSpan.Zero)
            {
                var defaultTimeString = _configuration.GetValue<string>(configKey);
                return TimeSpan.Parse(defaultTimeString);
            }
            return currentTime;
        }


        private DateTime AdjustToWorkingHours(DateTime dateTime, TimeSpan workStart, TimeSpan workEnd, List<Holiday> holidays)
        {
            //  set the time to the start (current time - earlier than the start of workday)
            if (dateTime.TimeOfDay < workStart)
            {
                return dateTime.Date + workStart;
            }

            // if working hours, move to the next work day (current time - later than or equal to the end of workday)
            else if (dateTime.TimeOfDay >= workEnd)
            {
                return _dateModificationService.MoveToNextWorkingDay(dateTime, holidays, workStart);
            }

            return dateTime;
        }

        private (DateTime, double) AddWorkingDays(DateTime dateTime, List<Holiday> holidays, double remainingDays, WorkingHours workingHours)
        {
            // remaining work hours for the current day
            double remainingWorkHours = (workingHours.End - dateTime.TimeOfDay).TotalHours;
            double workingHoursToAdd = Math.Min(remainingWorkHours, remainingDays * 8);

            // Add the calculated working hours
            dateTime = dateTime.AddHours(workingHoursToAdd);

            // Reduce remainingDays based on added work hours
            remainingDays -= workingHoursToAdd / 8;

            return (dateTime, remainingDays);
        }

        private (DateTime, double) SubtractWorkingDays(DateTime dateTime, List<Holiday> holidays, double remainingDays, WorkingHours workingHours)
        {
            // remaining work hours from the start of current day
            double remainingWorkHours = (dateTime.TimeOfDay - workingHours.Start).TotalHours;

            // If remainingWorkHours is negative - move to the previous work day
            if (remainingWorkHours < 0)
            {
                dateTime = _dateModificationService.MoveToPreviousWorkingDay(dateTime, holidays, workingHours.Start, workingHours.End);
                remainingWorkHours = (dateTime.TimeOfDay - workingHours.Start).TotalHours;
            }

            //hours to subtract - based on remaining days
            double substractWorkingHours = Math.Min(remainingWorkHours, -remainingDays * 8);
            dateTime = dateTime.AddHours(-substractWorkingHours);
            remainingDays += substractWorkingHours / 8; // Add full days to remaining days

            // no work hours are left - current day, move to previous work day
            if (remainingWorkHours <= 0)
            {
                dateTime = _dateModificationService.MoveToPreviousWorkingDay(dateTime, holidays, workingHours.Start, workingHours.End);
            }

            return (dateTime, remainingDays);
        }



    }
}
