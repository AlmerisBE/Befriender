namespace Befriender.Core.Proximity.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProximityService : IProximityService, IDisposable {
    private ICharacterRegistry registry;
    private IConfigurationService configService;
    private INotificationManager notificationManager;
    private ILocalizationService loc;
    private IObjectTable objectTable;
    private IFramework framework;
    private IClientState clientState;

    private HashSet<Guid> currentlyNearbyIds = new();
    private DateTime lastScanTime = DateTime.MinValue;
    private readonly TimeSpan scanInterval = TimeSpan.FromSeconds(2);

    public ProximityService(ICharacterRegistry registry, IConfigurationService configService, INotificationManager notificationManager, ILocalizationService loc, IObjectTable objectTable, IFramework framework, IClientState clientState) {
        this.registry = registry;
        this.configService = configService;
        this.notificationManager = notificationManager;
        this.loc = loc;
        this.objectTable = objectTable;
        this.framework = framework;
        this.clientState = clientState;

        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        var now = DateTime.Now;
        if (now - this.lastScanTime < this.scanInterval) {
            return;
        }

        this.lastScanTime = now;

        var config = this.configService.GetConfig();
        if (!config.EnableProximityDetection) {
            if (this.currentlyNearbyIds.Count > 0) {
                this.currentlyNearbyIds.Clear();
            }

            return;
        }

        var allCharacters = this.registry.GetAllCharacters();
        var newNearbyIds = new HashSet<Guid>();
        bool stateChanged = false;

        for (int i = 0; i < this.objectTable.Length; i++) {
            var obj = this.objectTable[i];
            if (obj is not IPlayerCharacter pc) {
                continue;
            }

            var localPlayer = this.objectTable.LocalPlayer;
            if (localPlayer != null && pc.Address == localPlayer.Address) {
                continue;
            }

            var character = allCharacters.FirstOrDefault(c => string.Equals(c.Name, pc.Name.TextValue, StringComparison.OrdinalIgnoreCase) && c.HomeWorldId == pc.HomeWorld.RowId);

            if (character != null) {
                newNearbyIds.Add(character.Id);

                // Forcer la mise à jour absolue des données
                if (!character.IsOnline || character.LocationId != this.clientState.TerritoryType || character.JobId != pc.ClassJob.RowId || character.Level != pc.Level) {
                    character.IsOnline = true;
                    character.LocationId = this.clientState.TerritoryType;
                    character.JobId = (byte)pc.ClassJob.RowId;
                    character.Level = pc.Level;
                    character.CurrentWorldId = pc.CurrentWorld.RowId;
                    character.LastSeenAt = now;
                    stateChanged = true;
                }

                if (!this.currentlyNearbyIds.Contains(character.Id)) {
                    bool shouldNotify = (character.IsActivelyTracked && config.NotifyOnNearbyFriends) || (!character.IsActivelyTracked && config.NotifyOnNearbyArchived);

                    if (shouldNotify) {
                        this.notificationManager.AddNotification(new Notification {
                            Title = "Befriender",
                            Content = this.loc.Translate("Notification_FriendNearby", character.Name),
                            Type = NotificationType.Info
                        });
                    }
                }
            }
        }

        this.currentlyNearbyIds = newNearbyIds;

        if (stateChanged) {
            this.registry.SaveMasterList();
        }
    }

    public bool IsFriendNearby(ulong contentId) {
        var character = this.registry.GetAllCharacters().FirstOrDefault(c => c.ContentId == contentId);
        return character != null && this.currentlyNearbyIds.Contains(character.Id);
    }

    public IReadOnlyList<ulong> GetNearbyFriendIds() {
        return this.registry.GetAllCharacters()
            .Where(c => this.currentlyNearbyIds.Contains(c.Id))
            .Select(c => c.ContentId)
            .ToList();
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}