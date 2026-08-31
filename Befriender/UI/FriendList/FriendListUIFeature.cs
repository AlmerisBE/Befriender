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
        services.AddSingleton<VanillaFriendListModifierService>();

        services.AddSingleton<ILocalizationProvider, FriendListLocalizationProvider>();

        // Components
        services.AddSingleton<ListToolbarComponent>();
        services.AddSingleton<CharacterProfilePanelComponent>();
        services.AddSingleton<FriendStatusBarComponent>();
        services.AddSingleton<GroupManagementComponent>();
        services.AddSingleton<TagManagementComponent>();
        services.AddSingleton<RemoveConfirmationModalComponent>();

        // Register the tabs
        services.AddSingleton<ITab, ListTab>();
        services.AddSingleton<ITab, ConsolidatedTab>();
        services.AddSingleton<ITab, FreeCompanyTab>();
        services.AddSingleton<ITab, AboutTab>();

        // Window & Command
        services.AddSingleton<FriendListWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<FriendListWindow>());
        services.AddSingleton<ICommand, OpenFriendListCommand>();
    }
}