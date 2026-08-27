namespace Befriender.UI.FriendList.Contracts;

using System;

public interface IWindowNavigationService {
    event Action<string>? OnTabRequested;
    event Action? OnWindowToggleRequested;

    void OpenTab(string tabInternalName);
    void ToggleWindow();
}