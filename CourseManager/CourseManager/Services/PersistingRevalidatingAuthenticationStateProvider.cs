using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using System.Security.Claims;
using CourseManager.Client.Models; // Pfad zu deiner UserInfo
using CourseManager.Data;          // Pfad zu deinem Teacher

// Wir erben von ServerAuthenticationStateProvider -> Viel pflegeleichter!
public class PersistingServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState _state;
    private readonly PersistingComponentStateSubscription _subscription;

    public PersistingServerAuthenticationStateProvider(PersistentComponentState state)
    {
        _state = state;
        // Wir registrieren den Beamer: Pack die Daten ein, bevor WASM übernimmt
        _subscription = _state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    private async Task OnPersistingAsync()
    {
        // Wir holen den Status direkt aus der Basisklasse
        var authenticationState = await GetAuthenticationStateAsync();
        var user = authenticationState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            // Claims extrahieren
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value; // Fallback für manche Identity Setups
            var email = user.FindFirst(ClaimTypes.Name)?.Value
                         ?? user.FindFirst(ClaimTypes.Email)?.Value;

            if (userId != null && email != null)
            {
                // Das Paket für den Client schnüren
                _state.PersistAsJson(nameof(UserInfo), new UserInfo { UserId = userId, Email = email });
            }
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}