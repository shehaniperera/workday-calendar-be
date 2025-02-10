namespace WorkdayCalendar.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }

    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class RepositoryException : Exception
    {
        public RepositoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class ServiceException : Exception
    {
        public ServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}
