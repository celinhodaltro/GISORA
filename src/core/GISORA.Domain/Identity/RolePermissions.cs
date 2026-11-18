namespace GISORA.Domain.Identity;

public enum AccessRight
{
    ViewOverview,
    EditCadastral,
    ManageTerritory,
    ManageData,
    ManageUsers,
}

public static class RoleRights
{
    private static readonly IReadOnlyCollection<AccessRight> Administrator =
    [
        AccessRight.ViewOverview,
        AccessRight.EditCadastral,
        AccessRight.ManageTerritory,
        AccessRight.ManageData,
        AccessRight.ManageUsers,
    ];

    private static readonly IReadOnlyCollection<AccessRight> Analyst =
    [
        AccessRight.ViewOverview,
        AccessRight.EditCadastral,
        AccessRight.ManageTerritory,
    ];

    private static readonly IReadOnlyCollection<AccessRight> PublicViewer =
    [
        AccessRight.ViewOverview,
    ];

    public static IReadOnlyCollection<AccessRight> For(UserRole role) => role switch
    {
        UserRole.Administrator => Administrator,
        UserRole.Analyst => Analyst,
        UserRole.PublicViewer => PublicViewer,
        _ => [],
    };

    public static bool Includes(UserRole role, AccessRight permission) => For(role).Contains(permission);
}