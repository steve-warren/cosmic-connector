using System.Runtime.CompilerServices;

namespace Cosmodust.Shared;

internal static class Ensure
{
    public static void NotNullOrWhiteSpace(
        string? argument,
        string? message = null,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
            throw new ArgumentException(message, paramName);
    }

public static void NotNull(
        object? argument,
        string? message = null,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName, message);
    }
}
