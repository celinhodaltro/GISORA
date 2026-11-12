using GISORA.BuildingBlocks.Application;
using GISORA.Geospatial.Domain;
using GISORA.Properties.Application.Commands;
using GISORA.PublicFacilities.Application.Commands;
using GISORA.PublicFacilities.Domain;
using GISORA.Roads.Application.Commands;
using GISORA.Roads.Domain;
using GISORA.Territory.Application;
using GISORA.Territory.Application.Commands;

namespace GISORA.Infrastructure.LocalStorage.Demo;

public sealed class DemoDataSeeder(
    ILocalStore store,
    CreateMunicipalityHandler createMunicipality,
    CreateDistrictHandler createDistrict,
    CreateNeighborhoodHandler createNeighborhood,
    CreateFiscalSectorHandler createFiscalSector,
    CreateUrbanBlockHandler createUrbanBlock,
    CreateStreetHandler createStreet,
    CreatePropertyHandler createProperty,
    CreateFacilityHandler createFacility) : IDemoDataSeeder
{
    private const string DemoNamespace = "demo";
    private const string SeededKey = "seeded";

    public async Task<DemoSeedResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var alreadySeeded = await store.GetAsync<bool?>(DemoNamespace, SeededKey, cancellationToken) ?? false;

        if (alreadySeeded)
        {
            return new DemoSeedResult(true, 0, 0, 0, 0);
        }

        var municipality = Require(await createMunicipality.HandleAsync(
            new CreateMunicipalityCommand("Passira", "2611903"), cancellationToken));

        var district = Require(await createDistrict.HandleAsync(
            new CreateDistrictCommand(municipality, "Sede"), cancellationToken));

        var center = Require(await createNeighborhood.HandleAsync(
            new CreateNeighborhoodCommand(district, "Centro"), cancellationToken));

        Require(await createNeighborhood.HandleAsync(
            new CreateNeighborhoodCommand(district, "Alto"), cancellationToken));

        var sector = Require(await createFiscalSector.HandleAsync(
            new CreateFiscalSectorCommand(district, "001"), cancellationToken));

        Require(await createUrbanBlock.HandleAsync(new CreateUrbanBlockCommand(sector, "Q-01"), cancellationToken));

        var matriz = Require(await createStreet.HandleAsync(
            new CreateStreetCommand("Rua da Matriz", StreetType.Street, center.Value), cancellationToken));

        var central = Require(await createStreet.HandleAsync(
            new CreateStreetCommand("Avenida Central", StreetType.Avenue, center.Value), cancellationToken));

        Require(await createProperty.HandleAsync(new CreatePropertyCommand(
            "001", "002", "003", null, matriz.Value, "10", center.Value, 500m, Square(-35.2520, -7.9520)), cancellationToken));

        Require(await createProperty.HandleAsync(new CreatePropertyCommand(
            "001", "002", "004", null, central.Value, "120", center.Value, 420m, Square(-35.2480, -7.9500)), cancellationToken));

        Require(await createProperty.HandleAsync(new CreatePropertyCommand(
            "001", "003", "010", null, matriz.Value, "45", center.Value, 360m, null), cancellationToken));

        Require(await createFacility.HandleAsync(new CreateFacilityCommand(
            "Escola Municipal", FacilityType.School, center.Value, PointAt(-35.2500, -7.9510)), cancellationToken));

        Require(await createFacility.HandleAsync(new CreateFacilityCommand(
            "Praca Central", FacilityType.Square, center.Value, PointAt(-35.2490, -7.9525)), cancellationToken));

        await store.SetAsync(DemoNamespace, SeededKey, true, cancellationToken);

        return new DemoSeedResult(false, 3, 1, 2, 2);
    }

    private static T Require<T>(Result<T> result)
        => result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"Falha ao carregar dados demonstrativos: {result.Error}");

    private static Polygon Square(double longitude, double latitude)
    {
        const double size = 0.0012;

        return new Polygon([
            new CoordinateSequence([
                new Coordinate(longitude, latitude, SpatialReference.Wgs84),
                new Coordinate(longitude + size, latitude, SpatialReference.Wgs84),
                new Coordinate(longitude + size, latitude + size, SpatialReference.Wgs84),
                new Coordinate(longitude, latitude + size, SpatialReference.Wgs84),
                new Coordinate(longitude, latitude, SpatialReference.Wgs84),
            ]),
        ]);
    }

    private static Point PointAt(double longitude, double latitude)
        => new(new Coordinate(longitude, latitude, SpatialReference.Wgs84));
}