namespace Befriender.Core.Friends;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Storage;
using Befriender.Core.Framework;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Services;
using Microsoft.Extensions.DependencyInjection;

public class FriendsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<ICharacterIdentityService, CharacterIdentityService>();

        // Removed obsolete IFriendStorage and JsonFriendStorage registrations
        services.AddSingleton<Contracts.ICharacterGroupStorage, JsonCharacterGroupStorage>();

        services.AddSingleton<FriendRepository>();
        services.AddSingleton<IFriendRepository>(provider => provider.GetRequiredService<FriendRepository>());
        services.AddSingleton<ICharacterSource>(provider => provider.GetRequiredService<FriendRepository>());

        services.AddSingleton<IFriendGroupRepository, FriendGroupRepository>();
        services.AddSingleton<IFriendTagRepository, FriendTagRepository>();
        services.AddSingleton<Contracts.ICharacterTagStorage, JsonCharacterTagStorage>();

        services.AddSingleton<FriendSyncService>();
        services.AddSingleton<IFriendSyncService>(provider => provider.GetRequiredService<FriendSyncService>());
    }
}