namespace Befriender.UI.FriendList.Contracts;

using System;

public interface IWindowNavigationService {
    event Action<string>? OnTabRequested;
    event Action<bool>? OnProfilePanelToggled;

    void OpenTab(string tabInternalName);
    void ToggleProfilePanel(bool open);
}