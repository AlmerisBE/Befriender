namespace Befriender.Core.Proximity.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
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

    private HashSet<Guid> currentlyNearbyIds = new();
    private readonly Guid proximitySourceId = Guid.Parse("51000000-0000-0000-0000-000000000003");

    public ProximityService(ICharacterRegistry registry, IConfigurationService configService, INotificationManager notificationManager, ILocalizationService loc) {
        this.registry = registry;
        this.configService = configService;
        this.notificationManager = notificationManager;
        this.loc = loc;

        this.registry.RegistryUpdated += this.OnRegistryUpdated;
    }

    private void OnRegistryUpdated() {
        var config = this.configService.GetConfig();
        if (!config.EnableProximityDetection) {
            if (this.currentlyNearbyIds.Count > 0) {
                this.currentlyNearbyIds.Clear();
            }

            return;
        }

        var allCharacters = this.registry.GetAllCharacters();
        var newNearbyIds = new HashSet<Guid>();

        foreach (var character in allCharacters) {
            if (character.ActiveSourceIds.Contains(this.proximitySourceId)) {
                newNearbyIds.Add(character.Id);

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
    }

    public bool IsFriendNearby(ulong contentId) {
        var character = this.registry.GetAllCharacters().FirstOrDefault(c => c.ContentId == contentId);
        return character != null && character.ActiveSourceIds.Contains(this.proximitySourceId);
    }

    public IReadOnlyList<ulong> GetNearbyFriendIds() {
        return this.registry.GetAllCharacters()
            .Where(c => c.ActiveSourceIds.Contains(this.proximitySourceId))
            .Select(c => c.ContentId)
            .ToList();
    }

    public void Dispose() {
        this.registry.RegistryUpdated -= this.OnRegistryUpdated;
    }
}