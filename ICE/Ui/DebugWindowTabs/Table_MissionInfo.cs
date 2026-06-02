using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Utilities.Cosmic_Helper;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Table_MissionInfo
    {
        private static string CraftingTableSearchText = "";
        private static string AttributeSearchText = "";
        private static uint RankSearch = 0;
        private static uint jobSearch = 7;
        private static string exportPath = "";
        private static string statusMessage = "";

        private static readonly Dictionary<uint, string> JobAbbreviations = new()
        {
            [8] = "CRP",   // Carpenter
            [9] = "BSM",   // Blacksmith
            [10] = "ARM",  // Armorer
            [11] = "GSM",  // Goldsmith
            [12] = "LTW",  // Leatherworker
            [13] = "WVR",  // Weaver
            [14] = "ALC",  // Alchemist
            [15] = "CUL",  // Culinarian
            [16] = "MIN",  // Miner
            [17] = "BTN",  // Botanist
            [18] = "FSH",  // Fisher
        };

        private static FileDialogManager fileDialogManager = new FileDialogManager();

        public static unsafe void Draw()
        {
            var itemSheet = ExcelHelper.ItemSheet;
            ImGui.SetNextItemWidth(250);
            ImGui.InputText(T("Search by Name"), ref CraftingTableSearchText, 100);
            ImGui.SetNextItemWidth(250);
            ImGui.InputText(T("Search by Attribute"), ref AttributeSearchText, 100);
            ImGui.SetNextItemWidth(250);
            ImGui.SliderUInt(T("Rank ID"), ref RankSearch, 0, 6);
            ImGui.SetNextItemWidth(250);
            ImGui.SliderUInt(T("Class Selection"), ref jobSearch, 7, 18);
            if (ImGui.Button(T("Copy Scores")))
            {
                ImGui.SetClipboardText(GenerateMissionScoreDictionaryCode());
            }
            ImGui.SameLine();

            if (ImGui.Button(T("Export Fishing Missions")))
            {
                var fishingMissions = CosmicHelper.SheetMissionDict
                    .Where(kvp => kvp.Value.Attributes.HasFlag(MissionAttributes.Fish)) // Adjust flag name as needed
                    .OrderBy(kvp => kvp.Key) // Optional: sort by mission ID
                    .Select(kvp => $"    [{kvp.Key}] = \"\",")
                    .ToArray();

                if (fishingMissions.Length > 0)
                {
                    var result = string.Join(Environment.NewLine, fishingMissions);

                    // Copy to clipboard
                    ImGui.SetClipboardText(result);

                    // Optional: Show a tooltip or notification
                    // You could also use a popup or status message here
                    ImGui.SetTooltip($"Copied {fishingMissions.Length} fishing missions to clipboard!");
                }
                else
                {
                    ImGui.SetTooltip(T("No fishing missions found!"));
                }
            }

            ImGui.SameLine();

            if (ImGui.Button(T("Clear stored scores")))
            {
                C.ScoreKeeper.Clear();
                C.Save();
            }

            ImGui.SetNextItemWidth(200);
            ImGui.InputText("##ExportPath", ref exportPath, 500);

            ImGui.SameLine();
            if (ImGui.Button(T("Browse...")))
            {
                fileDialogManager.SaveFileDialog(
                    "Select Export Location",
                    ".csv",
                    "mission_scores.csv",
                    ".csv",
                    (success, path) =>
                    {
                        if (success)
                        {
                            exportPath = path;
                        }
                    }
                );
            }

            ImGui.SameLine();
            if (ImGui.Button(T("Export CSV")))
            {
                ExportToCsv();
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                ImGui.TextColored(
                    statusMessage.Contains("Success") ? new System.Numerics.Vector4(0, 1, 0, 1) : new System.Numerics.Vector4(1, 0, 0, 1),
                    statusMessage
                );
            }

            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                            ImGuiTableFlags.Borders |
                            ImGuiTableFlags.SizingFixedFit |
                            ImGuiTableFlags.Resizable |           // Allow column resizing
                            ImGuiTableFlags.Reorderable |         // Allow column reordering
                            ImGuiTableFlags.Hideable;             // Allow hiding columns via right-click

            if (ImGui.BeginTable("Moon Mission Information Table", 35, tableFlags)) // Increased column count by 1
            {
                ImGui.TableSetupColumn(T("ID"));
                ImGui.TableSetupColumn(T("Jobs"));

                ImGui.TableSetupColumn(T("Mission Name"));
                ImGui.TableSetupColumn(T("Job"));
                ImGui.TableSetupColumn(T("2nd Job"));
                ImGui.TableSetupColumn(T("Rank"));
                ImGui.TableSetupColumn(T("ToDo ID"));
                ImGui.TableSetupColumn(T("Bronze"));
                ImGui.TableSetupColumn(T("Silver"));
                ImGui.TableSetupColumn(T("Gold"));
                ImGui.TableSetupColumn(T("Attribute Flags"));

                IOrderedEnumerable<KeyValuePair<int, string>> orderedExp = CosmicHelper.ExpDictionary.ToList().OrderBy(exp => exp.Key);
                var agent = AgentMap.Instance();
                var wk = WKSManager.Instance();

                foreach (var exp in orderedExp)
                {
                    ImGui.TableSetupColumn($"{exp.Value}", ImGuiTableColumnFlags.WidthFixed, -1);
                }

                ImGui.TableSetupColumn(T("Test Flag"));

                ImGui.TableSetupColumn(T("Score"));

                ImGui.TableSetupColumn(T("Main Item 1"));
                ImGui.TableSetupColumn(T("Amount #1"));
                ImGui.TableSetupColumn(T("Main Item 2"));
                ImGui.TableSetupColumn(T("Amount #2"));
                ImGui.TableSetupColumn(T("Main Item 3"));
                ImGui.TableSetupColumn(T("Amount #3"));
                ImGui.TableSetupColumn(T("Pre-Craft Item"));
                ImGui.TableSetupColumn(T("Pre-Craft Amount"));
                ImGui.TableSetupColumn(T("Export")); // New column for export button
                for (int i = 1; i < 4; i++)
                {
                    ImGui.TableSetupColumn($"Gather [{i}]");
                    ImGui.TableSetupColumn($"Amount [G-{i}]");
                }
                ImGui.TableSetupColumn(T("Completion"));
                ImGui.TableSetupColumn(T("Activate Mission"));

                ImGui.TableHeadersRow();

                foreach (var entry in CosmicHelper.SheetMissionDict)
                {
                    // Case-insensitive name search
                    if (!string.IsNullOrEmpty(CraftingTableSearchText) &&
                        !entry.Value.Name.ToLower().Contains(CraftingTableSearchText.ToLower()))
                        continue;

                    // Case-insensitive attribute search
                    string attributedText = entry.Value.Attributes.ToString().ToLower();
                    if (!string.IsNullOrEmpty(AttributeSearchText) &&
                        !attributedText.Contains(AttributeSearchText.ToLower()))
                        continue;

                    if (RankSearch != 0 && entry.Value.Rank != RankSearch)
                        continue;

                    if (jobSearch != 7)
                    {
                        var jobs = entry.Value.Jobs;

                        if (!jobs.Contains(jobSearch))
                            continue;
                    }

                    ImGui.TableNextRow();

                    ImGui.PushID(entry.Key);

                    // Mission ID
                    ImGui.TableSetColumnIndex(0);
                    if (ImGui.Button($"{entry.Key}"))
                    {
                        ImGui.SetClipboardText($"{entry.Key}");
                    }

                    // Job Icons (for quick reference)
                    ImGui.TableNextColumn();

                    Vector2 size = new Vector2(20, 20);

                    int index = 0;
                    foreach (var jobId in entry.Value.Jobs)
                    {
                        ISharedImmediateTexture? icon = CosmicHelper.ClassInfoDict[jobId].JobIcon;
                        ImGui.Image(icon.GetWrapOrEmpty().Handle, size);

                        if (index < entry.Value.Jobs.Count - 1) // Not the only job, adding a sameline for it
                            ImGui.SameLine();

                        index++;
                    }

                    // Mission Name
                    ImGui.TableNextColumn();
                    ImGui.Text(entry.Value.Name);
                    if (ImGui.IsItemClicked())
                    {
                        ImGui.SetClipboardText(RemovePrivateUseChars(entry.Value.Name));
                    }

                    // JobId Attached to it
                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.Value.Jobs.First()}");

                    // 2nd Job for quest
                    ImGui.TableNextColumn();
                    if (entry.Value.Jobs.Count > 1)
                    {
                        ImGui.Text($"{entry.Value.Jobs.Last()}");
                    }
                    else
                    {
                        ImGui.Text($"0");
                    }

                    // Rank of the mission
                    ImGui.TableNextColumn();
                    string rank = "";
                    if (entry.Value.Rank == 1)
                        rank = "D";
                    else if (entry.Value.Rank == 2)
                        rank = "C";
                    else if (entry.Value.Rank == 3)
                        rank = "B";
                    else if (entry.Value.Rank == 4)
                        rank = "A";
                    else if (entry.Value.Rank == 5)
                        rank = "EX";
                    else if (entry.Value.Rank == 6)
                        rank = "EX+";
                    else
                    {
                        rank = entry.Value.Rank.ToString();
                    }
                    ImGui.Text($"{rank}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.Value.ToDoId}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.Value.BronzeScore}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.Value.SilverScore}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.Value.GoldScore}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{entry.Value.Attributes}");

                    foreach (var expType in orderedExp)
                    {
                        var type = expType.Key;
                        ImGui.TableNextColumn();
                        if (entry.Value.RelicXpInfo.TryGetValue(type, out var amount))
                        {
                            ImGui.Text($"{amount}");
                        }
                        else
                        {
                            ImGui.Text($"-");
                        }
                    }

                    ImGui.TableNextColumn();
                    if (entry.Value.MarkerId != 0)
                    {
                        if (ImGui.Button($"Flag###Flag-{entry.Key}"))
                        {
                            Utils.SetGatheringRing(entry.Value.TerritoryId, (int)entry.Value.MapPosition.X, (int)entry.Value.MapPosition.Y, entry.Value.Radius);
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text($"X: {entry.Value.MapPosition.X} | Y: {entry.Value.MapPosition.Y}");
                            ImGui.EndTooltip();
                        }
                        ImGui.SameLine();
                        if (ImGui.Button($"Copy Flag##Flag-{entry.Key}"))
                        {
                            ImGui.SetClipboardText($"{entry.Value.MapPosition.X}, {entry.Value.MapPosition.Y}");
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text($"X: {entry.Value.MapPosition.X} | Y: {entry.Value.MapPosition.Y}");
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.TableNextColumn();
                    uint missionScore = entry.Value.ClassScore;
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputUInt($"##MissionScore", ref missionScore))
                    {
                        entry.Value.ClassScore = missionScore;
                    }

                    // 27-29
                    foreach (var item in entry.Value.Gathering_Min)
                    {
                        ImGui.TableNextColumn();
                        var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name;
                        ImGui.Text($"{itemName}");
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text(T("Id: {0}", item.Key));
                            ImGui.EndTooltip();
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{item.Value}");
                    }

                    ImGui.TableSetColumnIndex(33);
                    var manager = WKSManager.Instance();
                    var isCompleted = manager->IsMissionCompleted(entry.Key);
                    var isGold = manager->IsMissionGolded(entry.Key);
                    Completion(entry.Key);

                    ImGui.TableSetColumnIndex(34);
                    if (CosmicHelper.CurrentLunarMission != 0)
                    {
                        if (ImGui.Button(T("Turn in")))
                        {
                            ReportMission();
                        }
                        ImGui.SameLine();
                        if (ImGui.Button(T("Abandon")))
                        {
                            AbandonMission();
                        }
                    }
                    else
                    {
                        if (ImGui.Button(T("Initiate")))
                        {
                            InitiateMission(entry.Key);
                        }
                    }

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }

            // IMPORTANT: Add this at the end of your Draw() method, before the last closing brace
            fileDialogManager.Draw();
        }

        public static string GenerateMissionScoreDictionaryCode()
        {
            var sb = new StringBuilder();
            sb.AppendLine("public static Dictionary<uint, uint> MissionScoreDict = new Dictionary<uint, uint>");
            sb.AppendLine("{");

            foreach (var kvp in CosmicHelper.SheetMissionDict)
            {
                sb.AppendLine($"    [{kvp.Key}] = {kvp.Value.ClassScore},");
            }

            sb.AppendLine("};");

            return sb.ToString();
        }

        public static string RemovePrivateUseChars(string input)
        {
            var result = new StringBuilder();
            foreach (char c in input)
            {
                int code = (int)c;
                // Skip Private Use Areas where games often store custom symbols
                if (!(code >= 0xE000 && code <= 0xF8FF) &&    // Private Use Area
                    !(code >= 0xF0000 && code <= 0xFFFFD) &&  // Supplementary Private Use Area A
                    !(code >= 0x100000 && code <= 0x10FFFD))  // Supplementary Private Use Area B
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        private static unsafe void Completion(uint id)
        {
            var manager = WKSManager.Instance();
            var isCompleted = manager->IsMissionCompleted(id);
            var isGold = manager->IsMissionGolded(id);

            if (isCompleted)
            {
                if (isGold)
                {
                    var starTex = Svc.Texture.GetFromGame("ui/uld/linkshell_hr1.tex").GetWrapOrEmpty();
                    Vector2 uvMin = new Vector2(0.027825013f, 0.04166667f);
                    Vector2 uvMax = new Vector2(0.305575f, 0.4583333f);

                    ImGui.Image(starTex.Handle, new Vector2(18, 18), uvMin, uvMax);
                }
                else
                {
                    FontAwesome.Print(EColor.Green, FontAwesome.Check);
                }
            }
            else
            {
                FontAwesome.Print(EColor.Red, FontAwesome.Cross);
            }
        }

        private static void ExportToCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(exportPath))
                {
                    statusMessage = "Error: Please specify an export path";
                    return;
                }

                if (!exportPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    exportPath += ".csv";

                var directory = Path.GetDirectoryName(exportPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var csv = new StringBuilder();
                csv.AppendLine("MissionID,Class,MissionName,Score");

                foreach (var mission in CosmicHelper.SheetMissionDict)
                {
                    uint missionId = mission.Key;
                    var info = mission.Value;

                    string classes = info.Jobs != null && info.Jobs.Any()
                        ? string.Join(";", info.Jobs.Select(jobId =>
                            JobAbbreviations.TryGetValue(jobId, out var abbr) ? abbr : jobId.ToString()))
                        : "None";

                    string missionName = EscapeCsvField(RemovePrivateUseChars(info.Name ?? ""));

                    csv.AppendLine($"{missionId},{classes},{missionName},{info.ClassScore}");
                }

                File.WriteAllText(exportPath, csv.ToString(), Encoding.UTF8);
                statusMessage = $"Success: Exported {CosmicHelper.SheetMissionDict.Count} missions to {exportPath}";
            }
            catch (Exception ex)
            {
                statusMessage = $"Error: {ex.Message}";
            }
        }

        private static string EscapeCsvField(string field)
        {
            // If field contains comma, newline, or quote, wrap in quotes and escape internal quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        private static unsafe void InitiateMission(uint missionId)
        {
            var WKSInstance = WKSManager.Instance();
            WKSInstance->MissionModule->InitiateMission((ushort)missionId);
        }

        private static unsafe void AbandonMission()
        {
            var WKSInstance = WKSManager.Instance();
            WKSInstance->MissionModule->AbandonMission();
        }

        private static unsafe void ReportMission()
        {
            var WKSInstance = WKSManager.Instance();
            WKSInstance->MissionModule->ReportMission();
        }
    }
}
