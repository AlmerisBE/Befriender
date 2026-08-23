using Befriender.Core.Command.Services;
using Befriender.Core.Framework;
using Befriender.UI.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Befriender;

public sealed class Befriender : IDalamudPlugin {
    public string Name => "Befriender";

    private ServiceProvider serviceProvider;
    private IDalamudPluginInterface pluginInterface;
    private WindowSystem windowSystem;

    public Befriender(
        IDalamudPluginInterface pluginInterface,
        IChatGui chatGui,
        ICommandManager commandManager,
        IClientState clientState,
        IPluginLog pluginLog) {
        this.pluginInterface = pluginInterface;
        this.windowSystem = new WindowSystem("Befriender");

        var services = new ServiceCollection();

        // 1. Register Dalamud Services
        services.AddSingleton(this.pluginInterface);
        services.AddSingleton(chatGui);
        services.AddSingleton(commandManager);
        services.AddSingleton(clientState);
        services.AddSingleton(pluginLog);

        // 2. Discover and register all features automatically
        services.AddPluginFeatures();

        // 3. Build the container
        this.serviceProvider = services.BuildServiceProvider();

        // 4. Initialize Core Systems
        this.serviceProvider.GetRequiredService<CommandDispatcher>();

        // 5. Initialize Window System
        var windows = this.serviceProvider.GetServices<Window>();
        foreach (var window in windows) {
            this.windowSystem.AddWindow(window);
        }

        // 6. Hook UI events
        this.pluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.pluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
    }

    private void OnOpenConfigUi() {
        var configWindow = this.serviceProvider.GetService<ConfigWindow>();
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