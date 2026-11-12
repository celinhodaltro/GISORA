namespace GISORA.BuildingBlocks.Application;

public sealed record DemoSeedResult(
    bool AlreadySeeded,
    int Properties,
    int UrbanBlocks,
    int Streets,
    int Facilities);

public interface IDemoDataSeeder
{
    Task<DemoSeedResult> SeedAsync(CancellationToken cancellationToken = default);
}