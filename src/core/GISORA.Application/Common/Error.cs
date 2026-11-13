namespace GISORA.Application.Common;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string message, string code = "validation") => new(code, message);

    public static Error NotFound(string message, string code = "not-found") => new(code, message);

    public static Error Conflict(string message, string code = "conflict") => new(code, message);

    public static Error Unauthorized(string message, string code = "unauthorized") => new(code, message);

    public static Error Unexpected(string message, string code = "unexpected") => new(code, message);

    public override string ToString() => $"{Code}: {Message}";
}