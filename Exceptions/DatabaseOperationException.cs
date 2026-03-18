namespace UserManagementAPI.Exceptions;

public class DatabaseOperationException(string message, Exception innerException)
    : Exception(message, innerException)
{
}
