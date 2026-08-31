using Befriender.Core.Framework;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Localization.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Core.Localization;

public class LocalizationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ILocalizationService, LocalizationService>();
    }
}