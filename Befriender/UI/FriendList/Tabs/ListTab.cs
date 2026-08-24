namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Numerics;

public class ListTab : ITab {
    private IFriendRepository friendRepository;
    private IFriendDisplayService displayService;
    private IFriendSyncService syncService;
    private IGameDataService gameDataService;
    private ITextureProvider textureProvider;
    private DateTime lastProcessedSyncTime = DateTime.MinValue;

    private bool showOnlineOnly = false;
    private IReadOnlyList<FriendProfile> cachedFriends = new List<FriendProfile>();
    private int lastFriendCount = -1;
    private bool forceRefresh = false;

    public string Name => "List";

    public ListTab(IFriendRepository friendRepository, IFriendDisplayService displayService, IFriendSyncService syncService, IGameDataService gameDataService, ITextureProvider textureProvider) {
        this.friendRepository = friendRepository;
        this.displayService = displayService;
        this.syncService = syncService;
        this.gameDataService = gameDataService;
        this.textureProvider = textureProvider;
    }

    public void Draw() {
        var rawFriends = this.friendRepository.GetFriends();

        if (rawFriends.Count == 0) {
            ImGui.Text("Your friend list is currently empty or syncing...");
            return;
        }

        float footerHeight = ImGui.GetFrameHeightWithSpacing();

        if (ImGui.BeginChild("TableChild", new Vector2(0, -footerHeight), false)) {
            if (ImGui.BeginTable("FriendsTable", 7, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable)) {
                ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("FC", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Added", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Last Seen", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableHeadersRow();

                this.HandleSortingAndFiltering(rawFriends);

                foreach (var friend in this.cachedFriends) {
                    ImGui.TableNextRow();

                    // 1. Colonne Statut (Pastille colorée)
                    ImGui.TableNextColumn();
                    var statusColor = friend.IsOnline
                        ? new Vector4(0.43f, 0.85f, 0.43f, 1.0f) // Vert vif
                        : new Vector4(0.4f, 0.4f, 0.4f, 1.0f);   // Gris terne

                    ImGui.TextColored(statusColor, "●");
                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(friend.IsOnline ? "Online" : "Offline");
                    }

                    // Début de l'atténuation (dimming) pour les joueurs hors-ligne
                    if (!friend.IsOnline) {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    }

                    // 2. Colonne Nom
                    ImGui.TableNextColumn();
                    ImGui.Text(friend.Name);

                    // 3. Colonne Job
                    ImGui.TableNextColumn();
                    if (friend.JobId > 0) {
                        var iconId = this.gameDataService.GetJobIconId(friend.JobId);
                        var jobAbbr = this.gameDataService.GetJobAbbreviation(friend.JobId);
                        bool iconDrawn = false;

                        if (iconId > 0) {
                            var iconLookup = new GameIconLookup { IconId = iconId };
                            var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                            if (iconWrap != null) {
                                var iconSize = new Vector2(24, 24);
                                // On applique une teinte sombre sur l'image si le joueur est hors-ligne
                                var imageTint = friend.IsOnline ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 1.0f);

                                ImGui.Image(iconWrap.Handle, iconSize, Vector2.Zero, Vector2.One, imageTint);

                                if (ImGui.IsItemHovered()) {
                                    ImGui.SetTooltip(jobAbbr);
                                }

                                iconDrawn = true;
                            }
                        }

                        if (!iconDrawn) {
                            ImGui.Text(jobAbbr);
                        }
                    }
                    else {
                        ImGui.Text(string.Empty);
                    }

                    // 4. Colonne FC
                    ImGui.TableNextColumn();
                    if (!string.IsNullOrEmpty(friend.FcTag)) {
                        ImGui.Text($"<{friend.FcTag}>");
                    }
                    else {
                        ImGui.Text(string.Empty);
                    }

                    // 5. Colonne Monde
                    ImGui.TableNextColumn();
                    ImGui.Text(this.gameDataService.GetWorldName(friend.HomeWorldId));

                    // 6. Colonne Ajout
                    ImGui.TableNextColumn();
                    var dateStr = friend.AddedAt == System.DateTime.MinValue ? "Unknown" : friend.AddedAt.ToShortDateString();
                    var locStr = this.gameDataService.GetLocationName(friend.AddedLocationId);
                    ImGui.Text($"{dateStr} ({locStr})");

                    // 7. Colonne Dernière connexion
                    ImGui.TableNextColumn();
                    if (friend.IsOnline) {
                        ImGui.Text("Online");
                    }
                    else if (friend.LastSeenAt == System.DateTime.MinValue) {
                        ImGui.Text("Unknown");
                    }
                    else {
                        var diff = System.DateTime.Now - friend.LastSeenAt;
                        if (diff.TotalDays >= 1) {
                            ImGui.Text($"{(int)diff.TotalDays}d");
                        }
                        else if (diff.TotalHours >= 1) {
                            ImGui.Text($"{(int)diff.TotalHours}h");
                        }
                        else if (diff.TotalMinutes >= 1) {
                            ImGui.Text($"{(int)diff.TotalMinutes}m");
                        }
                        else {
                            ImGui.Text("Just now");
                        }
                    }

                    if (!friend.IsOnline) {
                        ImGui.PopStyleColor();
                    }
                }

                ImGui.EndTable();
            }
            ImGui.EndChild();
        }

        ImGui.Separator();
        if (ImGui.Checkbox("Show Online Only", ref this.showOnlineOnly)) {
            this.forceRefresh = true;
        }

        ImGui.SameLine();
        var syncText = this.syncService.LastSyncTime == System.DateTime.MinValue
            ? "Syncing..."
            : $"Last Sync: {this.syncService.LastSyncTime.ToShortTimeString()}";

        ImGui.Text($"| {syncText} | Total: {rawFriends.Count}");
    }

    private void HandleSortingAndFiltering(IReadOnlyList<FriendProfile> rawFriends) {
        var sortSpecs = ImGui.TableGetSortSpecs();
        bool dataUpdated = this.syncService.LastSyncTime != this.lastProcessedSyncTime;

        if (sortSpecs.SpecsDirty || this.forceRefresh || rawFriends.Count != this.lastFriendCount || dataUpdated) {
            int sortColumn = -1;
            bool isAscending = true;

            if (sortSpecs.SpecsCount > 0) {
                var spec = sortSpecs.Specs[0];
                sortColumn = spec.ColumnIndex;
                isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;
            }

            this.cachedFriends = this.displayService.ProcessFriends(rawFriends, this.showOnlineOnly, sortColumn, isAscending);

            sortSpecs.SpecsDirty = false;
            this.forceRefresh = false;
            this.lastFriendCount = rawFriends.Count;
            this.lastProcessedSyncTime = this.syncService.LastSyncTime;
        }
    }
}