namespace Befriender.UI.Overlays;

using Befriender.Core.Framework;
using Befriender.UI.Overlays.Windows;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;

public class OverlaysFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<NaviMapOverlayWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<NaviMapOverlayWindow>());
    }
}