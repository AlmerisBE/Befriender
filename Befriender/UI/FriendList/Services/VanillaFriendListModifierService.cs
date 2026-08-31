namespace Befriender.UI.FriendList.Services;

using Befriender.Core.Characters.Contracts;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;

public unsafe class VanillaFriendListModifierService : IDisposable {
    private struct NodeState {
        public string Text;
        public ByteColor Color;
    }

    private IAddonLifecycle addonLifecycle;
    private ICharacterRegistry registry;
    private IPluginLog pluginLog;

    private Dictionary<nint, NodeState> originalNodeStates = new();
    private HashSet<nint> activeNodesThisFrame = new();
    private List<nint> staleKeysToRemove = new();

    public VanillaFriendListModifierService(IAddonLifecycle addonLifecycle, ICharacterRegistry registry, IPluginLog pluginLog) {
        this.addonLifecycle = addonLifecycle;
        this.registry = registry;
        this.pluginLog = pluginLog;

        this.addonLifecycle.RegisterListener(AddonEvent.PreDraw, "FriendList", this.OnFriendListPreDraw);
    }

    private void OnFriendListPreDraw(AddonEvent type, AddonArgs args) {
        try {
            this.activeNodesThisFrame.Clear();

            var addon = (AtkUnitBase*)args.Addon.Address;
            if (addon == null || !addon->IsVisible) {
                this.originalNodeStates.Clear();
                return;
            }

            var markedCharacters = this.registry.GetAllCharacters()
                .Where(c => c.IsMarkedForRemoval)
                .Select(c => c.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            this.TraverseAndColor(&addon->UldManager, markedCharacters);
            this.CleanupStaleNodes();
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, "Failed to apply native colors to FriendList.");
        }
    }

    private void TraverseAndColor(AtkUldManager* uldManager, HashSet<string> markedNames) {
        if (uldManager == null) {
            return;
        }

        for (int i = 0; i < uldManager->NodeListCount; i++) {
            var node = uldManager->NodeList[i];
            if (node == null || !node->IsVisible()) {
                continue;
            }

            if (node->Type == NodeType.Text) {
                var textNode = (AtkTextNode*)node;

                byte* stringPtr = textNode->NodeText.StringPtr;

                if (stringPtr != null) {
                    var text = MemoryHelper.ReadSeStringNullTerminated((nint)stringPtr).TextValue.Trim();

                    if (!string.IsNullOrEmpty(text)) {
                        var nodePtr = (nint)textNode;

                        if (markedNames.Contains(text)) {
                            if (!this.originalNodeStates.TryGetValue(nodePtr, out var state) || state.Text != text) {
                                this.originalNodeStates[nodePtr] = new NodeState {
                                    Text = text,
                                    Color = textNode->TextColor
                                };
                            }

                            textNode->TextColor = new ByteColor { A = 255, R = 255, G = 75, B = 75 };
                            this.activeNodesThisFrame.Add(nodePtr);
                        }
                    }
                }
            }
            else if ((ushort)node->Type >= 1000) {
                var compNode = (AtkComponentNode*)node;
                if (compNode->Component != null) {
                    this.TraverseAndColor(&compNode->Component->UldManager, markedNames);
                }
            }
        }
    }

    private void CleanupStaleNodes() {
        this.staleKeysToRemove.Clear();

        foreach (var kvp in this.originalNodeStates) {
            if (!this.activeNodesThisFrame.Contains(kvp.Key)) {
                var nodePtr = kvp.Key;
                var state = kvp.Value;
                var textNode = (AtkTextNode*)nodePtr;

                byte* stringPtr = textNode->NodeText.StringPtr;

                if (stringPtr != null) {
                    var text = MemoryHelper.ReadSeStringNullTerminated((nint)stringPtr).TextValue.Trim();

                    if (string.Equals(text, state.Text, StringComparison.OrdinalIgnoreCase)) {
                        textNode->TextColor = state.Color;
                    }
                }

                this.staleKeysToRemove.Add(nodePtr);
            }
        }

        foreach (var key in this.staleKeysToRemove) {
            this.originalNodeStates.Remove(key);
        }
    }

    public void Dispose() {
        this.addonLifecycle.UnregisterListener(AddonEvent.PreDraw, "FriendList", this.OnFriendListPreDraw);
        this.originalNodeStates.Clear();
        this.activeNodesThisFrame.Clear();
    }
}