namespace Journal.Domain.Exceptions;

public abstract class CustomException : Exception
{
    protected CustomException(string message, Exception? innerEx = null)
        : base(message, innerEx)
    {
    }
}