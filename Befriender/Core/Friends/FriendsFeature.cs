namespace Befriender.Core.Friends;

using Befriender.Core.Characters.Contracts;
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

        // Register the exact same instance for all its specialized contracts
        services.AddSingleton<FriendRepository>();
        services.AddSingleton<IFriendRepository>(provider => provider.GetRequiredService<FriendRepository>());
        services.AddSingleton<ICharacterSource>(provider => provider.GetRequiredService<FriendRepository>());

        services.AddSingleton<IFriendGroupRepository, FriendGroupRepository>();
        services.AddSingleton<IFriendTagRepository, FriendTagRepository>();
        services.AddSingleton<IFriendTagStorage, JsonFriendTagStorage>();

        services.AddSingleton<FriendSyncService>();
        services.AddSingleton<IFriendSyncService>(provider => provider.GetRequiredService<FriendSyncService>());
    }
}