using GISORA.BuildingBlocks.Domain;

namespace GISORA.ArchitectureTests;

public class HostRulesTests
{
    private static readonly Type[] ForbiddenBaseTypes =
    [
        typeof(ValueObject),
        typeof(AggregateRoot<>),
    ];

    [Fact]
    public void WebHostDoesNotDefineAggregatesOrValueObjects()
    {
        var hostAssembly = typeof(Web.App).Assembly;

        foreach (var type in hostAssembly.GetTypes())
        {
            foreach (var forbiddenBaseType in ForbiddenBaseTypes)
            {
                Assert.False(
                    DerivesFrom(type, forbiddenBaseType),
                    $"'{type.FullName}' herda de '{forbiddenBaseType.Name}', que pertence ao dominio.");
            }
        }
    }

    private static bool DerivesFrom(Type type, Type baseType)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current == baseType)
            {
                return true;
            }

            if (baseType.IsGenericTypeDefinition
                && current.IsGenericType
                && current.GetGenericTypeDefinition() == baseType)
            {
                return true;
            }
        }

        return false;
    }
}