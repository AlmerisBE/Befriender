using Befriender.Core.Framework;
using Befriender.UI.Localization.Contracts;
using Befriender.UI.Localization.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.UI.Localization;

public class LocalizationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ILocalizationService, LocalizationService>();
    }
}