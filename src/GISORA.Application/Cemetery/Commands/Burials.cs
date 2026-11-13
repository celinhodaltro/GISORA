using GISORA.Application.Common;
using GISORA.Domain.Cemetery;
using GISORA.Domain.Common;

namespace GISORA.Application.Cemetery.Commands;

public sealed record RegisterBurialCommand(GraveId GraveId, string PersonName, DateOnly BuriedAt);

public sealed class RegisterBurialHandler(IGraveRepository graves)
{
    public async Task<Result<Guid>> HandleAsync(RegisterBurialCommand command, CancellationToken cancellationToken = default)
    {
        var grave = await graves.FindAsync(command.GraveId, cancellationToken);
        if (grave is null)
        {
            return Result.Fail<Guid>(Error.NotFound("Jazigo nao encontrado.", "cemetery.grave.not-found"));
        }

        try
        {
            var burial = grave.AddBurial(command.PersonName, command.BuriedAt);
            await graves.UpdateAsync(grave, cancellationToken);

            return Result.Ok(burial.Id);
        }
        catch (DomainException exception)
        {
            return Result.Fail<Guid>(Error.Validation(exception.Message, exception.Code));
        }
    }
}

public sealed record ExhumeBurialCommand(GraveId GraveId, Guid BurialId, DateOnly ExhumedAt);

public sealed class ExhumeBurialHandler(IGraveRepository graves)
{
    public async Task<Result> HandleAsync(ExhumeBurialCommand command, CancellationToken cancellationToken = default)
    {
        var grave = await graves.FindAsync(command.GraveId, cancellationToken);
        if (grave is null)
        {
            return Result.Fail(Error.NotFound("Jazigo nao encontrado.", "cemetery.grave.not-found"));
        }

        try
        {
            grave.Exhume(command.BurialId, command.ExhumedAt);
            await graves.UpdateAsync(grave, cancellationToken);

            return Result.Ok();
        }
        catch (DomainException exception)
        {
            return Result.Fail(Error.Validation(exception.Message, exception.Code));
        }
    }
}