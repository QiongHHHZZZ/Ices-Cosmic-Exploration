using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Utilities.Cosmic_Helper;

public static partial class CosmicHelper
{
    public class CustomNotes
    {
        public string NoteInfo { get; set; } = "";
        public float SPM { get; set; } = 0;
    };

    // public static Dictionary<uint, CustomNotes> CustomMissionNotes = CreateMissionNotes();

    public static void CreateMissionNotes()
    {
        var notesDictonary = new Dictionary<uint, CustomNotes>();

        // Dual Class Missions
        AddMissions(notesDictonary, 256f, 
            "I would HIGHLY recommend doing these as you first start out.\n" +
            "Knocks out 2 classes at once, allowing you to double dip and get done quicker.\n" +
            "Make sure the turnin is on silver, gold isn't worth the extra time" +
            "Also, best for A Rank level on Sinus",
            496, 497, 498, 502, 503, 504);

        AddMissions(notesDictonary, 228f, "" +
            "Best fishing mission.... period. Which sucks.\n" +
            "Turnin on silver for best results. Don't do the other dual class, it's not worth\n" +
            "Recommend Raphael Solver",
            509);

        // Basic Sinus A Ranks
        AddMissions(notesDictonary, 241f, 
            "Best Score Per Minute outside of weather missions",
            295, 115, 70, 205, 25, 340, 160, 250);

        // Sinus Weather Missions
        AddMissions(notesDictonary, 490f,
            "Best Score Per Minute on Sinus. Ideally you would want to focus these as much as possible.\n" +
            "Aim to complete these on silver, that's the best SPM you'll get with the lv. 100 gear",
            31, 32, 76, 77, 121, 122, 166, 167, 211, 212, 256, 257, 301, 302, 346, 347);

        // Sinus A Rank Missions
        AddMissions(notesDictonary, 207f,
            "If you're not doing the dual craft missions (you should be, this is worse), then this is the 2nd best \n" +
            "Aim for silver or gold on this, they average about the same",
            387, 432);

        // Sinus Critical Missions
        AddMissions(notesDictonary, 406f,
            "Always aim to do criticals when you can for score",
            536, 537, 538, 539, 540, 541, 542, 543, 544);

        // Basic Phaenna
        AddMissions(notesDictonary, 336f, 
            "Best Score Per Minute out of weather missions.\n" +
            "Raphael Solver is a hard requirement to reach this",
            569, 611, 653, 695, 737, 779, 821, 863);

        // Phaenna Sequence
        AddMissions(notesDictonary, 362f,
            "This set of sequence missions is the best if you're chaining missions \n" +
            "This averages out to being better than a normal mission.",
            580, 622, 664, 706, 748, 790, 832, 874);

        // Phaenna Weather
        AddMissions(notesDictonary, 281f,
            "Best Weather missions that are here, not really worth doing over the basic missions IMHO. But The info is here for you",
            573, 615, 657, 699, 741, 783, 825, 867);

        AddMissions(notesDictonary, 379f,
            "Criticals are always worth doing for score",
            1007, 1008, 1009, 1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017, 1018, 
            1019, 1020, 1021, 1022, 1023, 1024, 1025, 1026, 1027, 1028, 1029, 1030);

        AddMissions(notesDictonary, 211f,
            "Best general gathering ones to do on Phaenna, not better than dual class.\n" +
            "But if you're sticking around for weather missions, might as well do this.",
            909, 951);

        AddMissions(notesDictonary, 437f,
            "Really good weather missions for scoring, if these are up, you typically want to aim to do them.",
            917, 959);

        AddMissions(notesDictonary, 392f,
            "Second best weather missions for scoring, still good to focus over the basic A Ranks",
            896, 938);

        // Auxesia (1370+): add AddMissions(...) blocks when you have SPM numbers from in-zone runs.
        // Unlock / quick-level IDs are built automatically — see CosmicMissionLists.cs.
        // If the sheet misses rows, use ManualUnlockAdditions / ManualQuickLevelAdditions there.

        foreach (var mission in notesDictonary)
        {
            SheetMissionDict[mission.Key].BestSPM = mission.Value;
        }
    }

    private static void AddMissions(Dictionary<uint, CustomNotes> dict, float spm, string note, params uint[] missionIds)
    {
        foreach (var id in missionIds)
        {
            dict[id] = new CustomNotes { SPM = spm, NoteInfo = note };
        }
    }

    /// <summary>Filled by CosmicMissionLists.BuildFromSheet() during startup — not a hand-edited list anymore.</summary>
    public static IEnumerable<uint> QuickLevelList => CosmicMissionLists.QuickLevelList;

    /// <summary>Filled by CosmicMissionLists.BuildFromSheet() during startup — not a hand-edited list anymore.</summary>
    public static IEnumerable<uint> Unlock_MissionList => CosmicMissionLists.UnlockMissionList;

    public class LevelInfo
    {
        public uint Level { get; set; } = 10;
        public List<uint> MissionId { get; set; } = new();
    }
}
