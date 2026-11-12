using GISORA.BuildingBlocks.Application;
using GISORA.BuildingBlocks.Domain;
using GISORA.Territory.Domain;

namespace GISORA.Territory.Application.Commands;

public sealed record CreateMunicipalityCommand(string Name, string IbgeCode);

public sealed class CreateMunicipalityHandler(IMunicipalityRepository repository)
{
    public async Task<Result<MunicipalityId>> HandleAsync(CreateMunicipalityCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var ibgeCode = IbgeCode.Parse(command.IbgeCode);

            if (await repository.ExistsByIbgeCodeAsync(ibgeCode, cancellationToken))
            {
                return Result.Fail<MunicipalityId>(
                    Error.Conflict("Municipio com este codigo IBGE ja esta cadastrado.", "territory.municipality.duplicated"));
            }

            var municipality = Municipality.Register(command.Name, ibgeCode);
            await repository.AddAsync(municipality, cancellationToken);

            return Result.Ok(municipality.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<MunicipalityId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}