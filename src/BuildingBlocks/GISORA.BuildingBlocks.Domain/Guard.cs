namespace GISORA.BuildingBlocks.Domain;

/// <summary>
/// Checagens de invariante com mensagem e codigo estaveis.
/// </summary>
public static class Guard
{
    public static T NotNull<T>(T? value, string parameterName)
        where T : class
        => value ?? throw new DomainException($"'{parameterName}' e obrigatorio.", "guard.null");

    public static string NotNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"'{parameterName}' e obrigatorio.", "guard.empty");
        }

        return value;
    }

    public static Guid NotEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"'{parameterName}' nao pode ser vazio.", "guard.empty-guid");
        }

        return value;
    }

    public static decimal Positive(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new DomainException($"'{parameterName}' deve ser maior que zero.", "guard.positive");
        }

        return value;
    }

    public static int Positive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new DomainException($"'{parameterName}' deve ser maior que zero.", "guard.positive");
        }

        return value;
    }

    public static void Condition(bool condition, string message, string code = "guard.condition")
    {
        if (!condition)
        {
            throw new DomainException(message, code);
        }
    }
}