namespace Befriender.UI.MainWindow.Contracts;

public interface ITab {
    string InternalName { get; }
    string Name { get; }
    int Order { get; }
    bool IsProfilePanelOpen { get; }
    void Draw();
}