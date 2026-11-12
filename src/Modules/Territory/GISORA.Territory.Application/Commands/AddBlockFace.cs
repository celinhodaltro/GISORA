using GISORA.BuildingBlocks.Application;
using GISORA.BuildingBlocks.Domain;
using GISORA.Geospatial.Domain;
using GISORA.Territory.Domain;

namespace GISORA.Territory.Application.Commands;

public sealed record AddBlockFaceCommand(UrbanBlockId UrbanBlockId, Guid StreetId, LineString Geometry, decimal FrontageMeters);

public sealed class AddBlockFaceHandler(IUrbanBlockRepository repository)
{
    public async Task<Result<Guid>> HandleAsync(AddBlockFaceCommand command, CancellationToken cancellationToken = default)
    {
        var urbanBlock = await repository.FindAsync(command.UrbanBlockId, cancellationToken);
        if (urbanBlock is null)
        {
            return Result.Fail<Guid>(Error.NotFound("Quadra nao encontrada.", "territory.urbanblock.not-found"));
        }

        try
        {
            var face = urbanBlock.AddFace(command.StreetId, command.Geometry, Distance.FromMeters(command.FrontageMeters));
            await repository.UpdateAsync(urbanBlock, cancellationToken);

            return Result.Ok(face.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<Guid>(Error.Validation(exception.Message, exception.Code));
        }
    }
}