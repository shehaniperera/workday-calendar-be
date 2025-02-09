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
            public const string ExceptionError = "Exception occurred, Please, try again.";
        }

        public enum ErrorCodes
        {
            [EnumMember(Value = "An unexpected error has occurred. Please, try again.")]
            InternalServerError = 126,
        }
    }
}
