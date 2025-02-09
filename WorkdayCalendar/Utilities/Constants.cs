using System.Runtime.Serialization;

namespace WorkdayCalendar.Utilities
{
    public static class Constants
    {

        public const string ErrorCodeString = "ErrorCode: {0} - {1}";
        public const string ExceptionDataMsg = "{0} \nException Data: Message- {1};InnerException- {2}; StackTrace- {3}";

        public static class ExceptionMessages
        {
            public const string CalculationException = "Exception occurred while calculating the work days";
            public const string InternalServerError = "Internal Server Error occurred";
            public const string WorkdayCalculationError = "Error occurred while calculating the workday";
        }

        public enum ErrorCodes
        {
            [EnumMember(Value = "An unexpected error has occurred. Please, try again or contact the administrator.")]
            InternalServerError = 126,
        }
    }
}
