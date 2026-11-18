using System.Security.Claims;

using GISORA.Application.Identity;
using GISORA.Application.Identity.Commands;

using Microsoft.AspNetCore.Components.Authorization;

namespace GISORA.Presentation.Shared.Security;

public sealed class GisoraAuthenticationStateProvider(
    RestoreSessionHandler restoreSession,
    SignOutHandler signOut) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private Task<AuthenticationState>? _state;

    public SessionUser? CurrentUser { get; private set; }

    public event Action? Changed;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => _state ??= LoadAsync();

    public void SignedIn(SessionUser user)
    {
        CurrentUser = user;
        _state = Task.FromResult(new AuthenticationState(BuildPrincipal(user)));
        NotifyAuthenticationStateChanged(_state);
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        await signOut.HandleAsync();

        CurrentUser = null;
        _state = Task.FromResult(Anonymous);
        NotifyAuthenticationStateChanged(_state);
        Changed?.Invoke();
    }

    public async Task RefreshAsync()
    {
        _state = null;

        var state = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    private async Task<AuthenticationState> LoadAsync()
    {
        try
        {
            CurrentUser = await restoreSession.HandleAsync();
        }
        catch (Exception)
        {
            CurrentUser = null;
        }

        Changed?.Invoke();

        return CurrentUser is null
            ? Anonymous
            : new AuthenticationState(BuildPrincipal(CurrentUser));
    }

    private static ClaimsPrincipal BuildPrincipal(SessionUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        claims.AddRange(user.Permissions.Select(permission => new Claim(IdentityClaimNames.Permission, permission.ToString())));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "gisora"));
    }
}

public static class IdentityClaimNames
{
    public const string Permission = "gisora/permission";
}