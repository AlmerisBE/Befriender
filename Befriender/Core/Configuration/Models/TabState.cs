namespace Befriender.Core.Configuration.Models;

using System;

[Serializable]
public class TabState {
    public bool ShowOnlineOnly { get; set; }
    public int StatusFilter { get; set; } = 0;
    public bool ShowNearbyOnly { get; set; }
    public bool GroupByGroups { get; set; }
    public bool IsFiltersExpanded { get; set; }
}