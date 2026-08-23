using Befriender.Core;
using Befriender.Features.Localization.Contracts;
using Befriender.Features.Localization.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender.Features.Localization;

public class LocalizationFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ILocalizationService, LocalizationService>();
    }
}