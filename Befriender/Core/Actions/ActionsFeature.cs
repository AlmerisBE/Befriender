namespace Befriender.Core.Actions;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Actions.Implementations;
using Befriender.Core.Actions.Services;
using Befriender.Core.Framework;
using Microsoft.Extensions.DependencyInjection;

public class ActionsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IFriendAction, CopyNameAction>();
        services.AddSingleton<IFriendAction, SendTellAction>();
        services.AddSingleton<IFriendAction, NativeInviteToPartyAction>();
        services.AddSingleton<IFriendAction, ViewAdventurerPlateAction>();
        services.AddSingleton<IFriendAction, ViewSearchInfoAction>();
        services.AddSingleton<IFriendAction, EstateTeleportationAction>();
        services.AddSingleton<IFriendAction, ViewPartyFinderListingAction>();
        services.AddSingleton<IFriendAction, JoinFriendAction>();
        services.AddSingleton<IFriendAction, DeleteFriendDataAction>();
        services.AddSingleton<IFriendAction, RestoreFriendAction>();

        services.AddSingleton<IFriendActionService, FriendActionService>();
    }
}