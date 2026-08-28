namespace Befriender.Core.Proximity;

using Befriender.Core.Framework;
using Befriender.Core.Proximity.Contracts;
using Befriender.Core.Proximity.Services;
using Microsoft.Extensions.DependencyInjection;

public class ProximityFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ProximityService>();
        services.AddSingleton<IProximityService>(provider => provider.GetRequiredService<ProximityService>());
    }
}