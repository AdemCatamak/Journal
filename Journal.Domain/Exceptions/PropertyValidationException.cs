namespace Journal.Domain.Exceptions;

public class PropertyValidationException : CustomException
{
    public PropertyValidationException(string propertyName, string message)
        : base($"{propertyName} is not valid. Details : {message}")
    {
    }
}