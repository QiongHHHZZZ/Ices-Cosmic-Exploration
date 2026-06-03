namespace ICE.Utilities.Cosmic_Helper;

/// <summary>
/// WKSMissionUnit row-id bands per moon release (legacy fallback + hand-authored lists).
/// </summary>
public static class CosmicMissionBlocks
{
    public const uint SinusStart = 1;
    public const uint PhaennaStart = 545;
    public const uint OizysStart = 1040;
    public const uint AuxesiaStart = 1370;

    /// <summary>First row ID after the last known Auxesia band (exclusive upper bound for legacy resolver).</summary>
    public const uint UnknownMissionStart = 1703;

    /// <summary>Row-id offset between Oizys and Auxesia mission blocks (1040 + 330 = 1370).</summary>
    public const uint OizysToAuxesiaOffset = 330;
}
