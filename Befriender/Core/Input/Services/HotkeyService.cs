namespace Befriender.Core.Input.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Input.Contracts;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using System;

public class HotkeyService : IHotkeyService, IDisposable {
    private IKeyState keyState;
    private IFramework framework;
    private IConfigurationService configService;
    private bool wasKeyPressed = false;

    public event Action? OnHotkeyPressed;

    public HotkeyService(IKeyState keyState, IFramework framework, IConfigurationService configService) {
        this.keyState = keyState;
        this.framework = framework;
        this.configService = configService;

        this.framework.Update += this.OnUpdate;
    }

    private void OnUpdate(IFramework fw) {
        var config = this.configService.GetConfig();
        var targetKey = config.Hotkey;

        if (targetKey == 0) {
            return;
        }

        bool isKeyPressed = this.keyState[targetKey];

        // On vérifie le "just pressed" pour éviter le spam de la fenêtre (Toggle en boucle)
        if (isKeyPressed && !this.wasKeyPressed) {
            bool ctrlPressed = this.keyState[VirtualKey.CONTROL];
            bool shiftPressed = this.keyState[VirtualKey.SHIFT];
            bool altPressed = this.keyState[VirtualKey.MENU];

            if (ctrlPressed == config.HotkeyCtrl &&
                shiftPressed == config.HotkeyShift &&
                altPressed == config.HotkeyAlt) {
                this.OnHotkeyPressed?.Invoke();
            }
        }

        this.wasKeyPressed = isKeyPressed;
    }

    public void Dispose() {
        this.framework.Update -= this.OnUpdate;
    }
}