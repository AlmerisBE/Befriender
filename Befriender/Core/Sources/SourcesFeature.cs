namespace Befriender.Core.Sources;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Framework;
using Befriender.Core.Sources.FreeCompany;
using Befriender.Core.Sources.FreeCompany.Contracts;
using Befriender.Core.Sources.FreeCompany.Scanners;
using Befriender.Core.Sources.Friends;
using Befriender.Core.Sources.Friends.Contracts;
using Befriender.Core.Sources.Friends.Scanners;
using Microsoft.Extensions.DependencyInjection;

public class SourcesFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        // FriendList Source
        services.AddSingleton<IFriendListScanner, MemoryFriendListScanner>();
        services.AddSingleton<FriendListSource>();
        services.AddSingleton<ICharacterSource>(provider => provider.GetRequiredService<FriendListSource>());

        // FreeCompany Source
        services.AddSingleton<IFreeCompanyScanner, MemoryFreeCompanyScanner>();
        services.AddSingleton<FreeCompanySource>();
        services.AddSingleton<ICharacterSource>(provider => provider.GetRequiredService<FreeCompanySource>());
    }
}