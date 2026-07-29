namespace CrewFlow.Application.Common.Exceptions;

// Distinct from FluentValidation's own pipeline errors - used for validation failures
// surfaced by external systems we wrap (e.g. ASP.NET Identity's password/user rules).
public class ValidationAppException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationAppException(IEnumerable<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors.ToList();
    }
}
