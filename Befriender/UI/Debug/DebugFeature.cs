namespace Befriender.UI.Debug;

using Befriender.Core.Framework;
using Befriender.UI.Command.Contracts;
using Befriender.UI.Debug.Commands;
using Befriender.UI.Debug.Windows;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;

public class DebugFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<MainCommandDebugWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<MainCommandDebugWindow>());

        services.AddSingleton<ICommand, DebugMainCommand>();
    }
}