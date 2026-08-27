namespace Befriender.UI.FriendList.Services;

using Befriender.UI.FriendList.Contracts;
using System;

public class WindowNavigationService : IWindowNavigationService {
    public event Action<string>? OnTabRequested;
    public event Action? OnWindowToggleRequested;

    public void OpenTab(string tabInternalName) {
        this.OnTabRequested?.Invoke(tabInternalName);
    }

    public void ToggleWindow() {
        this.OnWindowToggleRequested?.Invoke();
    }
}