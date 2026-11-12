namespace GISORA.BuildingBlocks.Domain;

/// <summary>
/// Fato relevante ocorrido no dominio. Nao substitui chamada direta entre objetos.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}