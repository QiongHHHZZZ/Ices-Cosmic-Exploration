using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Utilities;

public static partial class CosmicHelper
{
    public class CustomNotes
    {
        public string NoteInfo { get; set; }
        public float SPM { get; set; }
    };

    public static Dictionary<uint, CustomNotes> CustomMissionNotes =
        CreateMissionNotes();

    private static Dictionary<uint, CustomNotes> CreateMissionNotes()
    {
        var dict = new Dictionary<uint, CustomNotes>();

        // Dual Class Missions
        AddMissions(dict, 256f, "I would HIGHLY recommend doing these as you first start out.\n" +
                                "Knocks out 2 classes at once, allowing you to double dip and get done quicker.\n" +
                                "Make sure the turnin is on silver, gold isn't worth the extra time" +
                                "Also, best for A Rank level on Sinus",
                                496, 497, 498, 502, 503, 504);

        AddMissions(dict, 228f, "Best fishing mission.... period. Which sucks.\n" +
                                "Turnin on silver for best results. Don't do the other dual class, it's not worth",
                                509);

        // Basic Sinus A Ranks
        AddMissions(dict, 241f, "Best Score Per Minute outside of weather missions",
                    295, 115, 70, 205, 25, 340, 160, 250);

        // Sinus Weather Missions
        AddMissions(dict, 490f,
                   "Best Score Per Minute on Sinus. Ideally you would want to focus these as much as possible.\n" +
                   "Aim to complete these on silver, that's the best SPM you'll get with the lv. 100 gear",
                    31, 32, 76, 77, 121, 122, 166, 167, 211, 212, 256, 257, 301, 302, 346, 347);

        // Sinus A Rank Missions
        AddMissions(dict, 207f,
                   "If you're not doing the dual craft missions (you should be, this is worse), then this is the 2nd best \n" +
                   "Aim for silver or gold on this, they average about the same",
                   387, 432);

        // Sinus Critical Missions
        AddMissions(dict, 406f,
                    "Always aim to do criticals when you can for score",
                    536, 537, 538, 539, 540, 541, 542, 543, 544);

        // Basic Phaenna
        AddMissions(dict, 336f, "Best Score Per Minute out of weather missions.",
                    569, 611, 653, 695, 737, 779, 821, 863);

        // Phaenna Sequence
        AddMissions(dict, 362f,
                   "This set of sequence missions is the best if you're chaining missions \nThis averages out to being better than a normal mission.",
                    580, 622, 664, 706, 748, 790, 832, 874);

        // Phaenna Weather
        AddMissions(dict, 281f,
                   "Best Weather missions that are here, not really worth doing over the basic missions IMHO. But The info is here for you",
                   573, 615, 657, 699, 741, 783, 825, 867);

        AddMissions(dict, 379f,
                   "Criticals are always worth doing for score",
                    1007, 1008, 1009, 1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017, 1018, 
                    1019, 1020, 1021, 1022, 1023, 1024, 1025, 1026, 1027, 1028, 1029, 1030);

        AddMissions(dict, 211f,
                    "Best general gathering ones to do on Phaenna, not better than dual class.\n" +
                    "But if you're sticking around for weather missions, might as well do this.",
                    909, 951);

        AddMissions(dict, 437f,
                    "Really good weather missions for scoring, if these are up, you typically want to aim to do them.",
                    917, 959);

        AddMissions(dict, 392f,
                    "Second best weather missions for scoring, still good to focus over the basic A Ranks",
                    896, 938);



        return dict;
    }

    private static void AddMissions(Dictionary<uint, CustomNotes> dict, float spm, string note, params uint[] missionIds)
    {
        foreach (var id in missionIds)
        {
            dict[id] = new CustomNotes { SPM = spm, NoteInfo = note };
        }
    }

