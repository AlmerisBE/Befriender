namespace Befriender.Api.Memory;

using Befriender.Api.Memory.Scanners;
using Befriender.Core.Framework;
using Befriender.Core.Friends.Contracts;
using Microsoft.Extensions.DependencyInjection;

public class MemoryApiFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IFriendScanner, MemoryFriendScanner>();
    }
}