namespace Befriender.UI.Theme;

using Befriender.Core.Framework;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Theme.Services;
using Microsoft.Extensions.DependencyInjection;

public class ThemeFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IThemeService, ThemeService>();
    }
}