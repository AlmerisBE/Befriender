namespace Befriender.Core.Friends;

using Befriender.Core.Framework;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Services;
using Microsoft.Extensions.DependencyInjection;

public class FriendsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IFriendStorage, JsonFriendStorage>();
        services.AddSingleton<IFriendRepository, FriendRepository>();
        services.AddSingleton<FriendSyncService>();
    }
}