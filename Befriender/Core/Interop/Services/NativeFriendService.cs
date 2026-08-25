namespace Befriender.Core.Interop.Services;

using Befriender.Core.Interop.Contracts;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;

public unsafe class NativeFriendService : INativeFriendService {
    private IPluginLog pluginLog;

    // The signature might be outdated due to game updates.
    // By setting Fallibility = Fallibility.Fallible, the plugin will safely load even if the signature is broken.
    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B F9 48 8B F2 48 8B 0D ?? ?? ?? ?? 48 8B 11", Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<nint, ulong, void> removeFriendNative = null;

    public NativeFriendService(IPluginLog pluginLog, IGameInteropProvider gameInteropProvider) {
        this.pluginLog = pluginLog;

        // Tells Dalamud to scan the game memory and bind the function pointer to our signature attribute
        gameInteropProvider.InitializeFromAttributes(this);
    }

    public void RemoveFriend(ulong contentId) {
        try {
            if (this.removeFriendNative == null) {
                this.pluginLog.Error("The memory signature for RemoveFriend could not be resolved. The native action is aborted.");
                return;
            }

            var uiModule = UIModule.Instance();
            if (uiModule == null) {
                return;
            }

            var infoModule = uiModule->GetInfoModule();
            if (infoModule == null) {
                return;
            }

            var friendProxy = infoModule->GetInfoProxyById(InfoProxyId.FriendList);
            if (friendProxy == null) {
                return;
            }

            this.pluginLog.Debug($"Executing native friend removal for ContentId: {contentId}");

            // Execute the native C++ function safely from our managed C# code
            this.removeFriendNative((nint)friendProxy, contentId);
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, "A fatal error occurred while attempting to execute the native friend removal.");
        }
    }
}