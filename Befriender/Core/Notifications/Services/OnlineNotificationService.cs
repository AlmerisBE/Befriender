namespace Befriender.Core.Notifications.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;

public class OnlineNotificationService : IDisposable {
    private ICharacterRegistry registry;
    private IChatGui chatGui;

    public OnlineNotificationService(ICharacterRegistry registry, IChatGui chatGui) {
        this.registry = registry;
        this.chatGui = chatGui;

        this.registry.CharacterLoggedOn += this.OnCharacterLoggedOn;
    }

    private void OnCharacterLoggedOn(Character character) {
        if (!character.IsTrackedForNotifications) {
            return;
        }

        var message = new SeString(new List<Payload> {
            new UIForegroundPayload(500),
            new TextPayload($"[Befriender] {character.Name} is now online!"),
            new UIForegroundPayload(0)
        });

        this.chatGui.Print(message);
    }

    public void Dispose() {
        this.registry.CharacterLoggedOn -= this.OnCharacterLoggedOn;
    }
}