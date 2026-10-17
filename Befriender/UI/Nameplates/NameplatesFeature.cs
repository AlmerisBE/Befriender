namespace Befriender.UI.Nameplates;

using Befriender.Core.Framework;
using Befriender.UI.Nameplates.Contracts;
using Befriender.UI.Nameplates.Services;
using Microsoft.Extensions.DependencyInjection;

public class NameplatesFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<INameplateService, NameplateService>();
    }
}