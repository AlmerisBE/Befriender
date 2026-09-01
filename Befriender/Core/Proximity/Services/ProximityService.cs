namespace Befriender.Core.Proximity.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.GameData.Contracts;
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
    private IObjectTable objectTable;
    private IClientState clientState;
    private IConfigurationService configService;
    private INotificationManager notificationManager;
    private ILocalizationService loc;
    private IFramework framework;
    private IGameDataService gameDataService;

    private HashSet<Guid> currentlyNearbyIds = new();
    private DateTime lastScanTime = DateTime.MinValue;

    public ProximityService(
        ICharacterRegistry registry,
        IObjectTable objectTable,
        IClientState clientState,
        IConfigurationService configService,
        INotificationManager notificationManager,
        ILocalizationService loc,
        IFramework framework,
        IGameDataService gameDataService) {

        this.registry = registry;
        this.objectTable = objectTable;
        this.clientState = clientState;
        this.configService = configService;
        this.notificationManager = notificationManager;
        this.loc = loc;
        this.framework = framework;
        this.gameDataService = gameDataService;

        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        var now = DateTime.Now;
        if ((now - this.lastScanTime).TotalSeconds < 2.0) {
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

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return;
        }

        var allCharacters = this.registry.GetAllCharacters();
        var lookup = new Dictionary<(string, uint), Character>();
        foreach (var c in allCharacters) {
            lookup[(c.Name, c.HomeWorldId)] = c;
        }

        uint currentTerritory = this.clientState.TerritoryType;
        uint localCurrentWorld = localPlayer.CurrentWorld.RowId;
        bool isStandardTerritory = this.gameDataService.IsStandardTerritory(currentTerritory);

        uint effectiveLocationId = currentTerritory;
        bool hasServerLocation = isStandardTerritory;

        // Stratégie Ysaline : Le joueur local est l'ancre de vérité absolue.
        // Si nous sommes dans une instance, on récupère le vrai LocationId du serveur depuis le propre profil du joueur local.
        if (!isStandardTerritory) {
            var localPlayerChar = allCharacters.FirstOrDefault(c =>
                c.Name.Equals(localPlayer.Name.TextValue, StringComparison.Ordinal) &&
                c.HomeWorldId == localPlayer.HomeWorld.RowId);

            if (localPlayerChar != null && localPlayerChar.LocationId > 0 && localPlayerChar.LocationId != currentTerritory) {
                effectiveLocationId = localPlayerChar.LocationId;
                hasServerLocation = true;
            }
        }

        var newNearbyIds = new HashSet<Guid>();
        bool stateChanged = false;

        for (int i = 0; i < this.objectTable.Length; i++) {
            var obj = this.objectTable[i];

            if (obj is IPlayerCharacter pc && pc.Address != localPlayer.Address && pc.HomeWorld.RowId > 0) {
                var key = (pc.Name.TextValue, pc.HomeWorld.RowId);

                if (lookup.TryGetValue(key, out var friend)) {
                    newNearbyIds.Add(friend.Id);

                    bool changed = false;

                    if (friend.Level != pc.Level) { friend.Level = pc.Level; changed = true; }
                    if (friend.JobId != pc.ClassJob.RowId) { friend.JobId = (byte)pc.ClassJob.RowId; changed = true; }

                    var tag = pc.CompanyTag.TextValue;
                    if (friend.FcTag != tag) { friend.FcTag = tag; changed = true; }

                    if (friend.CurrentWorldId != localCurrentWorld) {
                        friend.CurrentWorldId = localCurrentWorld;
                        changed = true;
                    }

                    unsafe {
                        var csChar = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)pc.Address;
                        if (csChar != null) {
                            if (friend.TitleId != csChar->TitleId) { friend.TitleId = csChar->TitleId; changed = true; }
                            if (friend.OnlineStatusId != csChar->CharacterData.OnlineStatus) { friend.OnlineStatusId = csChar->CharacterData.OnlineStatus; changed = true; }

                            byte race = csChar->DrawData.CustomizeData.Race;
                            byte tribe = csChar->DrawData.CustomizeData.Tribe;
                            byte gender = csChar->DrawData.CustomizeData.Sex;

                            if (friend.Race != 0 && (friend.Race != race || friend.Gender != gender)) {
                                friend.IsFantasiaDetected = true;
                                changed = true;
                            }

                            if (friend.Race != race) { friend.Race = race; changed = true; }
                            if (friend.Tribe != tribe) { friend.Tribe = tribe; changed = true; }
                            if (friend.Gender != gender) { friend.Gender = gender; changed = true; }
                        }
                    }

                    bool canUpdateLocation = hasServerLocation || !friend.IsActivelyTracked || friend.LocationId == 0;

                    if (canUpdateLocation && friend.LocationId != effectiveLocationId) {
                        friend.LocationId = effectiveLocationId;
                        changed = true;
                    }

                    if (!friend.IsOnline) { friend.IsOnline = true; changed = true; }

                    if ((now - friend.LastSeenAt).TotalMinutes > 5) {
                        friend.LastSeenAt = now;
                        changed = true;
                    }

                    if (changed) {
                        stateChanged = true;
                    }

                    if (!this.currentlyNearbyIds.Contains(friend.Id)) {
                        bool shouldNotify = (friend.IsActivelyTracked && config.NotifyOnNearbyFriends) ||
                                            (!friend.IsActivelyTracked && config.NotifyOnNearbyArchived);

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
        }

        this.currentlyNearbyIds = newNearbyIds;

        if (stateChanged) {
            this.registry.SaveMasterList();
            this.registry.NotifyDataChanged();
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