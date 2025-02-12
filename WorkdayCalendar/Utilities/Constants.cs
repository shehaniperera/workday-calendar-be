using System.Runtime.Serialization;

namespace WorkdayCalendar.Utilities
{
    public static class Constants
    {

        public const string ErrorCodeString = "ErrorCode: {0} - {1}";
        public const string ExceptionDataMsg = "{0} \nException Data: Message- {1};InnerException- {2}; StackTrace- {3}";
        


        public static class SucccessMessages
        {
            public const string AddHolidaySucessMessage = "Holiday added successfully!";
            public const string UpdateHolidaySucessMessage = "Holiday updated successfully!";
            public const string DeleteHolidaySucessMessage = "Holiday deleted successfully!";
        }

        public static class ExceptionMessages
        {
            public const string CalculationException = "Exception occurred while calculating the work days";
            public const string InternalServerError = "Internal Server Error occurred";
            public const string WorkdayCalculationError = "Error occurred while calculating the workday";
            public const string RecurringHolidayRetrievalError = "No recurring holidays found";
            public const string InvalidRequest = "Invalid request";
            public const string FixedHolidayRetrievalError = "No fixed holidays found";
            public const string ExceptionError = "Exception occurred, Please, try again";
            public const string HolidayNotFoundError = "Holiday not found";
            public const string DeleteHolidayError = "An error occurred while deleting holiday";
            public const string AddHolidayError = "An error occurred while adding holiday";
            public const string HolidayNullError = "Holiday cannot be null";
            public const string HolidayExistsError = "Holiday already exists";
            public const string GetHolidayError = "Error occurred while retrieving holiday";
            public const string UpdateHolidayError = "Error occurred while updating holiday";
            public const string FetchException = "An error occurred while retrieving holidays from the database";
            public const string FetchDateNameException = "An error occurred while fetching the holiday by date and name";
            public const string FixedHolidayException = "An error occurred while fetching fixed holidays";
            public const string RecurringHolidayException = "An error occurred while fetching recurring holidays";
            public const string UnexpectedWorkdayCalculationError = "An unexpected error occurred while calculating workday";
            public const string InvalidInputError = "Invalid Input Provided";
        }


        public static class ValidationMessages
        {
            public const string ValidStartDate = "StartDateTime must be a valid DateTime.";
            public const string HolidayIncomplete = "Holiday has incomplete or empty values";
            public const string HolidayDateEmpty = "Holiday date cannot be empty";
            public const string HolidayList = "Holidays list cannot be null.";
            public const string ValidHoliday = "Holiday Date must be a valid Date.";
            public const string WorkEndTime = "Working End time must be after Start time.";
            public const string ValidWorkTimes = "WorkingHours Start and End must be valid times.";
            public const string WorkdaySkip = "Not a working day. Skipping to next day";
            public const string FutureStart = "StartDateTime cannot be in the future.";
            public const string WorkdayCalculationInvalid = "Invalid workday calculation request";
        }

        public static class DBExceptionMessages
        {
            public const string GenericDbException = "An error occurred while retrieving records from the database";
        }

        public enum ErrorCodes
        {
            [EnumMember(Value = "An unexpected error has occurred. Please, try again.")]
            InternalServerError = 126,
        }
    }
}
