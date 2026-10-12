namespace BoaSorte.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Expired,
}

/// <summary>An expected failure. <paramref name="Field"/> points at the offending input, when there is one.</summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "C#-only codebase; 'Error' is the conventional name in the Result pattern.")]
public sealed record Error(string Code, string Message, ErrorType Type, string? Field = null)
{
    public static Error Validation(string code, string message, string? field = null) =>
        new(code, message, ErrorType.Validation, field);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Expired(string code, string message) => new(code, message, ErrorType.Expired);
}
