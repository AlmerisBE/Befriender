namespace Befriender.Core.Notifications.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;

public class OnlineNotificationService : IDisposable {
    private IFriendRepository friendRepository;
    private IChatGui chatGui;

    public OnlineNotificationService(IFriendRepository friendRepository, IChatGui chatGui) {
        this.friendRepository = friendRepository;
        this.chatGui = chatGui;

        this.friendRepository.FriendLoggedOn += this.OnFriendLoggedOn;
    }

    private void OnFriendLoggedOn(FriendProfile friend) {
        if (!friend.IsTrackedForNotifications) {
            return;
        }

        var message = new SeString(new List<Payload> {
            new UIForegroundPayload(500),
            new TextPayload($"[Befriender] {friend.Name} is now online!"),
            new UIForegroundPayload(0)
        });

        this.chatGui.Print(message);
    }

    public void Dispose() {
        this.friendRepository.FriendLoggedOn -= this.OnFriendLoggedOn;
    }
}