﻿using WorkdayCalendar.Models;
using WorkdayCalendar.IService;

namespace WorkdayCalendar.Service
{
    public class WorkdayService : IWorkdayService
    {
        private readonly IConfiguration _configuration;

        public WorkdayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<DateTime?> CalculateWorkday(WorkdayCalculation request)
        {
            DateTime resultDateTime = request.StartDateTime;
            List<Holiday> holidays = request.Holidays ?? new List<Holiday>();

            try
            {

                if (request.WorkingHours == null)
                {
                    request.WorkingHours = new WorkingHours();
                }

                if (request.WorkingHours.Start == TimeSpan.Zero)  // Check if Start time is 00:00:00
                {
                    var workHoursStartString = _configuration.GetValue<string>("WorkdaySettings:WorkingHours:Start");
                    request.WorkingHours.Start = TimeSpan.Parse(workHoursStartString);  // Assign default Start
                }

                if (request.WorkingHours.End == TimeSpan.Zero) // Check if Start end is 00:00:00
                {
                    var workHoursEndString = _configuration.GetValue<string>("WorkdaySettings:WorkingHours:End");
                    request.WorkingHours.End = TimeSpan.Parse(workHoursEndString);  // Assign default End
                }

                // Special case handling for the two specific scenarios
                if (resultDateTime.TimeOfDay < request?.WorkingHours?.Start)
                {
                    // Adjust time to the start of the working day
                    resultDateTime = resultDateTime.Date + request.WorkingHours.Start;
                }
                else if (resultDateTime.TimeOfDay >= request?.WorkingHours?.End)
                {
                    // Move to the next working day if the current time is after working hours
                    resultDateTime = MoveToNextWorkingDay(resultDateTime, holidays, request.WorkingHours.Start);
                }

                double remainingDays = request.WorkingDays;
                Console.WriteLine($"Initial Date: {resultDateTime}, Remaining Days: {remainingDays}");

                // Handle adding or subtracting working days
                while (Math.Abs(remainingDays) > 0.01) // Prevent infinite loops
                {
                    Console.WriteLine($"Checking Date: {resultDateTime}, Remaining Days: {remainingDays}");

                    try
                    {
                        if (IsWorkingDay(resultDateTime, holidays))
                        {
                            if (remainingDays > 0) // Adding working days
                            {
                                double remainingWorkHours = (request.WorkingHours.End - resultDateTime.TimeOfDay).TotalHours;
                                double workingHours = Math.Min(remainingWorkHours, remainingDays * 8); // 8 hours per working day

                                resultDateTime = resultDateTime.AddHours(workingHours);
                                remainingDays -= workingHours / 8; // Subtracting full days
                                Console.WriteLine($"Added {workingHours} hours, New Date: {resultDateTime}, Remaining Days: {remainingDays}");
                            }
                            else if (remainingDays < 0) // Subtracting working days
                            {
                                double remainingWorkHours = (resultDateTime.TimeOfDay - request.WorkingHours.Start).TotalHours;

                                if (remainingWorkHours < 0)
                                {
                                    // If starting before working hours, move to previous working day's end time
                                    resultDateTime = MoveToPreviousWorkingDay(resultDateTime, holidays, request.WorkingHours.Start, request.WorkingHours.End);
                                    remainingWorkHours = (resultDateTime.TimeOfDay - request.WorkingHours.Start).TotalHours;
                                }

                                double workingHours = Math.Min(remainingWorkHours, -remainingDays * 8);
                                resultDateTime = resultDateTime.AddHours(-workingHours);
                                remainingDays += workingHours / 8; // Increase remaining days

                                if (remainingWorkHours <= 0)
                                {
                                    resultDateTime = MoveToPreviousWorkingDay(resultDateTime, holidays, request.WorkingHours.Start, request.WorkingHours.End);
                                }
                            }

                        }
                        else
                        {
                            Console.WriteLine("Not a working day. Skipping to next day.");
                        }

                        // If adding working days and current time is beyond working hours, move to the next working day
                        if (resultDateTime.TimeOfDay >= request.WorkingHours.End && remainingDays > 0)
                        {
                            resultDateTime = MoveToNextWorkingDay(resultDateTime, holidays, request.WorkingHours.Start);
                            Console.WriteLine($"Moved to next working day: {resultDateTime}");
                        }
                        else if (resultDateTime.TimeOfDay < request.WorkingHours.Start && remainingDays < 0)
                        {
                            // Move to the previous working day when subtracting
                            resultDateTime = MoveToPreviousWorkingDay(resultDateTime, holidays, request.WorkingHours.Start, request.WorkingHours.End);
                            Console.WriteLine($"Moved to previous working day: {resultDateTime}");
                        }

                        // Exit condition: If remainingDays is essentially zero or very small
                        if (Math.Abs(remainingDays) < 0.1) // Adjusting precision to exit the loop
                        {
                            Console.WriteLine("Remaining days are small enough to exit the loop.");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during day calculation: {ex.Message}");
                        return DateTime.MinValue; // Return invalid date to indicate error
                    }
                }

                Console.WriteLine($"Final Date: {resultDateTime}");
                return resultDateTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return DateTime.MinValue; // Return an invalid date to signal error
            }
        }

        // Function to move to the next working day
        public DateTime MoveToNextWorkingDay(DateTime currentDateTime, List<Holiday> holidays, TimeSpan workingStartTime)
        {
            try
            {
                DateTime nextDay = currentDateTime.AddDays(1).Date + workingStartTime;

                // Skip weekends and holidays
                while (!IsWorkingDay(nextDay, holidays))
                {
                    nextDay = nextDay.AddDays(1);
                }

                return nextDay;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving to next working day: {ex.Message}");
                return DateTime.MinValue; // Return invalid date to indicate error
            }
        }

        // Function to move to the previous working day
        public DateTime MoveToPreviousWorkingDay(DateTime currentDateTime, List<Holiday> holidays, TimeSpan workingStartTime, TimeSpan workingEndTime)
        {
            try
            {
                DateTime previousDay = currentDateTime.AddDays(-1).Date + workingEndTime;

                // Skip non-working days
                while (!IsWorkingDay(previousDay, holidays))
                {
                    previousDay = previousDay.AddDays(-1);
                }

                // Ensure the correct offset for subtracting hours
                return previousDay.Date + workingEndTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving to previous working day: {ex.Message}");
                return DateTime.MinValue;
            }
        }

        // Check if the given date is a working day, excluding weekends and holidays
        public bool IsWorkingDay(DateTime date, List<Holiday> holidays)
        {
            try
            {
                var weekendDays = _configuration.GetSection("WorkdaySettings:WeekendDays").Get<List<int>>();

                // Exclude weekends (Saturday and Sunday)
                if (weekendDays.Contains((int)date.DayOfWeek))
                {
                    return false; // It's a weekend day, so not a working day
                }

                // Exclude holidays
                foreach (var holiday in holidays)
                {
                    if (holiday.IsRecurring && date.Month == holiday.Date.Month && date.Day == holiday.Date.Day)
                    {
                        return false;
                    }
                    if (!holiday.IsRecurring && date.Date == holiday.Date.Date)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking working day: {ex.Message}");
                return false; // Return false if an error occurs during the check
            }
        }
    }
}
