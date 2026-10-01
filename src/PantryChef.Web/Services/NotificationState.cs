using Microsoft.AspNetCore.Components.Authorization;
using PantryChef.Contracts;

namespace PantryChef.Web.Services;

public sealed class NotificationState : IDisposable
{
    private readonly PantryApiClient _api;
    private readonly AuthenticationStateProvider _auth;

    public NotificationState(PantryApiClient api, AuthenticationStateProvider auth)
    {
        _api = api;
        _auth = auth;
        _auth.AuthenticationStateChanged += OnAuthChanged;
    }

    public IReadOnlyList<NotificationDto> Items { get; private set; } = [];
    public int UnreadCount => Items.Count(n => !n.IsRead);

    public event Action? Changed;

    public async Task RefreshAsync()
    {
        Items = await _api.GetNotificationsAsync();
        Changed?.Invoke();
    }

    public async Task MarkReadAsync(int id)
    {
        await _api.MarkNotificationReadAsync(id);
        await RefreshAsync();
    }

    private async void OnAuthChanged(Task<AuthenticationState> stateTask)
    {
        try
        {
            var state = await stateTask;
            if (state.User.Identity?.IsAuthenticated != true)
            {
                Items = [];
                Changed?.Invoke();
            }
        }
        catch { }
    }

    public void Dispose() => _auth.AuthenticationStateChanged -= OnAuthChanged;
}