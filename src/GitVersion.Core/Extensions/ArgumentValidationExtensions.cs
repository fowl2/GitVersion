using System.Runtime.CompilerServices;

namespace GitVersion.Extensions;

#if NETFRAMEWORK
public sealed class ArgumentNullException : System.ArgumentNullException
{
    public ArgumentNullException() : base()
    {
    }

    public ArgumentNullException(string paramName) : base(paramName)
    {
    }

    public ArgumentNullException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ArgumentNullException(string paramName, string message) : base(paramName, message)
    {
    }

    public static void ThrowIfNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
            throw new System.ArgumentNullException(paramName);
    }
}
#endif
