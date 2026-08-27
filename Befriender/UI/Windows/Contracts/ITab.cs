namespace Befriender.UI.Windows.Contracts;

public interface ITab {
    string InternalName { get; }
    string Name { get; }
    void Draw();
}