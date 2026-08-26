namespace Befriender;

using Befriender.Core.Command.Services;
using Befriender.Core.Framework;
using Befriender.Core.Friends.Services;
using Befriender.UI.FriendList.Services;
using Befriender.UI.FriendList.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;

public sealed class BefrienderPlugin : IDalamudPlugin {
    public string Name => "Befriender";

    private ServiceProvider serviceProvider;
    private IDalamudPluginInterface pluginInterface;
    private WindowSystem windowSystem;

    public BefrienderPlugin(
        IDalamudPluginInterface pluginInterface,
        IChatGui chatGui,
        ICommandManager commandManager,
        IClientState clientState,
        IPluginLog pluginLog,
        IFramework framework,
        IObjectTable objectTable,
        IDataManager dataManager,
        ITextureProvider textureProvider,
        IGameInteropProvider gameInteropProvider,
        IAddonLifecycle addonLifecycle) {

        this.pluginInterface = pluginInterface;
        this.windowSystem = new WindowSystem("Befriender");

        var services = new ServiceCollection();

        services.AddSingleton(this.pluginInterface);
        services.AddSingleton(chatGui);
        services.AddSingleton(commandManager);
        services.AddSingleton(clientState);
        services.AddSingleton(pluginLog);
        services.AddSingleton(framework);
        services.AddSingleton(objectTable);
        services.AddSingleton(dataManager);
        services.AddSingleton(textureProvider);
        services.AddSingleton(gameInteropProvider);
        services.AddSingleton(addonLifecycle);

        services.AddPluginFeatures();

        this.serviceProvider = services.BuildServiceProvider();

        this.serviceProvider.GetRequiredService<CommandDispatcher>();
        this.serviceProvider.GetRequiredService<FriendSyncService>();
        this.serviceProvider.GetRequiredService<VanillaFriendListModifierService>();

        var windows = this.serviceProvider.GetServices<Window>();
        foreach (var window in windows) {
            this.windowSystem.AddWindow(window);
        }

        this.pluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.pluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
    }

    private void OnOpenConfigUi() {
        var configWindow = this.serviceProvider.GetService<FriendListWindow>();
        if (configWindow != null) {
            configWindow.IsOpen = true;
        }
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
        this.pluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;

        this.windowSystem.RemoveAllWindows();
        this.serviceProvider.Dispose();
    }
}