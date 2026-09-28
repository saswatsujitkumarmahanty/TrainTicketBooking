namespace Domain.Models;


public class BusinessRuleException(string message, int errorNumber) : Exception(message)
{
    public int ErrorNumber { get; } = errorNumber;
}