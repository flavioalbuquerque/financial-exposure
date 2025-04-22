namespace OrderAccumulator.Exceptions;

public class BusinessValidationException : Exception
{
    public string? FieldName { get; }

    public BusinessValidationException(string message, string? fieldName) : base(message)
    {
        FieldName = fieldName;
    }
}
