using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Ui.MainUi.ModeSelect_Modes;
using ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Text;
using static MissionTimer;

namespace ICE.Ui
{
    internal class Window_ExternalDetails : Window
    {
        public static uint SelectedMission = 0;

        public static List<string> JokeList = new()
        {
            "What is a pirates favorite letter?\n" +
            "You might thing it's R, but tis first love was the C\n" +
            "(It helps if you verbally say it like a pirate)",

            "You know, I was reading this book about anti-gravity recently,\n" +
            "and honestly I'm having a hard time putting it down",

            "Why are tennis pros always hugging each other?\n" +
            "Because they start their match at \"Love All\"",

            "Why can't ghost have babies?\n" +
            "Because they have hallow-eenies",

            "How do you save a drowning pirate?\n" +
            "You give him Cprrrrrr",

            "What is a skeleton's favorite snack?\n" +
            "Ribs! Spare Ribs!",

            "Honestly, just wanted to say thank you for using my plugin, you're appreciated <3",

            "Knock knock\n" +
            "[This is where you say who's there]\n" +
            "Lettuce\n" +
            "[Lettuce who]\n" +
            "Lettuce in",

            "What do you a dinosaur that only has one eye?" +
            "A \"Doyouthinkheseemesaurs\"",

            "So... you're telling me a shrimp fried this rice?",

            "Thank you everyone who's helped make this possible.\n" +
            "Strife special shoutout to you for doing what I didn't want to with fishing\n" +
            "(Sorry for making you start big fish #NotSorry#MuchLove)\n" +
            "Wah thank you for the UI, this is fucking beautiful as always\n" +
            "Puni.sh in general for each one of your help my dumb questions"
        };
        public static int jokeId = 0;

        public Window_ExternalDetails() : base($"Ice's Cosmic Exploration | Mission Details")
        {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(500, 500)
            };
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override void OnOpen()
        {
            Collapsed = false;
            BringToFront();
            CollapsedCondition = ImGuiCond.Appearing;
        }

        public override void Draw()
        {
            DrawMissionDetails();
        }
        public static void DrawMissionDetails()
        {
            if (CosmicHelper.SheetMissionDict.TryGetValue(SelectedMission, out var mission))
            {
                var id = SelectedMission;
                ImGui.PushID($"{mission}_{id}");

                #region Mission Name

                ImGui.Text($"Mission:");
                ImGui.SameLine(0, 5);
                ImGui.TextDisabled($"[{id}]");
                ImGui.SameLine(0, 5);
                ImGui.Text($"{mission.Name}");

                #endregion

                if (ImGui.BeginTable("Detailed Mission Info", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Info");

                    // Row 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(T("Cosmocredits"));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{mission.CosmoCredit}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Planetary Credits");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{mission.LunarCredit}");

                    if (mission.DronebitReward != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (Svc.Texture.TryGetFromGameIcon(65138, out var dronebitIcon))
                        {
                            ImGui.Image(dronebitIcon.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Image(dronebitIcon.GetWrapOrEmpty().Handle, new Vector2(40, 40));
                                ImGui.EndTooltip();
                            }
                            ImGui.SameLine();
                        }
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"Dronebits");

                        ImGui.TableNextColumn();
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"{mission.DronebitReward}");
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Class Score:");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{mission.ClassScore}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"Job(s)");

                    ImGui.TableNextColumn();
                    foreach (var job in mission.Jobs)
                    {
                        ISharedImmediateTexture? icon = CosmicHelper.ClassInfoDict[job].JobIcon;
                        Vector2 size = new Vector2(20, 20);
                        ImGui.Image(icon.GetWrapOrEmpty().Handle, size);
                        ImGui.SameLine();
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"Completed:");

                    ImGui.TableNextColumn();
                    ImGui_Ice.CompletionStatusIcon(mission);

                    if (mission.BronzeScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"Bronze Requirement");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.BronzeScore}");
                    }
                    if (mission.SilverScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"Silver Requirement");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.SilverScore}");
                    }
                    if (mission.GoldScore != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(T("Gold Requirement"));

                        ImGui.TableNextColumn();
                        ImGui.Text($"{mission.GoldScore}");
                    }

