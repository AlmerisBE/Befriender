namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.GameData.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterSearchService : ICharacterSearchService {
    private ICharacterTagRepository tagRepository;
    private ICharacterGroupRepository groupRepository;
    private IGameDataService gameDataService;

    public CharacterSearchService(
        ICharacterTagRepository tagRepository,
        ICharacterGroupRepository groupRepository,
        IGameDataService gameDataService) {

        this.tagRepository = tagRepository;
        this.groupRepository = groupRepository;
        this.gameDataService = gameDataService;
    }

    public IEnumerable<Character> FilterCharacters(IEnumerable<Character> characters, string searchQuery) {
        if (string.IsNullOrWhiteSpace(searchQuery)) return characters;

        var query = searchQuery.ToLowerInvariant();
        var allTags = this.tagRepository.GetTags();
        var allGroups = this.groupRepository.GetGroups();

        return characters.Where(m => {
            // 1. Current Name
            if (!string.IsNullOrEmpty(m.Name) && m.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;

            // 2. Previous Names
            if (m.PreviousNames != null && m.PreviousNames.Any(n => n.Contains(query, StringComparison.OrdinalIgnoreCase))) return true;

            // 3. Notes
            if (!string.IsNullOrEmpty(m.Notes) && m.Notes.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;

            // 4. Free Company Tag
            if (!string.IsNullOrEmpty(m.FcTag) && m.FcTag.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;

            // 5. Group Title
            if (m.CustomGroupId.HasValue) {
                var group = allGroups.FirstOrDefault(g => g.Id == m.CustomGroupId.Value);
                if (group != null && group.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;
            }

            // 6. Tags
            var characterTags = allTags.Where(t => m.Tags.Contains(t.Id));
            if (characterTags.Any(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase))) return true;

            // 7. Location (Display Name & Raw Name)
            var displayLocation = this.gameDataService.GetDisplayLocation(m.LocationId, m.CurrentWorldId, m.HomeWorldId, m.OnlineStateMask);
            if (!string.IsNullOrEmpty(displayLocation) && displayLocation.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;

            var locationName = this.gameDataService.GetLocationName(m.LocationId);
            if (!string.IsNullOrEmpty(locationName) && locationName.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        });
    }
}