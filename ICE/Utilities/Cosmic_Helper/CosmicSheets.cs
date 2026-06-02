using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;
using static MissionTimer;

namespace ICE.Utilities.Cosmic_Helper;

public static unsafe partial class CosmicHelper
{
    public class CraftingInfo
    {
        public uint ItemId { get; set; }
        public int RequiredAmount { get; set; }
        public uint RecipeId { get; set; }
        public RecipeInfo RecipeInfo { get; set; } = new();
        public bool ExpertCraft { get; set; } = false;
        public Dictionary<uint, int> RequiredItems { get; set; } = new();
        public int IconId { get; set; } = 0;
        public string ItemName { get; set; } = "???";
    }
    public enum Status
    {
        None,
        Completed,
        Gold,
    }
    public class CosmicInfo
    {
        public uint MissionId { get; set; } = 0;
        // - - - Crafter Specific - - - //
        /// <summary>
        /// Key = What's used in the config per recipe. This keeps track of it on a per-recipe basis
        /// </summary>
        public Dictionary<ushort, CraftingInfo> Crafts_Main { get; set; } = new();
        public Dictionary<ushort, CraftingInfo> Crafts_Pre { get; set; } = new();
        public bool IsExpert { get; set; } = false;

        // - - - BTN | MIN Specific - - - //
        public Dictionary<uint, int> Gathering_Min { get; set; } = new();

        // - - - FSH Specific - - - //
        public int Fish_AmountRequired { get; set; } = 0;
        public int Fish_VarietyAmount { get; set; } = 0;
        public List<string> Fish_Presets { get; set; } = new();

        // - - - Map Related - - - // 
        public Vector2 MapPosition { get; set; } = new();
        public int Radius { get; set; } = 0;
        public uint TerritoryId { get; set; }
        public uint MarkerId { get; set; }

        // - - - Exp Modifier Section - - - // 

        public uint ExpModifier_1 { get; set; } = 0;
        public uint ExpModifier_2 { get; set; } = 0;
        public uint ExpModifier_3 { get; set; } = 0;

        // - - - Universal Info - - - //
        public string Name { get; set; }
        public List<uint> Jobs { get; set; } = new();
        public uint ToDoId { get; set; } = 0;
        public uint Rank { get; set; } = 1;
        public uint Level { get; set; } = 0;
        public MissionAttributes Attributes { get; set; }
        public CosmicWeather Weather { get; set; }
        public uint StartTime { get; set; }
        public uint EndTime { get; set; }
        public uint ClassScore { get; set; } = 0;
        public uint CosmoCredit { get; set; } = 0;
        public uint LunarCredit { get; set; } = 0;
        public uint TokenItemId { get; set; } = 0;
        public uint TokenItemAmount { get; set; } = 0;
        public uint DronebitReward { get; set; } = 0;
        public uint PreviousMissionId { get; set; } = new();
        public Dictionary<int, int> RelicXpInfo { get; set; } = new();
        public uint BronzeScore { get; set; } = 0;
        public uint SilverScore { get; set; } = 0;
        public uint GoldScore { get; set; } = 0;
        public uint TemporaryActionId { get; set; } = 0;
        public uint TemporaryActionCount { get; set; } = 0;
        public Status CompletionStatus { get; set; } = Status.None;
        public List<uint> SequenceMissions_Previous { get; set; } = new();
        public List<uint> SequenceMissions_Next { get; set; } = new();
        public Dictionary<TurninState, RewardInfo> ScoreInfo()
        {
            Dictionary<TurninState, RewardInfo> reward = new();
            if (!C.MissionConfig.TryGetValue(MissionId, out var config))
                return reward;

            Dictionary<TurninState, (int Multiplier, double AverageTime, int TotalCompleted)> stateInfo = new()
            {
                [TurninState.Critical] = (1, config.AverageTime, config.CriticalCompletions),
                [TurninState.Bronze] = (1, config.AverageBronzeTime, config.BronzeCompletion),
                [TurninState.Silver] = (4, config.AverageSilverTime, config.SilverCompletions),
                [TurninState.Gold] = (5, config.AverageGoldTime, config.GoldCompletions),
                [TurninState.SequenceGold] = new()
            };

            foreach (var (state, info) in stateInfo)
            {
                if (IsCritical && state != TurninState.Critical)
                    continue;
                else if (!IsCritical && state == TurninState.Critical)
                    continue;
                else if (state == TurninState.SequenceGold)
                {
                    if (!IsSequence || SequenceMissions_Previous.Count() == 0)
                        continue;
                    reward[TurninState.SequenceGold] = CalculateSequenceGoldSPM(MissionId);
                    continue;
                }


                reward[state] = new RewardInfo
                {
                    Completions = info.TotalCompleted,
                    Score = CalculatePerMinute(info.AverageTime, ClassScore, info.Multiplier),
                    Cosmocredit = CalculatePerMinute(info.AverageTime, CosmoCredit, info.Multiplier),
                    PlanetCredits = CalculatePerMinute(info.AverageTime, LunarCredit, info.Multiplier),
                    Tokens = CalculatePerMinute(info.AverageTime, TokenItemAmount, info.Multiplier)
                };
            }

            return reward;
        }
        public CustomNotes BestSPM { get; set; } = new();
        public List<uint> MissionUnlock { get; set; } = new();

