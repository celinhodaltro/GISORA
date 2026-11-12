namespace GISORA.BuildingBlocks.Domain;

/// <summary>
/// Excecao que representa violacao de invariante de dominio.
/// Regra de negocio quebrada nao e erro de infraestrutura: e contrato do modelo.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message, string code = "domain.invariant")
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}