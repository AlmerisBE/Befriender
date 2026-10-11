namespace Befriender.UI.ContextMenu;

using Befriender.Core.Framework;
using Befriender.UI.ContextMenu.Services;
using Microsoft.Extensions.DependencyInjection;

public class ContextMenuFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        // Automatically initialized service (no interface needed for event subscribers)
        services.AddSingleton<ContextMenuService>();
    }
}