using System.Collections.Generic;

namespace ICE.Utilities.Cosmic
{
    /// <summary>
    /// IDs of missions that should be disabled and shown as unsupported.
    /// </summary>
    public static class UnsupportedMissions
    {
        public static readonly HashSet<uint> Ids = new HashSet<uint>
        {
            0,

            // Oizyr Missions
        };
    }
}
