using GISORA.BuildingBlocks.Domain;

namespace GISORA.Properties.Domain;

public sealed class OwnershipShare : ValueObject
{
    private OwnershipShare(decimal percentage) => Percentage = percentage;

    public decimal Percentage { get; }

    public static OwnershipShare FromPercentage(decimal percentage)
    {
        Guard.Condition(percentage > 0, "Fracao de titularidade deve ser maior que zero.", "properties.ownership.share-minimum");
        Guard.Condition(percentage <= 100, "Fracao de titularidade nao pode exceder 100%.", "properties.ownership.share-maximum");

        return new OwnershipShare(percentage);
    }

    public override string ToString() => $"{Percentage}%";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Percentage;
    }
}