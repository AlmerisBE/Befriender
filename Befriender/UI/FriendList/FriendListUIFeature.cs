namespace Befriender.UI.FriendList;

using Befriender.Core.Command.Contracts;
using Befriender.Core.Framework;
using Befriender.UI.FriendList.Commands;
using Befriender.UI.FriendList.Components;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.FriendList.Services;
using Befriender.UI.FriendList.Tabs;
using Befriender.UI.FriendList.Windows;
using Befriender.UI.Windows.Contracts;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;

public class FriendListUIFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IFriendDisplayService, FriendDisplayService>();

        // Register UI Components
        services.AddSingleton<FriendListTableComponent>();
        services.AddSingleton<FriendProfilePanelComponent>();
        services.AddSingleton<FriendStatusBarComponent>();

        // Register the List tab orchestrator
        services.AddSingleton<ITab, ListTab>();

        services.AddSingleton<FriendListWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<FriendListWindow>());
        services.AddSingleton<ICommand, OpenFriendListCommand>();
    }
}