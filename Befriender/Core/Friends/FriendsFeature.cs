namespace Befriender.Core.Friends;

using Befriender.Core.Framework;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Services;
using Befriender.Core.Friends.Storage;
using Microsoft.Extensions.DependencyInjection;

public class FriendsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ICharacterIdentityService, CharacterIdentityService>();
        services.AddSingleton<IFriendStorage, JsonFriendStorage>();
        services.AddSingleton<IFriendGroupStorage, JsonFriendGroupStorage>();
        services.AddSingleton<IFriendRepository, FriendRepository>();
        services.AddSingleton<IFriendGroupRepository, FriendGroupRepository>();
        services.AddSingleton<IFriendTagRepository, FriendTagRepository>();
        services.AddSingleton<IFriendTagStorage, JsonFriendTagStorage>();

        // Register the concrete class to keep the framework event running, and forward the interface to it
        services.AddSingleton<FriendSyncService>();
        services.AddSingleton<IFriendSyncService>(provider => provider.GetRequiredService<FriendSyncService>());
    }
}