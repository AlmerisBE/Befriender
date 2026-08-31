namespace Befriender.UI.MainWindow;

using Befriender.Core.Framework;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Command.Contracts;
using Befriender.UI.MainWindow.Commands;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.MainWindow.Contracts;
using Befriender.UI.MainWindow.Lists.Consolidated;
using Befriender.UI.MainWindow.Lists.FreeCompany;
using Befriender.UI.MainWindow.Lists.Friends;
using Befriender.UI.MainWindow.Providers;
using Befriender.UI.MainWindow.Services;
using Befriender.UI.MainWindow.Tabs;
using Befriender.UI.MainWindow.Windows;
using Befriender.UI.Windows.Contracts;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;

public class MainWindowFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IWindowNavigationService, WindowNavigationService>();
        services.AddSingleton<VanillaFriendListModifierService>();

        services.AddSingleton<ILocalizationProvider, MainWindowLocalizationProvider>();

        // Components
        services.AddSingleton<ListToolbarComponent>();
        services.AddSingleton<CharacterProfilePanelComponent>();
        services.AddSingleton<FriendStatusBarComponent>();
        services.AddSingleton<GroupManagementComponent>();
        services.AddSingleton<TagManagementComponent>();
        services.AddSingleton<RemoveConfirmationModalComponent>();

        // Register the tabs
        services.AddSingleton<ITab, FriendListTab>();
        services.AddSingleton<ITab, ConsolidatedTab>();
        services.AddSingleton<ITab, FreeCompanyTab>();
        services.AddSingleton<ITab, AboutTab>();

        // Window & Command
        services.AddSingleton<MainWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<MainWindow>());
        services.AddSingleton<ICommand, OpenMainWindowCommand>();
    }
}