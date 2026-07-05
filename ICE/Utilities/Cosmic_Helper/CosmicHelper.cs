using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Enums;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using static ICE.ConfigFiles.Config;
using static ICE.Localization.L10n;

namespace ICE.Utilities.Cosmic_Helper;

public static unsafe partial class CosmicHelper
{
    public static readonly int MinimumLevel = 10;
    public static readonly int MaximumLevel = Player.MaxLevel;
    public static readonly float ImageSize = 24;

    // Relic XP bar cap (highest stage across all moons + overcap headroom).
    public static readonly float MaxRelicExpStatus = CosmicMoonRegistry.MaxRelicExpBarCap;

    // Shared cosmo credits (45690) — not the per-planet gamba tokens. Use this instead of hardcoding the ID.
    public const uint CosmoCreditItemId = 45690;



    public static readonly List<uint> CrafterJobList = [8, 9, 10, 11, 12, 13, 14, 15];
    public static readonly List<uint> GatheringJobList = [16, 17, 18];
    public static readonly List<uint> SupportedJobs = [8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18];

    public static Dictionary<CosmicWeather, ISharedImmediateTexture> WeatherIconDict = new();

    public class JobClass
    {
        public JobFilter JobFlag { get; set; } = JobFilter.None;
        public string JobName { get; set; } = "???";
        public string shortName { get; set; } = "???";
        public ISharedImmediateTexture JobIcon { get; set; }
    }
    public static Dictionary<uint, JobClass> ClassInfoDict = new()
    {
        [8] = new() { JobFlag = JobFilter.CRP },
        [9] = new() { JobFlag = JobFilter.BSM },
        [10] = new() { JobFlag = JobFilter.ARM },
        [11] = new() { JobFlag = JobFilter.GSM },
        [12] = new() { JobFlag = JobFilter.LTW },
        [13] = new() { JobFlag = JobFilter.WVR },
        [14] = new() { JobFlag = JobFilter.ALC },
        [15] = new() { JobFlag = JobFilter.CUL },
        [16] = new() { JobFlag = JobFilter.MIN },
        [17] = new() { JobFlag = JobFilter.BTN },
        [18] = new() { JobFlag = JobFilter.FSH },
    };

    public static string GetJobName(uint jobId)
    {
        return jobId switch
        {
            8 => "Carpenter",
            9 => "Blacksmith",
            10 => "Armorer",
            11 => "Goldsmith",
            12 => "Leatherworker",
            13 => "Weaver",
            14 => "Alchemist",
            15 => "Culinarian",
            16 => "Miner",
            17 => "Botanist",
            18 => "Fisher",
            _ => "Unknown"
        };
    }


    /// <summary>
    /// Currently contains all the WKSMissionLotterySpecialCond values that are weather based
    /// MAKE SURE. TO UPDATE THIS. COME NEW MOON
    /// </summary>
    public static readonly HashSet<uint> WeatherSelection = new() { 13, 14, 15, 16, 23, 24 };

    public static Dictionary<CosmicWeather, int> WeatherIds = new()
    {
        [CosmicWeather.UmbralWind] = 60219,
        [CosmicWeather.MoonDust] = 60222,
        [CosmicWeather.Clouds] = 60203,
        [CosmicWeather.Rain] = 60207,
        [CosmicWeather.ClearSkies] = 60201,
        [CosmicWeather.FairSkies] = 60202,
    };

    public static Dictionary<uint, uint> MissionScoreDict = new(); // MissionID -> Score

    // Load the CSV file
    public static void LoadMissionScores()
    {
        MissionScoreDict.Clear();

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "ICE.Resources.MissionScores.csv";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            PluginLog.Error($"Failed to find embedded CSV: {resourceName}");
            return;
        }

        using var reader = new StreamReader(stream);
        bool headerSkipped = false;
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (!headerSkipped)
            {
                headerSkipped = true;
                continue; // Skip header
            }

