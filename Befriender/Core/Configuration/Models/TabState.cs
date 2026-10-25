namespace Befriender.Core.Configuration.Models;

using System;
using System.Collections.Generic;

[Serializable]
public class TabState {
    public bool ShowOnlineOnly { get; set; }
    public int StatusFilter { get; set; } = 0;
    public bool ShowNearbyOnly { get; set; }
    public bool GroupByGroups { get; set; }
    public bool IsFiltersExpanded { get; set; }

    // Default columns matching the original layout
    public List<ColumnType> ActiveColumns { get; set; } = new() {
        ColumnType.Status,
        ColumnType.Name,
        ColumnType.Job,
        ColumnType.Location
    };
}