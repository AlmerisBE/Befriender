namespace Befriender;

using Befriender.Core.Characters.Services;
using Befriender.Core.Configuration.Services;
using Befriender.Core.Framework;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.Command.Services;
using Befriender.UI.Input.Services;
using Befriender.UI.MainWindow.Contracts;
using Befriender.UI.MainWindow.Services;
using Befriender.UI.Notifications.Services;
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
        IAddonLifecycle addonLifecycle,
        IKeyState keyState,
        INotificationManager notificationManager) {

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
        services.AddSingleton(keyState);
        services.AddSingleton(notificationManager);

        services.AddPluginFeatures();

        this.serviceProvider = services.BuildServiceProvider();

        // Bootstrapping core active services
        this.serviceProvider.GetRequiredService<CommandDispatcher>();
        this.serviceProvider.GetRequiredService<CharacterRegistry>();
        this.serviceProvider.GetRequiredService<VanillaFriendListModifierService>();
        this.serviceProvider.GetRequiredService<HotkeyService>();
        this.serviceProvider.GetRequiredService<OnlineNotificationService>();
        this.serviceProvider.GetRequiredService<IProximityService>();
        this.serviceProvider.GetRequiredService<AutomationService>();

        var windows = this.serviceProvider.GetServices<Window>();
        foreach (var window in windows) {
            this.windowSystem.AddWindow(window);
        }

        this.pluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.pluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
        this.pluginInterface.UiBuilder.OpenMainUi += this.OnOpenMainUi;
    }

    private void OnOpenConfigUi() {
        var navService = this.serviceProvider.GetService<IWindowNavigationService>();
        if (navService != null) {
            navService.OpenTab("Tab_Config");
        }
    }

    private void OnOpenMainUi() {
        var navService = this.serviceProvider.GetService<IWindowNavigationService>();
        if (navService != null) {
            navService.ToggleWindow();
        }
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
        this.pluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
        this.pluginInterface.UiBuilder.OpenMainUi -= this.OnOpenMainUi;

        this.windowSystem.RemoveAllWindows();
        this.serviceProvider.Dispose();
    }
}