using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Hud_Mission
    {
        private static int BestMission = 0;
        private static string MissionName = "";

        public static List<int> XpKinds = new() { 1, 2, 3, 4, 5 };

        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var x) && x.IsAddonReady)
            {
                ImGui.Text("List of Visible Missions");
                ImGui.Text($"Selected Mission Name: {x.SelectedMissionName}");
                ImGui.Text($"Selected Mission ID: {x.SelectedMissionId}");

                if (ImGui.Button("Help"))
                {
                    x.Help();
                }
                ImGui.SameLine();

                if (ImGui.Button("Mission Selection"))
                {
                    x.MissionSelection();
                }
                ImGui.SameLine();

                if (ImGui.Button("Mission Log"))
                {
                    x.MissionLog();
                }
                ImGui.SameLine();

                if (ImGui.Button("Basic Missions"))
                {
                    x.BasicMissions();
                }
                ImGui.SameLine();

                if (ImGui.Button("Provisional Missions"))
                {
                    x.ProvisionalMissions();
                }
                ImGui.SameLine();

                if (ImGui.Button("Critical Missions"))
                {
                    x.CriticalMissions();
                }

                if (ImGui.Button("Test Mission List"))
                {
                    Mission_Settings.SelectedJob = (uint)Player.Job;
                    Mission_Settings.Mode = C.SelectedMode;
                    Task_CheckMissions.RefreshMissionLibrary();
                }

                bool EnableDummyXp = C.UseDummyXp;
                if (ImGui.Checkbox("Enable Dummy XP", ref EnableDummyXp))
                {
                    C.UseDummyXp = EnableDummyXp;
                    C.Save();
                }

                for (int i = 0; i < x.SelectClass.Length; i++)
                {
                    if (i != 0)
                        ImGui.SameLine();
                    
                    if (ImGui.Button($"[{i}]"))
                    {
                        x.SelectClass[i].Select();
                    }
                }

                bool IgnoreManual = C.XPRelicIgnoreManual;
                bool onlyEnabled = C.XPRelicOnlyEnabled;
                if (ImGui.Checkbox("Ignore Manual Mode", ref IgnoreManual))
                {
                    C.XPRelicIgnoreManual = IgnoreManual;
                    C.Save();
                }
                if (ImGui.Checkbox("Only Enabled Missions", ref onlyEnabled))
                {
                    C.XPRelicOnlyEnabled = onlyEnabled;
                    C.Save();
                }

                if (ImGui.Button("Update Dummy XP"))
                {
                    foreach (var kind in XpKinds)
                    {
                        if (!C.DummyXP.ContainsKey(kind))
                        {
                            C.DummyXP[kind] = new()
                            {
                                CurrentXP = 0,
                                NeededXP = 1,
                            };
                            C.Save();
                        }
                    }
                }

                foreach (var key in C.DummyXP.Keys.ToList())
                {
                    var xp = C.DummyXP[key];

                    ImGui.Text($"XP: {key}");
                    ImGui.PushID(key);

                    int currentXP = xp.CurrentXP;
                    int neededXP = xp.NeededXP;

                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Current XP", ref currentXP))
                    {
                        xp.CurrentXP = currentXP;
                        C.Save();
                    }

                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Needed XP", ref neededXP))
                    {
                        xp.NeededXP = neededXP;
                        C.Save();
                    }

                    ImGui.PopID();

                    ImGui.Separator();
                }

                foreach (var m in x.StellerMissions)
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"[{m.MissionId}] {m.Name}");
                    ImGui.SameLine();
                    if (ImGui.Button($"Select###Select + {m.Name}"))
                    {
                        m.Select();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Initiate##Initiate + {m.Name}"))
                    {
                        m.Initiate();
                    }
                }

                ImGui.Text($"Best Relic Mission: {BestMission} | {MissionName}");
                if (ImGui.Button("Update Best Mission"))
                {
                    BestMission = (int)RelicMissionFinder();
                    if (BestMission < 1)
                        MissionName = "None";
                    else
                    {
                        MissionName = CosmicHelper.SheetMissionDict[(uint)BestMission].Name;
                    }
                }


            }
            else
            {
                ImGui.Text("Waiting for \"WKSMission\" to be visible");
            }
        }

        private static unsafe uint RelicMissionFinder()
        {
            if (GenericHelpers.TryGetAddonMaster<WKSMission>("WKSMission", out var missionInfo) && missionInfo.IsAddonReady)
            {
                var job = Mission_Settings.SelectedJob;
                var relicInfo = CosmicHelper.Cosmic_ClassInfo();
                var classInfo = relicInfo[job];

                var urgency = new Dictionary<int, float>();
                if (C.UseDummyXp)
                {
                    foreach (var exp in C.DummyXP)
                    {
                        urgency[exp.Key] = 1f - exp.Value.CurrentXP / exp.Value.NeededXP;
                    }
                }
                else
                {
                    foreach (var exp in classInfo.CurrentExp)
                    {
                        if (classInfo.Stage_Current != classInfo.Stage_Next)
                            urgency[exp.Key] = exp.Value.Needed > 0 ? 1f - (float)exp.Value.Current / exp.Value.Needed : 0f;
                        else
                            urgency[exp.Key] = 1f - (float)exp.Value.Current / exp.Value.Max;
                    }
                }
                IceLogging.Verbose($"Urgency Exp Values");
                foreach (var exp in urgency)
                {
                    IceLogging.Verbose($"{exp.Key} : Value: {exp.Value:N2}");
                }

                List<uint> missionList = new();
                foreach (var mission in C.MissionConfig)
                {
                    if (CosmicHelper.SheetMissionDict.TryGetValue(mission.Key, out var sheetInfo))
                    {
                        if (sheetInfo.TerritoryId != Player.Territory.RowId)
                            continue;

                        if (!sheetInfo.Jobs.Contains((uint)Player.Job))
                            continue;

                        if (sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalWeather) || sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalSequential)
                        || sheetInfo.Attributes.HasFlag(MissionAttributes.ProvisionalTimed))
                            continue;

                        missionList.Add(mission.Key);
                    }
                }

                IceLogging.Verbose($"Total Mission Count: {missionList.Count()}");


                uint? bestMissionId = null;
                float bestScore = float.NegativeInfinity;
                foreach (var missionId in missionList)
                {
                    IceLogging.Verbose($"Seeing if menu contains: {missionId}");
                    var mission = missionInfo.StellerMissions.Where(x => x.MissionId == missionId).FirstOrDefault();
                    if (mission != null)
                    {
                        IceLogging.Verbose($"Mission was valid option: {missionId}");
                        if (CosmicHelper.SheetMissionDict.TryGetValue(mission.MissionId, out var sheetInfo))
                        {
                            float score = 0;
                            foreach (var reward in sheetInfo.RelicXpInfo)
                            {
                                if (urgency.TryGetValue(reward.Key, out var info))
                                {
                                    float contribution = info * reward.Value;
                                    if (contribution > 0)
                                    {
                                        score += contribution;
                                    }
                                }
                            }
                            IceLogging.Verbose($"[{mission.MissionId}] score: {score}");
                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestMissionId = missionId;
                            }
                        }
                    }
                }

                if (bestMissionId != null)
                {
                    return bestMissionId.Value;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                IceLogging.Info("We're somehow not showing the window, so returning 0.");

                return 0;
            }
        }
    }
}
