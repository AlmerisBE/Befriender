namespace Befriender.UI.FriendList.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.UI.FriendList.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendSearchService : IFriendSearchService {
    private IFriendTagRepository tagRepository;

    public FriendSearchService(IFriendTagRepository tagRepository) {
        this.tagRepository = tagRepository;
    }

    public IReadOnlyList<FriendProfile> FilterFriends(IEnumerable<FriendProfile> friends, string searchQuery) {
        if (string.IsNullOrWhiteSpace(searchQuery)) {
            return friends.ToList();
        }

        var query = searchQuery.Trim();
        var allTags = this.tagRepository.GetTags();

        return friends.Where(f => {
            if (f.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }

            var friendTags = allTags.Where(t => f.Tags.Contains(t.Id));
            if (friendTags.Any(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase))) {
                return true;
            }

            return false;
        }).ToList();
    }
}