    public static List<uint> QuickLevelList = new()
    {
        // Sinus
        3, 8, 19,      // CRP
        48, 53, 64,    // ARM
        93, 98, 109,   // BSM
        138, 143, 154, // GSM
        183, 188, 199, // LTW
        228, 233, 244, // WVR
        273, 278, 289, // ALC
        318, 323, 334, // CUL
        365, 369, 374, // MIN
        410, 414, 419, // BTN
        453, 458, 465, // FHS


        // Phaenna // 1 2 2
        545, 556, 561, // CRP
        587, 598, 603, // BSM
        629, 640, 645, // ARM
        671, 682, 687, // GSM
        713, 724, 729, // LTW
        755, 766, 771, // WVR
        797, 808, 813, // ALC
        839, 850, 855, // CUL
        883, 903, 886, // MIN
        925, 945, 928, // BTN
        967, 973, 979, // FSH

        1040, 1045, 1048, 
        1068, 1073, 1076, 
        1096, 1101, 1108, 
        1124, 1129, 1136, 
        1152, 1157, 1160, 
        1180, 1185, 1188, 
        1208, 1213, 1216, 
        1236, 1241, 1244, 
        1266, 1270, 1274, 
        1294, 1298, 1301, 
        1321, 1327, 1331,
    };

    public static List<uint> Unlock_MissionList = new()
    {
        // Sinus

        // CRP
        1, 2, 3, 4, 5, 8, 9, 11, 12, 13,
        // BSM
        46, 47, 48, 49, 50, 53, 54, 56, 57, 58,
        // ARM
        91, 92, 93, 94, 95, 98, 99, 101, 102, 103,
        // GSM
        136, 137, 138, 139, 140, 143, 144, 146, 147, 148,
        // LTW
        181, 182, 183, 184, 185, 188, 189, 191, 192, 193,
        // WVR
        226, 227, 228, 229, 230, 233, 234, 236, 237, 238,
        // ALC
        271, 272, 273, 274, 275, 278, 279, 281, 282, 283,
        // CUL
        316, 317, 318, 319, 320, 323, 324, 326, 327, 328,
        // MIN
        361, 362, 365, 366, 367, 369, 370, 371, 372,
        // BTN
        406, 407, 408, 409, 410, 411, 412, 413, 414, 415, 416, 417,
        // FSH
        451, 452, 453, 454, 456, 457, 458, 459, 460, 461, 463,

        // Phaenna
        // CRP
        545, 546, 547, 548, 549, 552, 553, 555, 556, 557,
        // BSM
        587, 588, 589, 590, 591, 594, 595, 597, 598, 599,
        // ARM
        629, 630, 631, 632, 633, 636, 637, 639, 640, 641,
        // GSM
        671, 672, 673, 674, 675, 678, 679, 681, 682, 683,
        // LTW
        713, 714, 715, 716, 717, 720, 721, 723, 724, 725,
        // WVR
        755, 756, 757, 758, 759, 762, 763, 765, 766, 767,
        // ALC
        797, 798, 799, 800, 801, 804, 805, 807, 808, 809,
        // CUL
        839, 840, 841, 842, 843, 846, 847, 849, 850, 851,
        // MIN
        881, 882, 883, 889, 890, 902, 903, 904, 911, 912,
        // BTN
        923, 924, 925, 930, 931, 932, 944, 945, 946, 947, 953, 954,
        // FSH
        965, 967, 968, 970, 971, 972, 973, 974, 975, 977,

        // Oizys
        1040, 1041, 1042, 1043, 1044, 1045, 1046, 1047, 
        1068, 1069, 1070, 1071, 1072, 1073, 1074, 1075, 
        1096, 1097, 1098, 1099, 1100, 1101, 1102, 1103, 
        1124, 1125, 1126, 1127, 1128, 1129, 1130, 1131, 
        1152, 1153, 1154, 1155, 1156, 1157, 1158, 1159, 
        1180, 1181, 1182, 1183, 1184, 1185, 1186, 1187, 
        1208, 1209, 1210, 1211, 1212, 1213, 1214, 1215, 
        1236, 1237, 1238, 1239, 1240, 1241, 1242, 1243, 
        1264, 1265, 1266, 1267, 1268, 1269, 1270, 1271, 
        1292, 1293, 1294, 1295, 1296, 1297, 1298, 1299,
    };

    public class LevelInfo
    {
        public uint Level { get; set; } = 10;
        public List<uint> MissionId { get; set; } = new();
    }
}
