namespace GISORA.BuildingBlocks.Application;

/// <summary>
/// Resultado de um caso de uso: sucesso sem valor ou falha com <see cref="Error"/>.
/// Invariante violada no dominio continua sendo excecao; o handler traduz para Result.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Resultado de sucesso nao pode carregar erro.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Resultado de falha exige um erro.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Ok() => new(true, Error.None);

    public static Result Fail(Error error) => new(false, error);

    public static Result<TValue> Ok<TValue>(TValue value) => new(value);

    public static Result<TValue> Fail<TValue>(Error error) => new(error);
}

/// <summary>
/// Resultado com valor. Acessar <see cref="Value"/> em falha e erro de programacao.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue _value;

    internal Result(TValue value)
        : base(true, Error.None)
        => _value = value;

    internal Result(Error error)
        : base(false, error)
        => _value = default!;

    public TValue Value => IsSuccess
        ? _value
        : throw new InvalidOperationException($"Resultado de falha nao possui valor. Erro: {Error}");
}