        public bool IsProvisional => Attributes.HasFlag(MissionAttributes.ProvisionalWeather)
            || Attributes.HasFlag(MissionAttributes.ProvisionalSequential)
            || Attributes.HasFlag(MissionAttributes.ProvisionalTimed);

        public bool IsCritical => Attributes.HasFlag(MissionAttributes.Critical);
        public bool IsWeather => Attributes.HasFlag(MissionAttributes.ProvisionalWeather);
        public bool IsTimed => Attributes.HasFlag(MissionAttributes.ProvisionalTimed);
        public bool IsSequence => Attributes.HasFlag(MissionAttributes.ProvisionalSequential);
        public bool ARank => Rank is 5 or 4;
        public bool BRank => Rank is 3;
        public bool CRank => Rank is 2;
        public bool Drank => Rank == 1 && !Attributes.HasFlag(MissionAttributes.Critical);

    }
    public static Dictionary<uint, CosmicInfo> SheetMissionDict = new();
    public class RewardInfo
    {
        public int Completions { get; set; } = 0;
        public double Score { get; set; } = 0;
        public double Cosmocredit { get; set; } = 0;
        public double PlanetCredits { get; set; } = 0;
        public double Tokens { get; set; } = 0;
    }
    public class MissionInfo
    {
        public uint Id { get; set; } = 0;
        public bool Enabled => C.MissionConfig[Id].Enabled;
        public CosmicInfo SheetInfo => SheetMissionDict[Id];
    }
    private static double CalculatePerMinute(double averageTime, uint score, int multiplier)
    {
        if (averageTime <= 0) return 0;
        return (60 * score * multiplier) / averageTime;
    }
    private static RewardInfo CalculateSequenceGoldSPM(uint missionId)
    {
        if (!SheetMissionDict.TryGetValue(missionId, out var root))
            return new RewardInfo();

        // Build the full chain: previous missions + this one
        var chain = root.SequenceMissions_Previous
            .Select(id => SheetMissionDict.TryGetValue(id, out var m) ? m : null)
            .Append(root)
            .ToList();

        // If any mission in the chain has no gold completions, sequence gold is meaningless
        if (chain.Any(m => m == null
            || !C.MissionConfig.TryGetValue(m.MissionId, out var cfg)
            || cfg.GoldCompletions == 0))
            return new RewardInfo { Score = 0, Cosmocredit = 0, PlanetCredits = 0, Tokens = 0 };

        double totalTime = 0;
        double totalScore = 0;
        double totalCosmo = 0;
        double totalPlanet = 0;
        double totalTokens = 0;

        foreach (var mission in chain)
        {
            if (!C.MissionConfig.TryGetValue(mission.MissionId, out var config))
                return new RewardInfo();

            totalTime += config.AverageGoldTime;
            totalScore += mission.ClassScore * 5;
            totalCosmo += mission.CosmoCredit * 5;
            totalPlanet += mission.LunarCredit * 5;
            totalTokens += mission.TokenItemAmount * 5;
        }

        return new RewardInfo
        {
            Completions = C.MissionConfig[missionId].GoldCompletions,
            Score = CalculatePerMinute(totalTime, (uint)totalScore, 1),
            Cosmocredit = CalculatePerMinute(totalTime, (uint)totalCosmo, 1),
            PlanetCredits = CalculatePerMinute(totalTime, (uint)totalPlanet, 1),
            Tokens = CalculatePerMinute(totalTime, (uint)totalTokens, 1),
        };
    }
}
