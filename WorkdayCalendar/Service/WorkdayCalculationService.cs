using WorkdayCalendar.Models;
using WorkdayCalendar.IService;
using WorkdayCalendar.Utilities;
using System;

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
                            resultDateTime = ProcessWorkingDay(resultDateTime, holidays, ref remainingDays, request.WorkingHours);
                        }
                        else
                        {
                            _logger.LogInformation(Constants.ValidationMessages.WorkdaySkip);
                        }

                        resultDateTime = MoveToNextOrPreviousWorkingDay(resultDateTime, remainingDays, request.WorkingHours, holidays);

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
                return SetTimeToStartOfWorkday(dateTime, workStart);
            }

            // if working hours, move to the next work day (current time - later than or equal to the end of workday)
            else if (dateTime.TimeOfDay >= workEnd)
            {
                return MoveToNextWorkingDay(dateTime, holidays, workStart);
            }

            return dateTime;
        }

        // set the time to the start (current time - earlier than the start of workday)
        private DateTime SetTimeToStartOfWorkday(DateTime dateTime, TimeSpan workStart)
        {
            return dateTime.Date + workStart;
        }

        // if working hours, move to the next work day (current time - later than or equal to the end of workday)
        private DateTime MoveToNextWorkingDay(DateTime dateTime, List<Holiday> holidays, TimeSpan workStart)
        {
            return _dateModificationService.MoveToNextWorkingDay(dateTime, holidays, workStart);
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
            double remainingWorkHours = GetRemainingWorkHours(dateTime, workingHours.Start);

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

        // get remaming hours
        private double GetRemainingWorkHours(DateTime dateTime, TimeSpan workStart)
        {
            return (dateTime.TimeOfDay - workStart).TotalHours;
        }

        private DateTime MoveToNextOrPreviousWorkingDay(DateTime currentDateTime, double remainingDays, WorkingHours workingHours, List<Holiday> holidays)
        {
            // If adding working days and the current time is above working hours, move to the next working day
            if (currentDateTime.TimeOfDay >= workingHours.End && remainingDays > 0)
            {
                return _dateModificationService.MoveToNextWorkingDay(currentDateTime, holidays, workingHours.Start);
            }

            // If subtracting working days and time is before working hours, move to the previous working day
            else if (currentDateTime.TimeOfDay < workingHours.Start && remainingDays < 0)
            {
                return _dateModificationService.MoveToPreviousWorkingDay(currentDateTime, holidays, workingHours.Start, workingHours.End);
            }
            return currentDateTime;
        }

        // process work days - add or substract work days
        private DateTime ProcessWorkingDay(DateTime resultDateTime, List<Holiday> holidays, ref double remainingDays, WorkingHours workingHours)
        {
            if (_dateModificationService.IsWorkingDay(resultDateTime, holidays))
            {
                if (remainingDays > 0)
                {
                    (resultDateTime, remainingDays) = AddWorkingDays(resultDateTime, holidays, remainingDays, workingHours);
                }
                else
                {
                    (resultDateTime, remainingDays) = SubtractWorkingDays(resultDateTime, holidays, remainingDays, workingHours);
                }
            }
            else
            {
                _logger.LogInformation(Constants.ValidationMessages.WorkdaySkip);
            }

            return MoveToNextOrPreviousWorkingDay(resultDateTime, remainingDays, workingHours, holidays);
        }
    }
}
