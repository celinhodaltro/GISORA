using GISORA.Domain.Identity;

namespace GISORA.Presentation.Shared.Security;

public static class RoleLabels
{
    public static string For(UserRole role) => role switch
    {
        UserRole.Administrator => "Administrador",
        UserRole.Analyst => "Analista",
        UserRole.PublicViewer => "Consulta publica",
        _ => role.ToString(),
    };
}