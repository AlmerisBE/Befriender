namespace Befriender.UI.FriendList.Services;

using Befriender.UI.FriendList.Contracts;
using System;

public class WindowNavigationService : IWindowNavigationService {
    public event Action<string>? OnTabRequested;
    public event Action<bool>? OnProfilePanelToggled;

    public void OpenTab(string tabInternalName) {
        this.OnTabRequested?.Invoke(tabInternalName);
    }

    public void ToggleProfilePanel(bool open) {
        this.OnProfilePanelToggled?.Invoke(open);
    }
}