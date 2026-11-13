namespace GISORA.Infrastructure.Persistence;

public class StorageException : Exception
{
    public StorageException(string message)
        : base(message)
    {
    }

    public StorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public sealed class StorageUnavailableException : StorageException
{
    public StorageUnavailableException()
        : base("Armazenamento local indisponivel neste ambiente.")
    {
    }
}

public sealed class StorageQuotaExceededException : StorageException
{
    public StorageQuotaExceededException(Exception innerException)
        : base("Limite de armazenamento local excedido.", innerException)
    {
    }
}