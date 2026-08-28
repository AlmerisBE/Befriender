namespace Befriender.Core.Proximity.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProximityService : IProximityService, IDisposable {
    private IObjectTable objectTable;
    private IFramework framework;
    private IFriendRepository friendRepository;
    private IConfigurationService configService;
    private INotificationManager notificationManager;
    private ILocalizationService loc;

    private HashSet<ulong> currentlyNearbyIds = new();
    private DateTime lastScanTime = DateTime.MinValue;

    public ProximityService(IObjectTable objectTable, IFramework framework, IFriendRepository friendRepository, IConfigurationService configService, INotificationManager notificationManager, ILocalizationService loc) {
        this.objectTable = objectTable;
        this.framework = framework;
        this.friendRepository = friendRepository;
        this.configService = configService;
        this.notificationManager = notificationManager;
        this.loc = loc;

        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if ((DateTime.Now - this.lastScanTime).TotalSeconds < 2.0) {
            return;
        }

        this.lastScanTime = DateTime.Now;

        var config = this.configService.GetConfig();
        if (!config.EnableProximityDetection) {
            if (this.currentlyNearbyIds.Count > 0) {
                this.currentlyNearbyIds.Clear();
            }

            return;
        }

        var friends = this.friendRepository.GetFriends();
        var newNearbyIds = new HashSet<ulong>();
        var lookup = friends.ToDictionary(f => $"{f.Name}@{f.HomeWorldId}");

        foreach (var obj in this.objectTable) {
            if (obj is not IPlayerCharacter pc) {
                continue;
            }

            if (this.objectTable.LocalPlayer != null && pc.Address == this.objectTable.LocalPlayer.Address) {
                continue;
            }

            string key = $"{pc.Name.TextValue}@{pc.HomeWorld.RowId}";

            if (lookup.TryGetValue(key, out var friend)) {
                newNearbyIds.Add(friend.ContentId);

                if (!this.currentlyNearbyIds.Contains(friend.ContentId)) {
                    bool shouldNotify = (!friend.IsArchived && config.NotifyOnNearbyFriends) || (friend.IsArchived && config.NotifyOnNearbyArchived);

                    if (shouldNotify) {
                        this.notificationManager.AddNotification(new Notification {
                            Title = "Befriender",
                            Content = this.loc.Translate("Notification_FriendNearby", friend.Name),
                            Type = NotificationType.Info
                        });
                    }
                }
            }
        }

        this.currentlyNearbyIds = newNearbyIds;
    }

    public bool IsFriendNearby(ulong contentId) => this.currentlyNearbyIds.Contains(contentId);

    public IReadOnlyList<ulong> GetNearbyFriendIds() => this.currentlyNearbyIds.ToList();

    public void Dispose() => this.framework.Update -= this.OnFrameworkUpdate;
}