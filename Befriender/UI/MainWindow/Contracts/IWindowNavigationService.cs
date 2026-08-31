namespace Befriender.UI.MainWindow.Contracts;

using System;

public interface IWindowNavigationService {
    event Action<string>? OnTabRequested;
    event Action? OnWindowToggleRequested;

    void OpenTab(string tabInternalName);
    void ToggleWindow();
}