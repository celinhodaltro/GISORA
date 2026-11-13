namespace GISORA.Domain.Common;

public abstract record StronglyTypedId<T>(T Value)
    where T : notnull;