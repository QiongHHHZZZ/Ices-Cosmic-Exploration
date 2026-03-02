using System;
using System.Collections.Generic;
using System.Numerics;
using YamlDotNet.Serialization;

namespace ICE.Resources.GatheringRoutes;

public class GathNodeInfo
{
    [YamlMember(Alias = "node_id")]
    public uint NodeId { get; set; }

    [YamlMember(Alias = "position")]
    public Vector3 Position { get; set; }

    [YamlMember(Alias = "land_zone")]
    public Vector3 LandZone { get; set; }

    [YamlMember(Alias = "radius_start")]
    public float Radius_Start { get; set; } = 0.0f;

    [YamlMember(Alias = "radius_end")]
    public float Radius_End { get; set; } = 360.0f;

    [YamlMember(Alias = "min_distance")]
    public float Distance_Min { get; set; } = 1.0f;

    [YamlMember(Alias = "max_distance")]
    public float Distance_Max { get; set; } = 3.0f;
    public float FanHeight { get; set; } = 0;
}

public class GatheringRouteFile
{
    [YamlMember(Alias = "zone_id")]
    public uint ZoneId { get; set; }

    [YamlMember(Alias = "zone_name")]
    public string ZoneName { get; set; } = string.Empty;

    [YamlMember(Alias = "job")]
    public string Job { get; set; } = string.Empty;  // "MIN" or "BTN"

    [YamlMember(Alias = "flag")]
    public Vector2 Flag { get; set; }

    [YamlMember(Alias = "author")]
    public string? Author { get; set; }

    [YamlMember(Alias = "date_modified")]
    public DateTime? DateModified { get; set; }

    [YamlMember(Alias = "nodes")]
    public List<GathNodeInfo> Nodes { get; set; } = new();
}