                    if (mission.MarkerId != 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(T("Gathering Zone"));

                        ImGui.TableNextColumn();

                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.Text(FontAwesomeIcon.Flag.ToIconString());
                        ImGui.PopFont();
                        if (ImGui.IsItemClicked())
                        {
                            Utils.SetGatheringRing(mission.TerritoryId, (int)mission.MapPosition.X, (int)mission.MapPosition.Y, mission.Radius, mission.Name);
                        }
                    }

                    if (CosmicHelper.CriticalLocations.TryGetValue(SelectedMission, out var criticalLoc))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(T("Critical Area"));

                        ImGui.TableNextColumn();
                        ImGuiEx.Icon(FontAwesomeIcon.Flag);
                        if (ImGui.IsItemClicked())
                        {
                            Utils.SetFlagForNPC(mission.TerritoryId, criticalLoc.MapInfo.X, criticalLoc.MapInfo.Y);
                        }
                    }

                    ImGui.EndTable();
                }

                if (ImGui.BeginTable("Relic Exp Info Table", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Relix Exp Kind");
                    ImGui.TableSetupColumn("Amount");

                    ImGui.TableHeadersRow();

                    foreach (var xp in mission.RelicXpInfo.OrderByDescending(x => x.Key))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        string type = "";
                        switch (xp.Key)
                        {
                            case 1:
                                type = "I";
                                break;
                            case 2:
                                type = "II";
                                break;
                            case 3:
                                type = "III";
                                break;
                            case 4:
                                type = "IV";
                                break;
                            case 5:
                                type = "V";
                                break;
                            case 6:
                                type = "VI";
                                break;
                            case 7:
                                type = "VII";
                                break;
                            default:
                                type = "???";
                                break;
                        }

                        ImGui.Text($"Lv. {type}");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{xp.Value}");
                    }

                    ImGui.EndTable();
                }

                if (mission.ExpModifier_3 != 0)
                {
                    if (ImGui.BeginTable("Exp Rewards", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                    {
                        ImGui.TableSetupColumn("Class Exp");
                        ImGui.TableSetupColumn("% of Level");

                        ImGui.TableHeadersRow();

                        if (mission.ExpModifier_1 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text(T("Lv. 10-49"));

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mission.ExpModifier_1}%");
                        }

                        if (mission.ExpModifier_2 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text(T("Lv. 50-89"));

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mission.ExpModifier_2}%");
                        }

                        if (mission.ExpModifier_3 != 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text(T("Lv. 90-99"));

                            ImGui.TableNextColumn();
                            ImGui.Text($"{mission.ExpModifier_3}%");
                        }

                        ImGui.EndTable();
                    }
                }

                if (mission.Crafts_Main.Count > 0)
                {
                    ImGui_Ice.WindowSpacer();

                    Mission_Table.CrafterManagement(mission, id);
                }

                ImGui_Ice.WindowSpacer();

                ImGui.Text(T("Mission Atributes"));
                if (mission.Attributes == MissionAttributes.None)
                {
                    ImGui.Text(T("None"));
                    return;
                }
                else
                {
                    foreach (MissionAttributes flag in Enum.GetValues<MissionAttributes>())
                    {
                        if (flag != MissionAttributes.None && mission.Attributes.HasFlag(flag))
                        {
                            ImGui.Text($"{EnumNameConverter(flag)}");
                        }
                    }
                }

                if (CosmicHelper.MissionUnlock.TryGetValue(SelectedMission, out var unlock))
                {
                    ImGui.Text(T("The following missions are required to have gold before you can do this one"));
                    foreach (var lockedMission in unlock)
                    {
                        ImGui_Ice.CompletionStatusIcon(CosmicHelper.SheetMissionDict[lockedMission]);
                        ImGui.SameLine();
                        ImGui.Text($"[{lockedMission}] - {CosmicHelper.SheetMissionDict[lockedMission].Name}");
                    }

                }

                ImGui_Ice.WindowSpacer();
                ImGui.Text($"Mission Times!");

                if (C.MissionConfig.TryGetValue(SelectedMission, out var config))
                {
                    bool allowDelete = (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift)) && (ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl));

                    using (ImRaii.Disabled(!allowDelete))
                    {
                        if (ImGui.Button(T("Reset Stats")))
                        {
                            P.MissionTimer.ResetTimers(SelectedMission);
                        }
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Hold Shift + Control"));
                        ImGui.EndTooltip();
                    }

                    if (config.TurninRecords.Count > 0)
                    {
                        ImGui.Text($"Best Time: {TimeSpan.FromSeconds(config.BestTime):mm\\:ss\\.ff}");
                        ImGui.Text($"Average Time: {TimeSpan.FromSeconds(config.AverageTime):mm\\:ss\\.ff}");
                    }
                    else
                    {
                        ImGui.Text(T("Best Time: --:--:--"));
                        ImGui.Text(T("Average Time: --:--:--"));
                    }

                    ImGui.Text($"Times Completed: {config.TotalCompletions}");
                    ImGui.Text($"Times Attempted: {config.TotalAttempts}");

                    if (CosmicHelper.SheetMissionDict.TryGetValue(SelectedMission, out var missionInfo))
                    {
                        var baseScore = missionInfo.ClassScore;
                        var comsoCredit = missionInfo.CosmoCredit;
                        var planetCredit = missionInfo.LunarCredit;

                        ImGui.Separator();
                        ImGui.Text(T("Estimated Score Per Hour:"));
                        ImGui.SameLine();
                        ImGui.TextDisabled("?");
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text(T("This is ASSUMING:"));
                            ImGui.Text(T("1: You have immaculate rng of getting the mission you want every time"));
                            ImGui.Text(T("2: You're hitting the threshold every time"));
                            ImGui.Text("This is based on your average time.\n" +
                                       "So get a good couple of runs to get a good feel for the timing");
                            ImGui.EndTooltip();
                        }
                        if (ImGui.BeginTable("Score Info: External Details", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                        {
                            foreach (var entry in missionInfo.ScoreInfo().Where(x => x.Value.Score != 0))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.Text($"{entry.Key} [{entry.Value.Completions:N0}]");

                                ImGui.TableNextColumn();
                                ImGui.Text($"{entry.Value.Score:N2}");

                                ImGui.TableNextColumn();
                                ImGui.Text($"{entry.Value.Cosmocredit:N2}");

                                ImGui.TableNextColumn();
                                ImGui.Text($"{entry.Value.PlanetCredits:N2}");

                                ImGui.TableNextColumn();
                                string tokens = entry.Value.Tokens > 0 ? $"{entry.Value.Tokens:N2}" : "-";
                                ImGui.Text(tokens);
                            }

                            ImGui.EndTable();
                        }
                    }
                }

                ImGui.PopID();
            }
            else
            {
                string joke = JokeList[jokeId];
                ImGui.TextWrapped(joke);
            }
        }

        public static string EnumNameConverter(MissionAttributes attribute)
        {
            return attribute switch
            {
                MissionAttributes.Craft => "Crafting",
                MissionAttributes.Gather => "Gathering",
                MissionAttributes.Fish => "Fishing",
                MissionAttributes.Limited => "Limited Supplies",
                MissionAttributes.Collectables => "Collectable",
                MissionAttributes.ReducedItems => "Reducable Items",
                MissionAttributes.ExpertCraft => "Expert Crafts",
                MissionAttributes.Score_TimeRemaining => "Timed Scoring",
                MissionAttributes.Score_Chain => "Chained Gather Scoring",
                MissionAttributes.Score_Boon => "Gatherer's Boons Scoring",
                MissionAttributes.Score_LargestSize => "Largest Fish Scored",
                MissionAttributes.Score_Variety => "Variety of Fish Required",
                MissionAttributes.Score_MinimumScore => "Mission Score Required",
                MissionAttributes.Critical => "Critical Mission",
                MissionAttributes.ProvisionalTimed => "Time Required",
                MissionAttributes.ProvisionalWeather => "Weather Required",
                MissionAttributes.ProvisionalSequential => "Sequential Missions Required",
                _ => attribute.ToString()
            };
        }
    }
}
