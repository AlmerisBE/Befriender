namespace Befriender.UI.FriendList;

using Befriender.Core.Command.Contracts;
using Befriender.Core.Framework;
using Befriender.UI.FriendList.Commands;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.FriendList.Services;
using Befriender.UI.FriendList.Windows;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;

public class FriendListUIFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        // Register display services
        services.AddSingleton<IFriendDisplayService, FriendDisplayService>();

        // Register the window
        services.AddSingleton<FriendListWindow>();
        services.AddSingleton<Window>(provider => provider.GetRequiredService<FriendListWindow>());

        // Register the command to open it
        services.AddSingleton<ICommand, OpenFriendListCommand>();
    }
}