namespace Befriender.Core.Input.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Input.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;

public unsafe class HotkeyService : IHotkeyService, IDisposable {
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
        bool isInputFocused = false;

        try {
            if (ImGui.GetIO().WantCaptureKeyboard) {
                isInputFocused = true;
            }

            var atkStage = AtkStage.Instance();
            if (atkStage != null && atkStage->GetFocus() != null) {
                isInputFocused = true;
            }
        }
        catch { } // Silently swallow exceptions in isolated unit test environments

        // Trigger only if the key is just pressed AND no text input is currently focused
        if (isKeyPressed && !this.wasKeyPressed && !isInputFocused) {
            bool ctrlPressed = this.keyState[VirtualKey.CONTROL];
            bool shiftPressed = this.keyState[VirtualKey.SHIFT];
            bool altPressed = this.keyState[VirtualKey.MENU];

            if (ctrlPressed == config.HotkeyCtrl &&
                shiftPressed == config.HotkeyShift &&
                altPressed == config.HotkeyAlt) {
                this.OnHotkeyPressed?.Invoke();
            }
        }

        // Always track the physical key state accurately to avoid stuck logic
        this.wasKeyPressed = isKeyPressed;
    }

    public void Dispose() {
        this.framework.Update -= this.OnUpdate;
    }
}