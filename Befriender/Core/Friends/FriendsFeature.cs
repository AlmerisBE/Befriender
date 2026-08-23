namespace Befriender.Core.Friends;

using Befriender.Core.Framework;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Services;
using Microsoft.Extensions.DependencyInjection;

public class FriendsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ICharacterIdentityService, CharacterIdentityService>();
        services.AddSingleton<IFriendStorage, JsonFriendStorage>();
        services.AddSingleton<IFriendRepository, FriendRepository>();

        // Register the concrete class to keep the framework event running, and forward the interface to it
        services.AddSingleton<FriendSyncService>();
        services.AddSingleton<IFriendSyncService>(provider => provider.GetRequiredService<FriendSyncService>());
    }
}