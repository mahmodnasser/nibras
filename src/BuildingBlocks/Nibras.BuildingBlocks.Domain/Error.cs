namespace Nibras.BuildingBlocks.Domain;

/// <summary>
/// A failure with a stable code from the service's error catalog (Appendix K), for example
/// <c>ATTENDANCE_SESSION_LOCKED</c>. The code is the contract; the message is for people.
/// </summary>
public sealed record Error
{
    public Error(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (!IsValidCode(code))
        {
            throw new ArgumentException(
                $"Error code '{code}' must be upper snake case, such as PLATFORM_VALIDATION_FAILED.", nameof(code));
        }

        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }

    private static bool IsValidCode(string code)
    {
        var previousWasUnderscore = true;
        foreach (var c in code)
        {
            if (c == '_')
            {
                if (previousWasUnderscore)
                {
                    return false;
                }

                previousWasUnderscore = true;
                continue;
            }

            if (c is not ((>= 'A' and <= 'Z') or (>= '0' and <= '9')))
            {
                return false;
            }

            previousWasUnderscore = false;
        }

        return !previousWasUnderscore;
    }
}
