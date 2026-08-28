namespace Befriender.UI.FriendList;

using Befriender.Core.Command.Contracts;
using Befriender.Core.Framework;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Commands;
using Befriender.UI.FriendList.Components;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.FriendList.Providers;
using Befriender.UI.FriendList.Services;
using Befriender.UI.FriendList.Tabs;
using Befriender.UI.FriendList.Windows;
using Befriender.UI.Windows.Contracts;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;

public class FriendListUIFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IWindowNavigationService, WindowNavigationService>();
        services.AddSingleton<IFriendDisplayService, FriendDisplayService>();
        services.AddSingleton<VanillaFriendListModifierService>();

        // Register feature-specific localization provider
        services.AddSingleton<ILocalizationProvider, FriendListLocalizationProvider>();

        // Register UI Components
        services.AddSingleton<ListToolbarComponent>();
        services.AddSingleton<FriendListTableComponent>();
        services.AddSingleton<ArchiveTableComponent>();
        services.AddSingleton<FriendProfilePanelComponent>();
        services.AddSingleton<FriendStatusBarComponent>();

        // Register the List tab orchestrator
        services.AddSingleton<ITab, ListTab>();
        services.AddSingleton<ITab, ArchiveTab>();
        services.AddSingleton<ITab, GroupsTab>();
        services.AddSingleton<ITab, AboutTab>();

        services.AddSingleton<FriendListWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<FriendListWindow>());
        services.AddSingleton<ICommand, OpenFriendListCommand>();
    }
}