            var parts = line.Split(',');
            if (parts.Length >= 4 &&
                uint.TryParse(parts[0].Trim(), out uint missionId) &&
                uint.TryParse(parts[3].Trim(), out uint score))
            {
                MissionScoreDict[missionId] = score;
            }
        }
    }

    public class XPType
    {
        public int CurrentXP { get; set; }
        public int NeededXP { get; set; }
    }

    public static Dictionary<uint, List<uint>> MissionUnlock = new()
    {
        [499] = new() { 82, 397 },
        [500] = new() { 217, 397 },
        [501] = new() { 262, 397 },
        [505] = new() { 37, 442 },
        [506] = new() { 127, 442 },
        [507] = new() { 307, 442 },
        [510] = new() { 172, 487 },
        [511] = new() { 352, 487 }
    };

    private static List<(string Name, int Id)> ArtisanMacros = new();
    public static void CrafterManagement(CosmicHelper.CosmicInfo mission, uint id, ImGuiTreeNodeFlags openDefault = ImGuiTreeNodeFlags.DefaultOpen)
    {
        var job = mission.Jobs.First(x => CosmicHelper.CrafterJobList.Contains(x));
        ImGui.Text(T("Recipe Detailed Info"));

        Dictionary<ushort, CosmicHelper.CraftingInfo> missionCrafts = new();
        foreach (var craft in mission.Crafts_Main)
            missionCrafts[craft.Key] = craft.Value;
        foreach (var craft in mission.Crafts_Pre)
            missionCrafts[craft.Key] = craft.Value;

        bool massApplyButton = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);

        if (ImGui.CollapsingHeader(T("Craft Item Settings"), openDefault))
        {
            using (ImRaii.Disabled(!massApplyButton))
            {
                ImGui.PushID(id);

                if (ImGui.Button(T("Apply to similar missions")))
                {
                    var currentMission = CosmicHelper.SheetMissionDict[id];
                    var recipeConfig = C.MissionConfig[id];

                    var currentRecipeSettings = new Dictionary<(int, int, int), MissionSettings.ArtisanSettings>();

                    foreach (var (key, craft) in currentMission.Crafts_Main)
                        if (recipeConfig.CraftSettings.TryGetValue(key, out var settings))
                            currentRecipeSettings[(craft.RecipeInfo.Durability, craft.RecipeInfo.Progress, craft.RecipeInfo.Quality)] = settings;

                    foreach (var (key, craft) in currentMission.Crafts_Pre)
                        if (recipeConfig.CraftSettings.TryGetValue(key, out var settings))
                            currentRecipeSettings[(craft.RecipeInfo.Durability, craft.RecipeInfo.Progress, craft.RecipeInfo.Quality)] = settings;

                    int appliedMissions = 0;
                    int appliedCrafts = 0;

                    void ApplyMatchingCrafts(Dictionary<ushort, CosmicHelper.CraftingInfo> crafts, MissionSettings targetConfig, ref int craftCount)
                    {
                        foreach (var (key, craft) in crafts)
                        {
                            var recipeKey = (craft.RecipeInfo.Durability, craft.RecipeInfo.Progress, craft.RecipeInfo.Quality);
                            if (!currentRecipeSettings.TryGetValue(recipeKey, out var src))
                                continue;

                            targetConfig.CraftSettings[key] = new MissionSettings.ArtisanSettings
                            {
                                UseGlobal = src.UseGlobal,
                                FoodId = src.FoodId,
                                FoodHQ = src.FoodHQ,
                                PotionId = src.PotionId,
                                PotionHQ = src.PotionHQ,
                                ManualId = src.ManualId,
                                SquadronManualId = src.SquadronManualId,
                                ArtisanSolverType = src.ArtisanSolverType,
                                MacroName = src.MacroName,
                                SkillUsageAmount = src.SkillUsageAmount,
                                MinStepsForMiracle = src.MinStepsForMiracle,
                                ExpertProfileId = src.ExpertProfileId,
                            };
                            craftCount++;
                        }
                    }

                    foreach (var sheetMission in CosmicHelper.SheetMissionDict)
                    {
                        if (!sheetMission.Value.Attributes.HasFlag(MissionAttributes.Craft))
                            continue;

                        if (sheetMission.Key == id)
                            continue;

                        if (!C.MissionConfig.TryGetValue(sheetMission.Key, out var targetConfig))
                        {
                            targetConfig = new MissionSettings();
                            C.MissionConfig[sheetMission.Key] = targetConfig;
                        }

                        int craftsBeforeApply = appliedCrafts;
                        ApplyMatchingCrafts(sheetMission.Value.Crafts_Main, targetConfig, ref appliedCrafts);
                        ApplyMatchingCrafts(sheetMission.Value.Crafts_Pre, targetConfig, ref appliedCrafts);

                        if (appliedCrafts > craftsBeforeApply)
                            appliedMissions++;
                    }

                    IceLogging.Info($"Amount of missions applied to: {appliedMissions}\n" +
                        $"Total amount of crafts applied to: {appliedCrafts}\n" +
                        $"Amount of recipies that the mission had: {currentRecipeSettings.Count()}\n" +
                        $"From Mission: {id}");
                }

                ImGui.PopID();
            }
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && !massApplyButton)
            {
                ImGui.SetTooltip(T("Hold shift to allow applying"));
            }

            foreach (var craft in missionCrafts)
            {
                if (ImGui.BeginTable($"Main Craft Details_{craft.Key}", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Hideable))
                {
                    ImGui.TableSetupColumn(T("Item Details"));
                    ImGui.TableSetupColumn(T("Dropdown Detail"));
                    ImGui.TableSetupColumn(T("Dropdown Selection"), ImGuiTableColumnFlags.WidthStretch);

                    if (C.MissionConfig[id].CraftSettings.TryGetValue(craft.Key, out var recipeConfig))
                    {
                        bool globalArtisan = recipeConfig.UseGlobal;
                        bool supportedArtisan = P.Artisan.UpdatedArtisan();

                        ImGui.TableSetColumnEnabled(1, !globalArtisan);
                        ImGui.TableSetColumnEnabled(2, !globalArtisan);

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (ImGui.Checkbox(T("Use Global Artisan Settings"), ref globalArtisan))
                        {
                            recipeConfig.UseGlobal = globalArtisan;
                            C.Save();
                        }

                        #region Label info

                        string GetSolverLabel(ArtisanCraftType type)
                        {
                            return type switch
                            {
                                ArtisanCraftType.Default => T("Default"),
                                ArtisanCraftType.Raphael => T("Raphael Solver"),
                                ArtisanCraftType.ProgressOnly => T("Progress Only Solver"),
                                ArtisanCraftType.Standard => T("Standard Solver"),
                                ArtisanCraftType.Expert => T("Expert Recipe Solver"),
                                ArtisanCraftType.Macro => T("Artisan Macro"),
                                _ => T("Unknown")
                            };
                        }
                        string GetFoodLable(uint foodId)
                        {
                            if (foodId == 0) return T("Default");
                            var item = ConsumableInfo.CrafterFood.FirstOrDefault(x => x.Id == foodId);
                            PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                            PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                            return BuildItemLabel(item.Name, nq, hq);
                        }
                        string GetPotionLable(uint potionId)
                        {
                            if (potionId == 0) return T("Default");
                            var item = ConsumableInfo.Pots.FirstOrDefault(x => x.Id == potionId);
                            PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                            PlayerHelper.GetItemCount(item.Id, out var hq, includeHq: true, includeNq: false);
                            return BuildItemLabel(item.Name, nq, hq);
                        }
                        string GetManualLabel(uint manualId)
                        {
                            if (manualId == 0) return T("Default");
                            var item = ConsumableInfo.Manuals.FirstOrDefault(x => x.Id == manualId);
                            PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                            return BuildItemLabel(item.Name, nq, 0);
                        }
                        string GetSquadronManualLabel(uint squadManualId)
                        {
                            if (squadManualId == 0) return T("Default");
                            var item = ConsumableInfo.SquadronManuals.FirstOrDefault(x => x.Id == squadManualId);
                            PlayerHelper.GetItemCount(item.Id, out var nq, includeHq: false, includeNq: true);
                            return BuildItemLabel(item.Name, nq, 0);
                        }
                        string BuildItemLabel(string name, int nqCount, int hqCount)
                        {
                            var parts = new List<string>();
                            if (hqCount > 0) parts.Add($"{(char)0xE03C} {name} [x{hqCount}]");
                            if (nqCount > 0) parts.Add($"{name} [x{nqCount}]");
                            return string.Join(" / ", parts);
                        }

                        var recipe_Solver = GetSolverLabel(recipeConfig.ArtisanSolverType);
                        var recipe_FoodLabel = GetFoodLable(recipeConfig.FoodId);
                        var recipe_PotionLabel = GetPotionLable(recipeConfig.PotionId);
                        var recipe_ManualLabel = GetManualLabel(recipeConfig.ManualId);
                        var recipe_SquadManualLabel = GetSquadronManualLabel(recipeConfig.SquadronManualId);

                        float recipe_ComboWidth = new[]
                        {
                                                recipe_FoodLabel,
                                                recipe_PotionLabel,
                                                recipe_ManualLabel,
                                                recipe_SquadManualLabel,
                                                recipe_Solver
                                            }.Max(label => ImGui.CalcTextSize(label).X + ImGui.GetStyle().FramePadding.X * 2 + ImGui.GetStyle().ScrollbarSize + 10);

                        List<ArtisanCraftType> standardSolvers = new()
                                    {
                                        ArtisanCraftType.Default,
                                        ArtisanCraftType.Standard,
                                        ArtisanCraftType.Raphael,
                                        ArtisanCraftType.ProgressOnly,
                                        ArtisanCraftType.Macro,
                                    };

                        List<ArtisanCraftType> expertSolvers = new()
                                    {
                                        ArtisanCraftType.Default,
                                        ArtisanCraftType.Expert,
                                        ArtisanCraftType.Raphael,
                                        ArtisanCraftType.Macro,
                                    };

                        #endregion

                        #region Image

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (Svc.Texture.TryGetFromGameIcon(craft.Value.IconId, out var iconImage))
                        {
                            ImGui.Image(iconImage.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text(T("Key / RecipeId: {0}", craft.Key));
                            ImGui.Text(T("ItemID: {0}", craft.Value.ItemId));
                            ImGui.EndTooltip();
                        }
                        if (craft.Value.ExpertCraft)
                        {
                            ImGui.SameLine();
                            ImGui.AlignTextToFramePadding();
                            ImGuiEx.Icon(new Vector4(1.0f, 0.4f, 0.0f, 1.0f), FontAwesomeIcon.Diamond);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Expert Craft"));
                            }
                        }

                        #endregion

                        #region Item Name + Solver


                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"{craft.Value.ItemName}");

                        ImGui.TableNextColumn();
                        ImGui.Text(T("Solver"));

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(recipe_ComboWidth);
                        if (ImGui.BeginCombo("##Solver", recipe_Solver))
                        {
                            if (craft.Value.ExpertCraft)
                            {
                                foreach (var type in expertSolvers)
                                {
                                    bool isSelected = recipeConfig.ArtisanSolverType == type;
                                    if (ImGui.Selectable(GetSolverLabel(type), isSelected))
                                    {
                                        recipeConfig.ArtisanSolverType = type;
                                        C.Save();
                                    }
                                    if (isSelected)
                                        ImGui.SetItemDefaultFocus();
                                }
                            }
                            else
                            {
                                foreach (var type in standardSolvers)
                                {
                                    bool isSelected = recipeConfig.ArtisanSolverType == type;
                                    if (ImGui.Selectable(GetSolverLabel(type), isSelected))
                                    {
                                        recipeConfig.ArtisanSolverType = type;
                                        C.Save();
                                    }
                                    if (isSelected)
                                        ImGui.SetItemDefaultFocus();
                                }
                            }

                            ImGui.EndCombo();
                        }

                        if (recipeConfig.ArtisanSolverType == ArtisanCraftType.Macro)
                        {

                            ImGui.SameLine();
                            string macroName = recipeConfig.MacroName;
                            ImGui.SetNextItemWidth(200);
                            if (ImGui.BeginCombo("##MacroName", macroName, ImGuiComboFlags.HeightLargest))
                            {
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputText(T("Macro Name"), ref macroName))
                                {
                                    recipeConfig.MacroName = macroName;
                                    C.SaveDebounced();
                                }

                                ImGui.Separator();
                                if (ImGui.Button(T("Refresh Artisan Macros")))
                                {
                                    ArtisanMacros = P.Artisan.MacroList();
                                }
                                if (ArtisanMacros.Count > 0)
                                {
                                    var lineHeight = ImGui.GetTextLineHeightWithSpacing();
                                    var macroChildHeight = lineHeight * 5 + ImGui.GetStyle().FramePadding.Y * 2;

                                    if (ImGui.BeginChild("Macro List: Child", new(ImGui.GetContentRegionAvail().X, macroChildHeight)))
                                    {
                                        foreach (var macro in ArtisanMacros)
                                        {
                                            bool isSelected = recipeConfig.MacroName == macro.Name;
                                            if (ImGui.Selectable($"[{macro.Id}] - {macro.Name}##{macro.Id}"))
                                            {
                                                recipeConfig.MacroName = macro.Name;
                                                C.Save();
                                            }

                                            if (isSelected)
                                                ImGui.SetItemDefaultFocus();
                                        }
                                    }
                                    ImGui.EndChild();
                                }

                                ImGui.EndCombo();
                            }

                            /*
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(200);
                            if (ImGui.InputText(T("Macro Name"), ref macroName))
                            {
                                recipeConfig.MacroName = macroName;
                                C.Save();
                            }
                            */
                        }

                        #endregion

                        #region Durability + Food

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text(T("Durability: {0}", craft.Value.RecipeInfo.Durability));

                        if (supportedArtisan)
                        {
                            ImGui.TableNextColumn();
                            ImGui.Text(T("Food"));

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(recipe_ComboWidth);
                            if (ImGui.BeginCombo("##FoodSelection", recipe_FoodLabel))
                            {
                                bool isDefaultSelected = recipeConfig.FoodId == 0;
                                if (ImGui.Selectable(T("Default"), isDefaultSelected))
                                {
                                    recipeConfig.FoodId = 0;
                                    recipeConfig.FoodHQ = false;
                                    C.Save();
                                }
                                if (isDefaultSelected)
                                    ImGui.SetItemDefaultFocus();

                                ImGui.Separator();

                                foreach (var item in ConsumableInfo.CrafterFood)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);
                                    PlayerHelper.GetItemCount(item.Id, out var hqCount, includeHq: true, includeNq: false);

                                    if (nqCount == 0 && hqCount == 0) continue;

                                    bool isSelected = recipeConfig.FoodId == item.Id;
                                    string label = BuildItemLabel(item.Name, nqCount, hqCount) + $"###{item.Id}";

                                    if (ImGui.Selectable(label, isSelected))
                                    {
                                        recipeConfig.FoodId = item.Id;
                                        recipeConfig.FoodHQ = hqCount > 0;
                                        C.Save();
                                    }

                                    if (isSelected)
                                        ImGui.SetItemDefaultFocus();
                                }

                                ImGui.EndCombo();
                            }
                        }

                        #endregion

                        #region Progress + Potion

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text(T("Progress: {0}", craft.Value.RecipeInfo.Progress));

                        if (supportedArtisan)
                        {
                            ImGui.TableNextColumn();
                            ImGui.Text(T("Potion"));

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(recipe_ComboWidth);
                            if (ImGui.BeginCombo("##StandardPotion", recipe_PotionLabel))
                            {
                                // Default option
                                bool isDefaultSelected = recipeConfig.PotionId == 0;
                                if (ImGui.Selectable(T("Default"), isDefaultSelected))
                                {
                                    recipeConfig.PotionId = 0;
                                    recipeConfig.PotionHQ = false;
                                    C.Save();
                                }
                                if (isDefaultSelected)
                                    ImGui.SetItemDefaultFocus();

                                ImGui.Separator();

                                foreach (var item in ConsumableInfo.Pots)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);
                                    PlayerHelper.GetItemCount(item.Id, out var hqCount, includeHq: true, includeNq: false);

                                    if (nqCount == 0 && hqCount == 0) continue;

                                    bool isSelected = recipeConfig.PotionId == item.Id;
                                    string label = BuildItemLabel(item.Name, nqCount, hqCount) + $"###{item.Id}";

                                    if (ImGui.Selectable(label, isSelected))
                                    {
                                        recipeConfig.PotionId = item.Id;
                                        recipeConfig.PotionHQ = hqCount > 0;
                                        C.Save();
                                    }

                                    if (isSelected)
                                        ImGui.SetItemDefaultFocus();
                                }

                                ImGui.EndCombo();
                            }
                        }

                        #endregion

                        #region Quality + Manual

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text(T("Quality: {0}", craft.Value.RecipeInfo.Quality));

                        if (supportedArtisan)
                        {
                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text(T("Manual"));

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(recipe_ComboWidth);
                            if (ImGui.BeginCombo("##StandardManual", recipe_ManualLabel))
                            {
                                // Default option
                                bool isDefaultSelected = recipeConfig.ManualId == 0;
                                if (ImGui.Selectable(T("Default"), isDefaultSelected))
                                {
                                    recipeConfig.ManualId = 0;
                                    C.Save();
                                }
                                if (isDefaultSelected)
                                    ImGui.SetItemDefaultFocus();

                                ImGui.Separator();

                                foreach (var item in ConsumableInfo.Manuals)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);

                                    if (nqCount == 0) continue;

                                    bool isSelected = recipeConfig.ManualId == item.Id;
                                    string label = BuildItemLabel(item.Name, nqCount, 0) + $"###{item.Id}";

                                    if (ImGui.Selectable(label, isSelected))
                                    {
                                        recipeConfig.ManualId = item.Id;
                                        C.Save();
                                    }

                                    if (isSelected)
                                        ImGui.SetItemDefaultFocus();
                                }

                                ImGui.EndCombo();
                            }
                        }

                        #endregion

                        #region Squadron Manual

                        if (!globalArtisan)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text(T("Squadron Manual"));

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(recipe_ComboWidth);
                            if (ImGui.BeginCombo("##StandardSquadManual", recipe_SquadManualLabel))
                            {
                                // Default option
                                bool isDefaultSelected = recipeConfig.SquadronManualId == 0;
                                if (ImGui.Selectable(T("Default"), isDefaultSelected))
                                {
                                    recipeConfig.SquadronManualId = 0;
                                    C.Save();
                                }
                                if (isDefaultSelected)
                                    ImGui.SetItemDefaultFocus();

                                ImGui.Separator();

                                foreach (var item in ConsumableInfo.SquadronManuals)
                                {
                                    PlayerHelper.GetItemCount(item.Id, out var nqCount, includeHq: false, includeNq: true);

                                    if (nqCount == 0) continue;

                                    bool isSelected = recipeConfig.SquadronManualId == item.Id;
                                    string label = BuildItemLabel(item.Name, nqCount, 0) + $"###{item.Id}";

                                    if (ImGui.Selectable(label, isSelected))
                                    {
                                        recipeConfig.SquadronManualId = item.Id;
                                        C.Save();
                                    }

                                    if (isSelected)
                                        ImGui.SetItemDefaultFocus();
                                }

                                ImGui.EndCombo();
                            }
                        }

                        #endregion

                        #region ActionUsage

                        var actionInfo = mission.TemporaryAction;
                        if (actionInfo.ActionId != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            var name = actionInfo.Name;
                            var icon = actionInfo.Icon.GetWrapOrEmpty();
                            ImGui.Image(icon.Handle, new(24, 24));
                            ImGui.AlignTextToFramePadding();
                            ImGui.SameLine();
                            ImGui.Text($"{name}");

                            if (supportedArtisan)
                            {
                                ImGui.TableNextColumn();
                                ImGui.Text(T("Max use"));

                                ImGui.TableNextColumn();
                                var maxUsage = recipeConfig.SkillUsageAmount;
                                ImGui.SetNextItemWidth(recipe_ComboWidth);
                                string skillUsageLabel = maxUsage == -1 ? T("Default") : $"{maxUsage}";
                                if (ImGui.SliderInt("##MaxSkillUsage", ref maxUsage, -1, (int)actionInfo.UseAmount, skillUsageLabel))
                                {
                                    recipeConfig.SkillUsageAmount = maxUsage;
                                    C.SaveDebounced();
                                }

                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
#if DEBUG
                                if (ImGui.Button(T("Test Apply")))
                                {
                                    var key = craft.Key;
                                    var useAmount = recipeConfig.SkillUsageAmount;
                                    var miracleSteps = recipeConfig.MinStepsForMiracle;
                                    IceLogging.Verbose($"Was Expert: {craft.Value.ExpertCraft}", "Test Apply Skills");
                                    if (craft.Value.ExpertCraft)
                                    {
                                        if (recipeConfig.SkillUsageAmount != -1)
                                        {
                                            P.Artisan.ChangeExpertMaxSteadyUses(key, (uint)useAmount, true);
                                            P.Artisan.ChangeExpertMaxMaterialMiracleUses(key, (uint)useAmount, true);
                                        }
                                        else
                                        {
                                            P.Artisan.SetTempExpertMaxSteadyUsesBackToNormal(key);
                                            P.Artisan.SetTempExpertMaxMaterialMiracleUsesBackToNormal(key);
                                        }

                                        if (miracleSteps != -1)
                                            P.Artisan.ChangeExpertMinimumStepsBeforeMiracle(key, (uint)miracleSteps, true);
                                        else
                                            P.Artisan.SetTempExpertMinimumStepsBeforeMiracleBackToNormal(key);
                                    }
                                    else
                                    {
                                        if (useAmount != -1)
                                        {
                                            P.Artisan.ChangeStandardMaxMaterialMiracleUses((uint)useAmount, true);
                                        }
                                        else
                                        {
                                            P.Artisan.SetTempStandardMaxMaterialMiracleUsesBackToNormal();
                                        }

                                        if (miracleSteps != 1)
                                        {
                                            P.Artisan.ChangeStandardMinimumStepsBeforeMiracle((uint)miracleSteps, true);
                                        }
                                        else
                                        {
                                            P.Artisan.SetTempStandardMinimumStepsBeforeMiracleBackToNormal();
                                        }
                                    }
                                }
#endif

                                if (mission.TemporaryAction.ActionId == 41269 && !globalArtisan)
                                {
                                    ImGui.TableSetColumnIndex(1);
                                    ImGui.Text(T("Use after this many steps"));

                                    ImGui.TableNextColumn();
                                    var minSteps = recipeConfig.MinStepsForMiracle;
                                    string skillMinStepsName = minSteps == -1 ? T("Default") : $"{minSteps}";
                                    ImGui.SetNextItemWidth(recipe_ComboWidth);
                                    if (ImGui.SliderInt("##MinMiracleSteps", ref minSteps, -1, 20, skillMinStepsName))
                                    {
                                        recipeConfig.MinStepsForMiracle = minSteps;
                                        C.SaveDebounced();
                                    }
                                }
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        C.MissionConfig[id].CraftSettings[craft.Key] = new();
                        C.SaveDebounced();
                    }

                    ImGui.EndTable();
                }
            }
        }
    }
}
