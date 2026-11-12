using GISORA.BuildingBlocks.Application;
using GISORA.Properties.Domain;

namespace GISORA.Properties.Application.Queries;

public sealed record GetPropertyQuery(PropertyId PropertyId);

public sealed class GetPropertyHandler(IPropertyRepository repository)
{
    public async Task<Result<PropertyDto>> HandleAsync(GetPropertyQuery query, CancellationToken cancellationToken = default)
    {
        var property = await repository.FindAsync(query.PropertyId, cancellationToken);

        return property is null
            ? Result.Fail<PropertyDto>(Error.NotFound("Imovel nao encontrado.", "properties.not-found"))
            : Result.Ok(property.ToDto());
    }
}