namespace Journal.Domain.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string resource, Exception? innerEx = null) : base($"{resource} could not found", innerEx)
    {
    }
}