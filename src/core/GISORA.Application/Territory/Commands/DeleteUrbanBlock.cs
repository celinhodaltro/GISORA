using GISORA.Application.Common;
using GISORA.Domain.Territory;

namespace GISORA.Application.Territory.Commands;

public sealed record DeleteUrbanBlockCommand(UrbanBlockId UrbanBlockId);

public sealed class DeleteUrbanBlockHandler(IUrbanBlockRepository repository)
{
    public async Task<Result> HandleAsync(DeleteUrbanBlockCommand command, CancellationToken cancellationToken = default)
    {
        var urbanBlock = await repository.FindAsync(command.UrbanBlockId, cancellationToken);
        if (urbanBlock is null)
        {
            return Result.Fail(Error.NotFound("Quadra nao encontrada.", "territory.urbanblock.not-found"));
        }

        if (urbanBlock.Faces.Count > 0)
        {
            return Result.Fail(Error.Conflict(
                "Quadra possui faces vinculadas e nao pode ser excluida.",
                "territory.urbanblock.has-faces"));
        }

        await repository.DeleteAsync(command.UrbanBlockId, cancellationToken);

        return Result.Ok();
    }
}