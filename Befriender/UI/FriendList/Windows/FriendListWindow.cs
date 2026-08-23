namespace Befriender.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.UI.FriendList.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface; // Required for FontAwesomeIcon
using Dalamud.Interface.Windowing;
using System.Collections.Generic;
using System.Numerics;

public class FriendListWindow : Window {
    private IFriendRepository friendRepository;
    private IFriendDisplayService displayService;
    private IFriendSyncService syncService;
    private bool showOnlineOnly = false;

    private IReadOnlyList<FriendProfile> cachedFriends = new List<FriendProfile>();
    private int lastFriendCount = -1;
    private bool forceRefresh = false;

    public FriendListWindow(IFriendRepository friendRepository, IFriendDisplayService displayService, IFriendSyncService syncService) : base("Befriender - Friend List", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        this.friendRepository = friendRepository;
        this.displayService = displayService;
        this.syncService = syncService;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(500, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        // Add the manual sync button to the title bar
        this.TitleBarButtons.Add(new TitleBarButton {
            Icon = FontAwesomeIcon.Sync,
            IconOffset = new Vector2(1, 1),
            Click = (mouseButton) => this.syncService.ForceSync()
        });
    }

    public override void Draw() {
        var rawFriends = this.friendRepository.GetFriends();

        if (rawFriends.Count == 0) {
            ImGui.Text("Your friend list is currently empty or syncing...");
            return;
        }

        if (ImGui.Checkbox("Show Online Only", ref this.showOnlineOnly)) {
            this.forceRefresh = true;
        }

        // We enable sorting on the table
        if (ImGui.BeginTable("FriendsTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable)) {
            ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("FC", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            this.HandleSortingAndFiltering(rawFriends);

            foreach (var friend in this.cachedFriends) {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text(friend.IsOnline ? "Online" : "Offline");

                ImGui.TableNextColumn();
                ImGui.Text(friend.Name);

                ImGui.TableNextColumn();
                ImGui.Text(friend.JobId.ToString());

                ImGui.TableNextColumn();
                if (!string.IsNullOrEmpty(friend.FcTag)) {
                    ImGui.Text($"<{friend.FcTag}>");
                }
                else {
                    ImGui.Text(string.Empty);
                }

                ImGui.TableNextColumn();
                ImGui.Text(friend.HomeWorldId.ToString());
            }

            ImGui.EndTable();
        }
    }

    private void HandleSortingAndFiltering(IReadOnlyList<FriendProfile> rawFriends) {
        var sortSpecs = ImGui.TableGetSortSpecs();

        if (sortSpecs.SpecsDirty || this.forceRefresh || rawFriends.Count != this.lastFriendCount) {
            int sortColumn = -1;
            bool isAscending = true;

            // We use SpecsCount provided by the ImGuiTableSortSpecs wrapper
            if (sortSpecs.SpecsCount > 0) {
                // ImGuiTableColumnSortSpecsPtr acts as an array pointer, so we can use the indexer
                var spec = sortSpecs.Specs[0];
                sortColumn = spec.ColumnIndex;
                isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;
            }

            this.cachedFriends = this.displayService.ProcessFriends(rawFriends, this.showOnlineOnly, sortColumn, isAscending);

            sortSpecs.SpecsDirty = false;
            this.forceRefresh = false;
            this.lastFriendCount = rawFriends.Count;
        }
    }
}