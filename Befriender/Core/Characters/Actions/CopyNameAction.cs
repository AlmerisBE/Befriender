namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using System;

public class CopyNameAction : ICharacterAction {
    private IChatGui chatGui;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000013");
    public string InternalName => "Action_CopyName";
    public FontAwesomeIcon Icon => FontAwesomeIcon.Copy;

    public CopyNameAction(IChatGui chatGui) {
        this.chatGui = chatGui;
    }

    public bool CanExecute(Character character) {
        return !string.IsNullOrEmpty(character.Name);
    }

    public void Execute(Character character) {
        Dalamud.Bindings.ImGui.ImGui.SetClipboardText(character.Name);
        this.chatGui.Print($"[Befriender] Copied {character.Name} to clipboard.");
    }
}