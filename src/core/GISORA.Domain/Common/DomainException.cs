namespace GISORA.Domain.Common;

public sealed class DomainException : Exception
{
    public DomainException(string message, string code = "domain.invariant")
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}