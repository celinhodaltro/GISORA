using GISORA.BuildingBlocks.Application;
using GISORA.Territory.Domain;

namespace GISORA.Territory.Application;

public sealed record MunicipalityListItem(MunicipalityId Id, string Name, string IbgeCode);

public sealed record DistrictListItem(DistrictId Id, MunicipalityId MunicipalityId, string Name);

public sealed record NeighborhoodListItem(NeighborhoodId Id, DistrictId DistrictId, string Name);

public sealed record FiscalSectorListItem(FiscalSectorId Id, DistrictId DistrictId, string Code);

public interface ITerritoryQueries
{
    Task<IReadOnlyList<MunicipalityListItem>> ListMunicipalitiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DistrictListItem>> ListDistrictsAsync(MunicipalityId? municipalityId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NeighborhoodListItem>> ListNeighborhoodsAsync(DistrictId? districtId = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FiscalSectorListItem>> ListFiscalSectorsAsync(DistrictId? districtId = null, CancellationToken cancellationToken = default);
}

public sealed record CreateDistrictCommand(MunicipalityId MunicipalityId, string Name);

public sealed class CreateDistrictHandler(IDistrictRepository repository)
{
    public async Task<Result<DistrictId>> HandleAsync(CreateDistrictCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var name = command.Name.Trim();

            if (await repository.ExistsByNameAsync(command.MunicipalityId, name, cancellationToken))
            {
                return Result.Fail<DistrictId>(Error.Conflict("Ja existe distrito com este nome no municipio.", "territory.district.duplicated"));
            }

            var district = District.Create(command.MunicipalityId, name);
            await repository.AddAsync(district, cancellationToken);

            return Result.Ok(district.Id);
        }
        catch (BuildingBlocks.Domain.DomainException exception)
        {
            return Result.Fail<DistrictId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record CreateNeighborhoodCommand(DistrictId DistrictId, string Name);

public sealed class CreateNeighborhoodHandler(INeighborhoodRepository repository)
{
    public async Task<Result<NeighborhoodId>> HandleAsync(CreateNeighborhoodCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var name = command.Name.Trim();

            if (await repository.ExistsByNameAsync(command.DistrictId, name, cancellationToken))
            {
                return Result.Fail<NeighborhoodId>(Error.Conflict("Ja existe bairro com este nome no distrito.", "territory.neighborhood.duplicated"));
            }

            var neighborhood = Neighborhood.Create(command.DistrictId, name);
            await repository.AddAsync(neighborhood, cancellationToken);

            return Result.Ok(neighborhood.Id);
        }
        catch (BuildingBlocks.Domain.DomainException exception)
        {
            return Result.Fail<NeighborhoodId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record CreateFiscalSectorCommand(DistrictId DistrictId, string Code);

public sealed class CreateFiscalSectorHandler(IFiscalSectorRepository repository)
{
    public async Task<Result<FiscalSectorId>> HandleAsync(CreateFiscalSectorCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var code = command.Code.Trim();

            if (await repository.ExistsByCodeAsync(command.DistrictId, code, cancellationToken))
            {
                return Result.Fail<FiscalSectorId>(Error.Conflict("Ja existe setor fiscal com este codigo no distrito.", "territory.fiscalsector.duplicated"));
            }

            var sector = FiscalSector.Create(command.DistrictId, code);
            await repository.AddAsync(sector, cancellationToken);

            return Result.Ok(sector.Id);
        }
        catch (BuildingBlocks.Domain.DomainException exception)
        {
            return Result.Fail<FiscalSectorId>(Error.Validation(exception.Message, exception.Code));
        }
    }
}