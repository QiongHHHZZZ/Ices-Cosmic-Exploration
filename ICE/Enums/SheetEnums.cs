namespace ICE.Enums
{
    [Flags]
    public enum MissionAttributes
    {
        None = 0,                  // Impossible to have no attributes. If None - we failed to parse the mission.

        // --- Activity Type ---
        Craft = 1 << 0,                  //
        Gather = 1 << 1,                 //
        Fish = 1 << 2,                   //

        // --- Constraints ---
        Limited = 1 << 3,                // Supplies/Nodes/Bait

        // --- Item/Focus Type ---
        Collectables = 1 << 4,           // Collectables
        ReducedItems = 1 << 5,           // Reduction
        ExpertCraft = 1 << 6,            // Expert Craft

        // --- Scoring Method ---
        Score_TimeRemaining = 1 << 7,    // Time Attack (Speeeeeeed)
        Score_Chain = 1 << 8,            // Chain Bonus
        Score_Boon = 1 << 9,             // Gatherers Boon
        Score_LargestSize = 1 << 10,     // Largest fish caught
        Score_Variety = 1 << 11,         // Fish Variety
        Score_MinimumScore = 1 << 12,    // Minimum Score Requirement

        // --- Misc ---
        Critical = 1 << 13,              // Critical Mission
        ProvisionalTimed = 1 << 14,      // Timed Mission
        ProvisionalWeather = 1 << 15,    // Weather Mission
        ProvisionalSequential = 1 << 16, // Sequential Mission

        GreaterReach_GatherX = 1 << 17,  // Greater Reach + Gather 100 Items
        GreaterReach_Chain = 1 << 18,    // Greater Reach + Chain Scoring
        GreaterReach_Boon = 1 << 19,     // Greater Reach + Boon Scoring
        GreaterReach_Boon_Chain = 1 << 20, // Greater Reach + Chain + Boon Scoring
    }
}