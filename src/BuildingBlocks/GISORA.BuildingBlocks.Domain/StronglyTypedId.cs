namespace GISORA.BuildingBlocks.Domain;

/// <summary>
/// Identificador tipado: evita trocar um Guid por outro de significado diferente.
/// A igualdade estrutural vem do record; cada contexto declara seu proprio tipo derivado.
/// </summary>
public abstract record StronglyTypedId<T>(T Value)
    where T : notnull;