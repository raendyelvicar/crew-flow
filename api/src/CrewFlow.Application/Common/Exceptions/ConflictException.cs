namespace CrewFlow.Application.Common.Exceptions;

// Thrown for business-rule conflicts: class at capacity (falls back to waitlist so this
// is only used for genuine conflicts), duplicate booking, no credits remaining, etc.
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
