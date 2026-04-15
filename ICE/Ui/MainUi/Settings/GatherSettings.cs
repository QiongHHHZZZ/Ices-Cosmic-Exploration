using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static ICE.ConfigFiles.Config;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings
{
    internal class GatherSettings
    {
        private static string newProfileName = "";
        private static string[] MissionTypes = ["限量节点", "采集数量", "限时任务", "连锁任务", "额外采集奖励任务", "连锁+额外采集奖励任务", "双职业任务"];
        private static readonly string[] RankLabels = ["全部任务", "D级及以上", "C级及以上", "B级及以上", "A级及以上", "EX及以上", "仅EX+"];
        private static int MissionIndex = 0;

        private static readonly string PROFILE_PREFIX = "IceGatherProfile_";

        public static string ExportGatherProfile(int profileId)
        {
            if (!C.GatherProfiles.TryGetValue(profileId, out var profile))
                return string.Empty;

            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);

            return PROFILE_PREFIX + base64;
        }

        public static bool ImportGatherProfile(string importString, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Check for and remove the prefix
                if (!importString.StartsWith(PROFILE_PREFIX))
                {
                    errorMessage = T("Invalid import string: Missing prefix");
                    return false;
                }

                var base64String = importString.Substring(PROFILE_PREFIX.Length);

                var bytes = Convert.FromBase64String(base64String);
                var json = Encoding.UTF8.GetString(bytes);

                var profile = JsonSerializer.Deserialize<GatherProfile>(json);
                if (profile == null)
                {
                    errorMessage = T("Failed to deserialize profile");
                    return false;
                }

                // Get the next available ID
                int nextId = C.GatherProfiles.Keys.Count > 0
                    ? C.GatherProfiles.Keys.Max() + 1
                    : 0;

                profile.Id = nextId;
                C.GatherProfiles[nextId] = profile;

                // Save the configuration
                C.Save();

                return true;
            }
            catch (FormatException)
            {
                errorMessage = T("Invalid import string: Not valid base64");
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = T("Import failed: {0}", ex.Message);
                return false;
            }
        }

        public static bool InitialSetupProfile(string importString, string type, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Check for and remove the prefix
                if (!importString.StartsWith(PROFILE_PREFIX))
                {
                    errorMessage = T("Invalid import string: Missing prefix");
                    return false;
                }

                var base64String = importString.Substring(PROFILE_PREFIX.Length);

                var bytes = Convert.FromBase64String(base64String);
                var json = Encoding.UTF8.GetString(bytes);

                var profile = JsonSerializer.Deserialize<GatherProfile>(json);
                if (profile == null)
                {
                    errorMessage = T("Failed to deserialize profile");
                    return false;
                }

                // Get the next available ID
                int nextId = C.GatherProfiles.Keys.Count > 0
                    ? C.GatherProfiles.Keys.Max() + 1
                    : 0;

                profile.Id = nextId;
                C.GatherProfiles[nextId] = profile;

                foreach (var mission in C.MissionConfig)
                {
                    var id = mission.Key;

                    var missionDict = CosmicHelper.SheetMissionDict[id];

                    bool craftMission = missionDict.Attributes.HasFlag(MissionAttributes.Craft);
                    bool gatherMission = missionDict.Attributes.HasFlag(MissionAttributes.Gather);

                    bool LimitedQuant = missionDict.Attributes.HasFlag(MissionAttributes.Limited);
                    // Gather X Amount is just "Gather" 
                    bool TimedMission = missionDict.Attributes.HasFlag(MissionAttributes.ScoreTimeRemaining);
                    bool ChainedMission = missionDict.Attributes.HasFlag(MissionAttributes.ScoreChains);
                    bool BoonMission = missionDict.Attributes.HasFlag(MissionAttributes.ScoreGatherersBoon);
                    bool collectableMission = missionDict.Attributes.HasFlag(MissionAttributes.Collectables);
                    bool stellerReductionMission = missionDict.Attributes.HasFlag(MissionAttributes.ReducedItems);

                    bool GatherX = !stellerReductionMission && !collectableMission && !BoonMission && !ChainedMission && !TimedMission && !LimitedQuant;

                    void UpdateMissions()
                    {
                        mission.Value.GProfileId = nextId;
                    }

                    if (gatherMission && (!collectableMission && !stellerReductionMission))
                    {
                        if (type == "limited" && LimitedQuant)
                            UpdateMissions();
                        else if (type == "timed" && TimedMission)
                            UpdateMissions();
                        else if (type == "chained" && ChainedMission && !BoonMission)
                            UpdateMissions();
                        else if (type == "boon" && BoonMission && !ChainedMission)
                            UpdateMissions();
                        else if (type == "boonChain" && ChainedMission && BoonMission)
                            UpdateMissions();
                        else if (type == "dualCraft" && craftMission)
                            UpdateMissions();
                        else if (type == "gatherX" && GatherX)
                            UpdateMissions();
                    }
                }



                return true;
            }
            catch (FormatException)
            {
                errorMessage = T("Invalid import string: Not valid base64");
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = T("Import failed: {0}", ex.Message);
                return false;
            }
        }

        private static Dictionary<uint, string> Foods = new();

        public static void Draw()
        {
            int maxGp = 1200;

            bool SelfSpiritbondGather = C.SelfSpiritbondGather;
            if (ImGui.Checkbox(T("Extract Spiritbond on Gather"), ref SelfSpiritbondGather))
            {
                if (C.SelfSpiritbondGather != SelfSpiritbondGather)
                {
                    C.SelfSpiritbondGather = SelfSpiritbondGather;
                    C.Save();
                }
            }

            bool AutoCordial = C.AutoCordial;
            if (ImGui.Checkbox(T("Auto Cordial"), ref AutoCordial))
            {
                C.AutoCordial = AutoCordial;
                C.Save();
            }
            ImGuiEx.HelpMarker(T("Will only work while using ICE and not manual mode\n" +
                               "Will also pause pandora cordial usage while on the moon"));
            if (ImGui.CollapsingHeader(T("Cordial Settings")))
            {
                int cordialMinRank = C.CordialMinRank;
                ImGui.SetNextItemWidth(150);
                if (ImGui.Combo(T("Min mission rank for cordials"), ref cordialMinRank, RankLabels, RankLabels.Length))
                {
                    C.CordialMinRank = cordialMinRank;
                    C.Save();
                }

                bool InverseCordialPrio = C.inverseCordialPrio;
                bool PreventOvercap = C.PreventOvercap;
                int CordialMinGp = C.CordialMinGp;

                if (ImGui.Checkbox(T("Inverse Priority (Watered -> Regular -> Hi)"), ref InverseCordialPrio))
                {
                    C.inverseCordialPrio = InverseCordialPrio;
                    C.Save();
                }
                if (ImGui.Checkbox(T("Prevent Overcap"), ref PreventOvercap))
                {
                    C.PreventOvercap = PreventOvercap;
                    C.Save();
                }
                ImGui.SetNextItemWidth(200);
                if (ImGui.SliderInt(T("Use cordial when below the following GP"), ref CordialMinGp, 0, maxGp))
                {
                    C.CordialMinGp = CordialMinGp;
                    C.SaveDebounced();
                }
                ImGui.SameLine();
                ImGuiEx.HelpMarker(T("What's the minimum gp you can have before it uses a cordial.\n" +
                                   "If set to 0, it'll never use a cordial even with it enabled (because... you'll never have 0 gp)"));
            }

            if (ImGui.CollapsingHeader(T("Food Settings")))
            {
                int foodMinRank = C.FoodMinRank;
                ImGui.SetNextItemWidth(150);
                if (ImGui.Combo(T("Min mission rank for food"), ref foodMinRank, RankLabels, RankLabels.Length))
                {
                    C.FoodMinRank = foodMinRank;
                    C.Save();
                }

                bool useFood = C.UseGatheringFood;
                if (ImGui.Checkbox(T("Use food on gathering missions"), ref useFood))
                {
                    C.UseGatheringFood = useFood;
                    C.Save();
                }

                if (ImGui.Button(T("Select Gathering Food")))
                {
                    foreach (var item in ConsumableInfo.GatherFood)
                    {
                        if (PlayerHelper.GetItemCount(item.Id, out var count) && count > 0)
                            Foods[item.Id] = item.Name;
                    }

                    ImGui.OpenPopup("Food Selection");
                }
                ImGui.SameLine();
                if (C.GatheringFood == 0)
                {
                    ImGui.Text(T("No Food Selected"));
                }
                else
                {
                    var itemName = Svc.Data.GetExcelSheet<Item>().Where(x => x.RowId == C.GatheringFood).FirstOrDefault().Name.ToString();
                    ImGui.Text($"{itemName}");
                }

                if (ImGui.BeginPopup("Food Selection"))
                {
                    if (ImGui.BeginTable("Food Item Selection", 2, ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn(T("Food Item"));
                        ImGui.TableSetupColumn(T("Amount"));

                        // First Column, pretty much giving an option for "None" if they want none
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (ImGui.Selectable(T("Use no gathering food")))
                        {
                            C.GatheringFood = 0;
                            C.Save();
                            ImGui.CloseCurrentPopup();
                        }

                        foreach (var item in Foods)
                        {
                            ImGui.TableNextRow();
                            ImGui.PushID(item.Key);

                            ImGui.TableSetColumnIndex(0);
                            if (ImGui.Selectable($"{item.Value}"))
                            {
                                C.GatheringFood = item.Key;
                                C.Save();

                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.TableNextColumn();
                            PlayerHelper.GetItemCount(item.Key, out var count);
                            if (ImGui.Selectable($"x {count}"))
                            {
                                C.GatheringFood = item.Key;
                                C.Save();

                                ImGui.CloseCurrentPopup();
                            }
                        }

                        ImGui.EndTable();
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.Separator();

            if (ImGui.BeginTable("Gathering Profile Settings", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn(T("Profile Selection"));
                ImGui.TableSetupColumn(T("Gathering Settings"));

                // 1st Row, technically only really used for the gather profile name creator
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.SetNextItemWidth(200);
                ImGui.InputText(T("New Profile Name"), ref newProfileName, 64);
                using (ImRaii.Disabled(newProfileName == ""))
                {
                    if (ImGui.Button(T("Add Profile")) && !string.IsNullOrWhiteSpace(newProfileName))
                    {
                        var newId = C.GatherProfiles.Keys.Max() + 1;
                        C.GatherProfiles[newId] = new()
                        {
                            Name = newProfileName,
                        };
                        C.Save();
                        newProfileName = "";
                    }
                }

                // 2nd Row, Actually profile selector
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                #region Profile Selection

                ImGui.Text(T("Gather Profiles"));

                bool canDelete = C.GatherProfiles.Count > 1 && C.SelectedGatherIndex != 0;
                using (ImRaii.Disabled(!canDelete))
                {
                    if (ImGui.Button(T("Delete Selected Profile")))
                    {
                        int deletedId = C.SelectedGatherIndex;

                        // Don't allow deleting the default profile
                        if (deletedId == 0)
                        {
                            return;
                        }

                        // Remove the profile
                        C.GatherProfiles.Remove(deletedId);

                        // Update all missions using this GatherSettingId
                        foreach (var mission in C.MissionConfig)
                        {
                            if (mission.Value.GProfileId == deletedId)
                            {
                                mission.Value.GProfileId = 0; // fallback to default
                            }
                        }

                        // Clamp the selected index and save
                        C.SelectedGatherIndex = 0;
                        C.Save();
                    }
                }

                if (ImGui.BeginChild("GatherProfileChild", new Vector2(300, ImGui.GetTextLineHeightWithSpacing() * 5 + 10), true))
                {
                    foreach (var profile in C.GatherProfiles)
                    {
                        var id = profile.Key;
                        var profileDisplayName = T(profile.Value.Name);
                        bool isSelected = C.SelectedGatherIndex == id;
                        if (ImGui.Selectable($"{profileDisplayName}##{profile.Value.Name}_{id}", isSelected))
                        {
                            C.SelectedGatherIndex = id;
                            C.Save();
                        }

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndChild();

                if (!C.GatherProfiles.TryGetValue(C.SelectedGatherIndex, out var entry))
                {
                    // We've somehow gotten a variable that is outside the normal index, so going to just reset it back to 0
                    C.SelectedGatherIndex = 0;
                    C.SaveDebounced();
                }

                ImGui.Combo(T("Mission Type"), ref MissionIndex, MissionTypes, MissionTypes.Length);
                if (ImGui.Button(T("Apply to Mission Types")))
                {
                    foreach (var mission in C.MissionConfig)
                    {
                        var id = mission.Key;
                        if (CosmicHelper.SheetMissionDict.TryGetValue(id, out var missionDict))
                        {
                            bool craftMission = missionDict.Attributes.HasFlag(MissionAttributes.Craft);
                            bool gatherMission = missionDict.Attributes.HasFlag(MissionAttributes.Gather);

                            bool LimitedQuant = missionDict.Attributes.HasFlag(MissionAttributes.Limited);
                            // Gather X Amount is just "Gather" 
                            bool TimedMission = missionDict.Attributes.HasFlag(MissionAttributes.ScoreTimeRemaining);
                            bool ChainedMission = missionDict.Attributes.HasFlag(MissionAttributes.ScoreChains);
                            bool BoonMission = missionDict.Attributes.HasFlag(MissionAttributes.ScoreGatherersBoon);
                            bool collectableMission = missionDict.Attributes.HasFlag(MissionAttributes.Collectables);
                            bool stellerReductionMission = missionDict.Attributes.HasFlag(MissionAttributes.ReducedItems);

                            bool GatherX = !stellerReductionMission && !collectableMission && !BoonMission && !ChainedMission && !TimedMission && !LimitedQuant;

                            void UpdateMissions()
                            {
                                mission.Value.GProfileId = entry.Id;
                            }

                            if (gatherMission && (!collectableMission && !stellerReductionMission))
                            {
                                if (MissionIndex == 0 && LimitedQuant)
                                    UpdateMissions();
                                else if (MissionIndex == 2 && TimedMission)
                                    UpdateMissions();
                                else if (MissionIndex == 3 && ChainedMission && !BoonMission)
                                    UpdateMissions();
                                else if (MissionIndex == 4 && BoonMission && !ChainedMission)
                                    UpdateMissions();
                                else if (MissionIndex == 5 && ChainedMission && BoonMission)
                                    UpdateMissions();
                                else if (MissionIndex == 6 && craftMission)
                                    UpdateMissions();
                                else if (MissionIndex == 1 && GatherX)
                                    UpdateMissions();
                            }
                        }
                    }

                    C.Save();
                }

                #endregion

                #region Profile Editor

                ImGui.TableNextColumn();
                #region Minimum GP + Dual Class Info

                int minGP = entry.MinimumGp;
                ImGui.SetNextItemWidth(100);
                if (ImGui.SliderInt(T("Minimum GP to start mission"), ref minGP, -1, maxGp))
                {
                    entry.MinimumGp = minGP;
                    C.SaveDebounced();
                }

                ImGui.Text(T("Where did the dual craft amount go?"));
                ImGui.SameLine();
                ImGui.Dummy(new(5, 0));
                ImGui.SameLine();
                ImGui.TextDisabled("?");
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetNextWindowSize(new(400.0f, 0.0f)); // Fixed width, auto height
                    ImGui.BeginTooltip();

ImGui.TextWrapped(T("Short answer: It's built in now\n" +
                     "Long answer: Honestly, this was a cumbersome system in itself. And with square deciding to not continue on with dual crafting missions going into the 2nd moon, I figured it would be better to just tie it into the scoring system. You realistically only need:\n" +
                     "Gold: 3 Items\n" +
                     "Silver: 2 Items\n" +
                     "Bronze: 1 Item\n" +
                     "to be able to hit the threshold. And even then, if you manage to not hit it on the first attempt, it'll just keep gathering. Plus. This makes it to where I can not have to worry about profile managing on fishing for... 4 missions? Seemed minorly reduntant in my eyes.\n" +
                     "So now how it'll work. Select the turnin option (Gold/Any both work the same) and it will now gather up to the necessary amount -> turnin when it's ready.\n" +
                     "NOW NONE OF YOU CAN TELL IT TO CRAFT 27 ITEMS. STOP IT. IT SAID CRAFT (╯°Д°)╯︵/(.□ . \\)"));
                    ImGui.EndTooltip();
                }

                #endregion

                #region Boon Increase 2

                if (ImGui.CollapsingHeader(T("Pioneer's | Mountaineer's Gift II")))
                {
                    string buffName = "BoonIncrease2";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Apply a 30% buff to your boon chance.");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }

                    ImGui.PopID();
                }

                #endregion

                #region Boon Increase 1

                if (ImGui.CollapsingHeader(T("Pioneer's | Mountaineer's Gift I")))
                {
                    string buffName = "BoonIncrease1";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Apply a 10% buff to your boon chance.");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #region Nophica's / Nald'thal's Tidings

                if (ImGui.CollapsingHeader(T("Nophica's / Nald'thal's Tidings Buff")))
                {
                    string buffName = "Tidings";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increases item yield from Gatherer's Boon by 1");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #region Blessed / Kings Yield II

                if (ImGui.CollapsingHeader(T("Blessed / Kings Yield II")))
                {
                    string buffName = "YieldII";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minUsableDurability = entry.GatherBuffs.Buffs[buffName].MinUsableDurability;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increases the number of items obtained when gathering by 2\n" +
                                        "Will only apply when the gathering node has full durability");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Node Durability For Usage"), ref minUsableDurability, 0, 8))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinUsableDurability = minUsableDurability;
                        C.SaveDebounced();
                    }
                    ImGui_Ice.IconWithTooltip(Dalamud.Interface.FontAwesomeIcon.InfoCircle,
                        T("What's the minimum durability a node can have before this action is activated?\n" +
                        "Mainly used for missions where you can chain durability refresh"));

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #region Blessed / Kings Yield I

                if (ImGui.CollapsingHeader(T("Blessed / Kings Yield I")))
                {
                    string buffName = "YieldI";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minUsableDurability = entry.GatherBuffs.Buffs[buffName].MinUsableDurability;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increases the number of items obtained when gathering by 1\n" +
                                        "Will only apply when the gathering node has full durability");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Node Durability For Usage"), ref minUsableDurability, 0, 8))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinUsableDurability = minUsableDurability;
                        C.SaveDebounced();
                    }
                    ImGui_Ice.IconWithTooltip(Dalamud.Interface.FontAwesomeIcon.InfoCircle,
                        T("What's the minimum durability a node can have before this action is activated?\n" +
                        "Mainly used for missions where you can chain durability refresh"));

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #region Bonus Integrity

                if (ImGui.CollapsingHeader(T("Ageless Words / Solid Reason")))
                {
                    string buffName = "BonusIntegrity";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minUsableDurability = entry.GatherBuffs.Buffs[buffName].MinUsableDurability;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increase the Integrity by 1\n" +
                                        "50% chance to grant Eureka Moment");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Node Durability for Usage"), ref minUsableDurability, 0, 8))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinUsableDurability = minUsableDurability;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #region Bountiful Yield II

                if (ImGui.CollapsingHeader(T("Bountiful Yield II / Bountiful Harvest II")))
                {
                    string buffName = "BountifulYieldII";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increases the number of items obtained when gathering by 2\n" +
                                        "Will only apply when the gathering node has full durability");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.Text(T("Minumum Items To Gather"));
                    ImGui.SameLine();
                    int minItems = entry.GatherBuffs.BountifulMinItem;
                    if (ImGui.DragInt("##MinItemsGather", ref minItems, 1, 2, 4))
                    {
                        entry.GatherBuffs.BountifulMinItem = minItems;
                        C.SaveDebounced();
                    }

                    ImGui.PopID();
                }

                #endregion

                #region Field Mastery (Gather Chance)

                #region Field Mastery III

                if (ImGui.CollapsingHeader(T("Field Mastery | Sharp Vision III")))
                {
                    string buffName = "FieldMasteryIII";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increases the gather chance by 50%\n" +
                                        "Please note: You can have multiple enabled, but only the one that will get you the closest to " +
                                        "100% the cheapest will be applied");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #region Field Mastery II

                if (ImGui.CollapsingHeader(T("Field Mastery | Sharp Vision II")))
                {
                    string buffName = "FieldMasteryII";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increases the gather chance by 15%\n" +
                                        "Please note: You can have multiple enabled, but only the one that will get you the closest to " +
                                        "100% the cheapest will be applied");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #region Field Mastery I

                if (ImGui.CollapsingHeader(T("Field Mastery | Sharp Vision I")))
                {
                    string buffName = "FieldMasteryI";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increases the gather chance by 5%\n" +
                                        "Please note: You can have multiple enabled, but only the one that will get you the closest to " +
                                        "100% the cheapest will be applied");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #region Field Mastery [Temp]

                if (ImGui.CollapsingHeader(T("Flora Mastery | Clear Vision [Temp]")))
                {
                    string buffName = "FieldMasteryTemp";

                    ImGui.PushID(buffName);

                    bool currentlyEnabled = entry.GatherBuffs.Buffs[buffName].Enabled;
                    int minUseGp = entry.GatherBuffs.Buffs[buffName].MinGp;
                    int minActionGp = GatheringUtil.GathActionDict[buffName].RequiredGp;
                    int maxActionUsage = entry.GatherBuffs.Buffs[buffName].MaxUse;
                    string ActionInfo = T("Increases the gather chance by 15%\n" +
                                        "This can be applied with normal field mastery, but will only apply per hit");

                    ImGui.Text(T("Action Info:"));
                    ImGuiEx.HelpMarker(ActionInfo);

                    if (ImGui.Checkbox(T("Enable"), ref currentlyEnabled))
                    {
                        entry.GatherBuffs.Buffs[buffName].Enabled = currentlyEnabled;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt(T("Minimum Gp for Usage"), ref minUseGp, minActionGp, maxGp))
                    {
                        entry.GatherBuffs.Buffs[buffName].MinGp = minUseGp;
                        C.SaveDebounced();
                    }

                    ImGui.SetNextItemWidth(200);
                    if (ImGui.InputInt(T("Max Use"), ref maxActionUsage))
                    {
                        entry.GatherBuffs.Buffs[buffName].MaxUse = maxActionUsage;
                        C.SaveDebounced();
                    }
                    ImGuiEx.HelpMarker(T("Set to -1 to allow for infinite uses \n" +
                                       "Set to 1-> X to set maximum amount of uses per mission"));

                    ImGui.PopID();
                }

                #endregion

                #endregion

                #endregion

                ImGui.EndTable();
            }

            ImGui.Separator();
            if (ImGui.Button(T("Copy Selected Profile")))
            {
                string export = ExportGatherProfile(C.SelectedGatherIndex);
                ImGui.SetClipboardText(export);
            }

            if (ImGui.Button(T("Import Selected Profile")))
            {
                string importProfile = ImGui.GetClipboardText();
                string errorMessage = "";
                ImportGatherProfile(importProfile, out errorMessage);
                if (errorMessage != "")
                {
                    IceLogging.Error(errorMessage);
                }
                C.Save();
            }

            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Separator();

            ImGui.Dummy(new Vector2(0, 10));

            using (ImRaii.Disabled(!ImGui.IsKeyDown(ImGuiKey.LeftShift)))
            {
                if (ImGui.Button(T("Setup Gathering Profiles")))
                {
                    SetupAllProfiles();

                    C.Save();
                }
            }
            ImGuiEx.HelpMarker(T("PLEASE NOTE:\n" +
                               "This will wipe out all your current profiles, and apply what I would suggest for each one.\n" +
                               "For most of you this would be fine, this is really only here if you don't know what to apply for each one.\n" +
                               "If you're okay with this, hold left shift and apply"));
        }

        public static void SetupAllProfiles()
        {
            foreach (var profile in C.GatherProfiles)
            {
                if (profile.Key == 0)
                    continue;
                else
                {
                    C.GatherProfiles.Remove(profile.Key);
                    foreach (var mission in C.MissionConfig)
                    {
                        if (mission.Value.GProfileId == profile.Key)
                        {
                            mission.Value.GProfileId = 0; // fallback to default
                        }
                    }
                }
            }

            string timedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IlRpbWVkIE1pc3Npb25zIiwiTWluaW11bUdwIjoxMDAsIkR1YWxDbGFzc0NyYWZ0QW1vdW50IjoxLCJHYXRoZXJCdWZmcyI6eyJCdWZmcyI6eyJCb29uSW5jcmVhc2UyIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9vbkluY3JlYXNlMSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
            string limitedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkxpbWl0ZWQgTm9kZXMiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIlRpZGluZ3MiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyMDAsIk1heFVzZSI6LTF9LCJZaWVsZElJIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjo1MDAsIk1heFVzZSI6LTF9LCJZaWVsZEkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo0MDAsIk1heFVzZSI6LTF9LCJCb3VudGlmdWxZaWVsZElJIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjMwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Q2hhbmNlIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjowLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5VGVtcCI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9fSwiQm91bnRpZnVsTWluSXRlbSI6NH19";
            string chainedMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkNoYWluZWQiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb29uSW5jcmVhc2UxIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9LCJUaWRpbmdzIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjAwLCJNYXhVc2UiOi0xfSwiWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
            string DualClass = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkR1YWwgQ2xhc3MiLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjIsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";
            string boonMissions = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkJvb24iLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIlRpZGluZ3MiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyMDAsIk1heFVzZSI6LTF9LCJZaWVsZElJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAwLCJNYXhVc2UiOi0xfSwiWWllbGRJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NDAwLCJNYXhVc2UiOi0xfSwiQm91bnRpZnVsWWllbGRJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjozMDAsIk1heFVzZSI6LTF9LCJCb251c0ludGVncml0eUNoYW5jZSI6eyJFbmFibGVkIjp0cnVlLCJNaW5HcCI6MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjI1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5VGVtcCI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfX0sIkJvdW50aWZ1bE1pbkl0ZW0iOjR9fQ==";
            string ChainBoonMission = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkNoYWluZWQgXHUwMDJCIEJvb24iLCJNaW5pbXVtR3AiOjEwMCwiRHVhbENsYXNzQ3JhZnRBbW91bnQiOjEsIkdhdGhlckJ1ZmZzIjp7IkJ1ZmZzIjp7IkJvb25JbmNyZWFzZTIiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvb25JbmNyZWFzZTEiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MDAsIk1heFVzZSI6LTF9LCJZaWVsZEkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo0MDAsIk1heFVzZSI6LTF9LCJCb3VudGlmdWxZaWVsZElJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjMwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Q2hhbmNlIjp7IkVuYWJsZWQiOnRydWUsIk1pbkdwIjowLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MjUwLCJNYXhVc2UiOi0xfSwiRmllbGRNYXN0ZXJ5SUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoxMDAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlUZW1wIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6NTAsIk1heFVzZSI6LTF9fSwiQm91bnRpZnVsTWluSXRlbSI6NH19";
            string GatherXAmount = "IceGatherProfile_eyJJZCI6MCwiTmFtZSI6IkdhdGhlciBYIEFtb3VudCIsIk1pbmltdW1HcCI6LTEsIkR1YWxDbGFzc0NyYWZ0QW1vdW50IjoxLCJHYXRoZXJCdWZmcyI6eyJCdWZmcyI6eyJCb29uSW5jcmVhc2UyIjp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MTAwLCJNYXhVc2UiOi0xfSwiQm9vbkluY3JlYXNlMSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjUwLCJNYXhVc2UiOi0xfSwiVGlkaW5ncyI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjIwMCwiTWF4VXNlIjotMX0sIllpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjUwMCwiTWF4VXNlIjotMX0sIllpZWxkSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjQwMCwiTWF4VXNlIjotMX0sIkJvdW50aWZ1bFlpZWxkSUkiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkJvbnVzSW50ZWdyaXR5Ijp7IkVuYWJsZWQiOmZhbHNlLCJNaW5HcCI6MzAwLCJNYXhVc2UiOi0xfSwiQm9udXNJbnRlZ3JpdHlDaGFuY2UiOnsiRW5hYmxlZCI6dHJ1ZSwiTWluR3AiOjAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjoyNTAsIk1heFVzZSI6LTF9LCJGaWVsZE1hc3RlcnlJSSI6eyJFbmFibGVkIjpmYWxzZSwiTWluR3AiOjEwMCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeUkiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX0sIkZpZWxkTWFzdGVyeVRlbXAiOnsiRW5hYmxlZCI6ZmFsc2UsIk1pbkdwIjo1MCwiTWF4VXNlIjotMX19LCJCb3VudGlmdWxNaW5JdGVtIjo0fX0=";

            GatherSettings.InitialSetupProfile(timedMissions, "timed", out var _);
            GatherSettings.InitialSetupProfile(limitedMissions, "limited", out var _);
            GatherSettings.InitialSetupProfile(chainedMissions, "chained", out var _);
            GatherSettings.InitialSetupProfile(boonMissions, "boon", out var _);
            GatherSettings.InitialSetupProfile(ChainBoonMission, "boonChain", out var _);
            GatherSettings.InitialSetupProfile(DualClass, "dualCraft", out var _);
            GatherSettings.InitialSetupProfile(GatherXAmount, "gatherX", out var _);
        }
    }
}
