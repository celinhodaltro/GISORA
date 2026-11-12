namespace GISORA.BuildingBlocks.Domain;

public abstract record StronglyTypedId<T>(T Value)
    where T : notnull;