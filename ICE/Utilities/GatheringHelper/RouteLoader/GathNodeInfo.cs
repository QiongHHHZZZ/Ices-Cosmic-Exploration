using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ICE.Utilities.GatheringHelper.RouteLoader;

public class GatheringRoute
{
    [JsonPropertyName("route_id")]
    public uint RouteId { get; set; }

    [JsonPropertyName("territory_id")]
    public uint TerritoryId { get; set; }

    [JsonPropertyName("gathering_job_id")]
    public uint GatheringJobId { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("date_modified")]
    public DateTime? DateModified { get; set; }

    [JsonPropertyName("node")]
    public List<NodeInfo> Nodes { get; set; } = new();
}

public class NodeInfo
{
    [JsonPropertyName("node_id")]
    public uint NodeId { get; set; }

    [JsonPropertyName("position")]
    public Vector3 Position { get; set; }

    [JsonPropertyName("land_zone")]
    public Vector3 LandZone { get; set; }

    [JsonPropertyName("radius_start")]
    public float RadiusStart { get; set; } = 0f;

    [JsonPropertyName("radius_end")]
    public float RadiusEnd { get; set; } = 359f;

    [JsonPropertyName("min_distance")]
    public float MinDistance { get; set; } = 1f;

    [JsonPropertyName("max_distance")]
    public float MaxDistance { get; set; } = 3f;

    [JsonPropertyName("fan_height")]
    public float FanHeight { get; set; } = 0f;
}
