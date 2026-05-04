using Dalamud.Game.Text;
using Dalamud.Interface;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ICE.Ui.MainUi;
using ICE.Ui.MainUi.Settings;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;
using System.Reflection;
using static ICE.Utilities.Cosmic_Helper.CosmicHelper;

namespace ICE.Ui
{
    internal class OverlayWindow : Window
    {
        public OverlayWindow() : base("ICE Overlay")
        {
            Flags = ImGuiWindowFlags.None;
     
            P.windowSystem.AddWindow(this);
            TitleBarButtons.Add(
                new()
                {
                    ShowTooltip = () => ImGui.SetTooltip("Overlay Settings"),
                    Icon = FontAwesomeIcon.Cog,
                    IconOffset = new(1, 1),
                    Click = _ => ImGui.OpenPopup("OverlaySettingsPopup")
                });
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override bool DrawConditions()
        {
            return C.ShowOverlay
                && (PlayerHelper.IsInCosmicZone());
        }

        public override void PreDraw()
        {
            Flags = C.Overlay_AutoResize
                ? ImGuiWindowFlags.AlwaysAutoResize
                : ImGuiWindowFlags.None;

            if (C.Overlay_AutoResize)
            {
                var minWidth = ImGui.CalcTextSize(new string('A', 30)).X + ImGui.GetStyle().WindowPadding.X * 2;
                ImGui.SetNextWindowSizeConstraints(new Vector2(minWidth, 0), new Vector2(float.MaxValue, float.MaxValue));
            }
        }

        public override void Draw()
        {
            if (ImGui.BeginPopup("OverlaySettingsPopup"))
            {
                Misc_Settings.OverlaySettings();
                ImGui.EndPopup();
            }

            SelectableSidebar.AutoSelectClass(C.AutoPickCurrentJob);
            SelectableSidebar.AutoSelectMoonUpdate(C.AutoSelectMoon);

            MissionDetails();

            if (ImGui.BeginTable("Weather/Time Info", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("##Planets");
                ImGui.TableSetupColumn("##Icons");
                ImGui.TableSetupColumn("Current");
                ImGui.TableSetupColumn("Next");

                ImGui.TableHeadersRow();

                foreach (var planet in Planets)
                {
                    if (planet.IsEnabled())
                    {
                        WeatherForcastForTerritory((ushort)planet.TerritoryId, planet.Asset);
                        TimedMissionDetailsForTerritory(planet.TerritoryId, planet.Asset);
                    }
                }

                ImGui.EndTable();
            }

            ClassExpDetails();

            ClassRelicDetails();
        }

        internal static void DrawModeSelectPopup(string popupId)
        {
            if (ImGui.BeginPopup(popupId))
            {
                ImGui.Text("Select Mode");
                ImGui.Separator();

                bool standard = C.SelectedMode == ModeSelect.Standard;
                bool relicMode = C.SelectedMode == ModeSelect.RelicMode;
                bool xpLeveling = C.SelectedMode == ModeSelect.LevelMode;
                bool agendaMode = C.SelectedMode == ModeSelect.AgendaMode;

                if (ImGui.RadioButton("Standard", standard))
                {
                    C.SelectedMode = ModeSelect.Standard;
                    C.Save();
                }
                if (ImGui.RadioButton("Relic Grind", relicMode))
                {
                    C.SelectedMode = ModeSelect.RelicMode;
                    C.Save();
                }
                if (ImGui.RadioButton("Leveling Grind", xpLeveling))
                {
                    C.SelectedMode = ModeSelect.LevelMode;
                    C.Save();
                }
                if (ImGui.RadioButton("Agenda Mode", agendaMode))
                {
                    C.SelectedMode = ModeSelect.AgendaMode;
                    C.Save();
                }

                ImGui.EndPopup();
            }
        }
        private void MissionDetails()
        {
            var settingsIcon = C.Overlay_UseCogsIcon ? FontAwesomeIcon.Cogs : FontAwesomeIcon.Home;
            ImGui.PushFont(UiBuilder.IconFont);
            var iconWidth = ImGui.CalcTextSize(FontAwesomeIcon.Cogs.ToIconString()).X;
            ImGui.PopFont();
            var buttonSize = new Vector2(iconWidth + ImGui.GetStyle().FramePadding.X * 2, 0);
            if (ImGuiEx.IconButton(settingsIcon, "##OpenICE", buttonSize))
            {
                P.mainWindow.IsOpen = true;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Open ICE");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.ListUl, "##OverlayModeSelect", buttonSize))
            {
                ImGui.OpenPopup("Overlay Mode Select");
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Change mode");
                ImGui.EndTooltip();
            }
            DrawModeSelectPopup("Overlay Mode Select");
            if (PlayerHelper.IsInOizys())
            {
                ImGui.SameLine();
                bool droneActive = SchedulerMain.State == IceState.ArtifactSearch;
                if (droneActive)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchLocation, "##DroneFinder", buttonSize))
                {
                    if (droneActive)
                        SchedulerMain.DisablePlugin();
                    else
                        SchedulerMain.State = IceState.ArtifactSearch;
                }
                if (droneActive)
                    ImGui.PopStyleColor();
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(droneActive ? "Stop Drone Finder" : "Run Drone Finder");
                    ImGui.EndTooltip();
                }
            }
            ImGui.SameLine();

            var modeName = C.SelectedMode switch
            {
                ModeSelect.Standard => "Standard",
                ModeSelect.RelicMode => "Relic Grind",
                ModeSelect.LevelMode => "Leveling Grind",
                ModeSelect.AgendaMode => "Cosmic Agenda",
                _ => C.SelectedMode.ToString(),
            };
            ImGui.Text($"{modeName} - {SchedulerMain.State}");

            // Start/Stop toggle
            bool running = SchedulerMain.State != IceState.Idle;
            if (running)
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.2f, 1.0f));
            if (ImGuiEx.IconButton(running ? FontAwesomeIcon.Stop : FontAwesomeIcon.Play, "##StartStop", buttonSize))
            {
                if (running)
                    SchedulerMain.DisablePlugin();
                else
                    SchedulerMain.EnablePlugin();
            }
            if (running)
                ImGui.PopStyleColor();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(running ? "Stop" : "Start");
                ImGui.EndTooltip();
            }

            // Stop after current mission toggle
            ImGui.SameLine();
            bool stopAfter = Mission_Settings.StopAfterCurrent;
            if (stopAfter)
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.5f, 0.0f, 1.0f));
            if (ImGuiEx.IconButton(FontAwesomeIcon.StepForward, "##StopAfterCurrent", buttonSize))
            {
                Mission_Settings.StopAfterCurrent = !Mission_Settings.StopAfterCurrent;
            }
            if (stopAfter)
                ImGui.PopStyleColor();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(Mission_Settings.StopAfterCurrent ? "Stop after current mission: ON" : "Stop after current mission: OFF");
                ImGui.EndTooltip();
            }

            ImGui.SameLine();
            if (CosmicHelper.SheetMissionDict.TryGetValue(CosmicHelper.CurrentLunarMission, out var missionName) && SchedulerMain.State != IceState.AbandonMission)
            {
                var missionText = $"[{CosmicHelper.CurrentLunarMission}] {missionName.Name}";
                if (missionText.Length > 35)
                    missionText = missionText[..32] + "...";
                ImGui.Text(missionText);
            }
            else
            {
                ImGui.Text("No mission");
            }
#if DEBUG
            if (C.ShowDebugGatherInfo)
            {
                ImGui.Text($"Total Node: {Mission_Settings.nodeTotal}");
                ImGui.Text($"Node Counter: {Mission_Settings.nodeCounter}");
            }
#endif
        }
        private static HashSet<uint> GetOverlayJobFilter()
        {
            if (C.Overlay_FilterByCurrentJob)
                return new HashSet<uint> { (uint)Player.Job };
            if (C.Overlay_FilterJobs.Count > 0)
                return C.Overlay_FilterJobs;
            return null;
        }

        private static Dictionary<uint, List<KeyValuePair<uint, CosmicInfo>>> GetExPlusTokenWeatherMissions(uint territoryId)
        {
            var jobFilter = GetOverlayJobFilter();
            var result = new Dictionary<uint, List<KeyValuePair<uint, CosmicInfo>>>();
            foreach (var kvp in SheetMissionDict)
            {
                var mission = kvp.Value;
                if (mission.TerritoryId == territoryId
                    && mission.Rank >= 6
                    && mission.Weather != CosmicWeather.None
                    && mission.RewardItem != 0
                    && (jobFilter == null || mission.Jobs.Any(j => jobFilter.Contains(j)))
                    && CosmicHelper.WeatherIds.TryGetValue(mission.Weather, out var iconId))
                {
                    var key = (uint)iconId;
                    if (!result.ContainsKey(key))
                        result[key] = new List<KeyValuePair<uint, CosmicInfo>>();
                    result[key].Add(kvp);
                }
            }
            return result;
        }
        private void DrawWeatherIcon(WeatherForecast forecast, Dictionary<uint, List<KeyValuePair<uint, CosmicInfo>>> weatherMissions, string extraTooltip = null)
        {
            if (!Svc.Texture.TryGetFromGameIcon(forecast.IconId, out var weatherIcon))
                return;

            var iconSize = new Vector2(23, 23);
            bool highlight = weatherMissions.ContainsKey(forecast.IconId);

            if (highlight)
            {
                var drawList = ImGui.GetWindowDrawList();
                var pos = ImGui.GetCursorScreenPos();
                var center = pos + iconSize * 0.5f;
                drawList.AddCircle(center, 13, ImGui.GetColorU32(new Vector4(1.0f, 0.85f, 0.0f, 1.0f)), 0, 2.0f);
            }

            ImGui.Image(weatherIcon.GetWrapOrEmpty().Handle, iconSize);

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text($"{forecast.Name}");
                if (extraTooltip != null)
                    ImGui.Text(extraTooltip);
                if (highlight)
                {
                    ImGui.Separator();
                    foreach (var mission in weatherMissions[forecast.IconId])
                    {
                        foreach (var jobId in mission.Value.Jobs)
                        {
                            if (CosmicHelper.JobIconDict.TryGetValue(jobId, out var jobIcon))
                            {
                                ImGui.Image(jobIcon.GetWrapOrEmpty().Handle, new Vector2(18, 18));
                                ImGui.SameLine(0, 4);
                            }
                        }
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"[{mission.Key}] {mission.Value.Name} ({mission.Value.RewardItemAmount}x tokens)");
                    }
                }
                if (C.Overlay_WeatherSelected)
                {
                    List<uint> enabledWeathers= new();
                    foreach (var mission in C.MissionConfig)
                    {
                        if (!mission.Value.Enabled)
                            continue;

                        var sheetInfo = CosmicHelper.SheetMissionDict[mission.Key];
                        if (sheetInfo.Weather == CosmicWeather.None)
                            continue;

                        if (CosmicHelper.WeatherIds.TryGetValue(sheetInfo.Weather, out var iconId) && iconId == forecast.IconId)
                            enabledWeathers.Add(mission.Key);
                    }
                    if (enabledWeathers.Count > 0)
                    {
                        ImGui.Separator();
                        if (ImGui.BeginTable($"Enabled Mission Table", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                        {
                            ImGui.TableSetupColumn("Icon");
                            ImGui.TableSetupColumn("Mission");
                            ImGui.TableSetupColumn("Name");
                            foreach (var mission in enabledWeathers)
                            {
                                var sheetInfo = CosmicHelper.SheetMissionDict[mission];
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                foreach (var job in sheetInfo.Jobs)
                                {
                                    var icon = CosmicHelper.JobIconDict[job];
                                    ImGui.Image(icon.GetWrapOrEmpty().Handle, new Vector2(23, 23));
                                    ImGui.SameLine();
                                }

                                ImGui.TableNextColumn();
                                ImGui.Text($"{mission}");

                                ImGui.TableNextColumn();
                                ImGui.Text($"{sheetInfo.Name}");
                            }

                            ImGui.EndTable();
                        }
                    }
                }
                ImGui.EndTooltip();
            }
        }
        private void WeatherForcastForTerritory(ushort territoryId, string moonAsset)
        {
            var weatherForecasts = WeatherForecastHandler.GetTerritoryForecast(territoryId);

            if (weatherForecasts.Count == 0) return;

            var weatherMissions = C.Overlay_HighlightTokenWeather
                ? GetExPlusTokenWeatherMissions(territoryId)
                : new Dictionary<uint, List<KeyValuePair<uint, CosmicInfo>>>();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            DrawMoonAndIcon(moonAsset, FontAwesomeIcon.Cloud);

            // Current weather column
            ImGui.TableNextColumn();
            if (weatherForecasts.Count > 0)
            {
                DrawWeatherIcon(weatherForecasts[0], weatherMissions);
            }

            // Next weather column - show next 4 weathers
            ImGui.TableNextColumn();
            for (int i = 1; i < Math.Min(6, weatherForecasts.Count); i++)
            {
                if (i > 1)
                    ImGui.SameLine(0, 2);

                DrawWeatherIcon(weatherForecasts[i], weatherMissions, $"In: {WeatherForecastHandler.FormatForecastTime(weatherForecasts[i].Time)}");
            }
        }
        private unsafe void TimedMissionDetailsForTerritory(uint territoryId, string moonAsset)
        {
            var eorzeaTime = DateTimeOffset.FromUnixTimeSeconds(Framework.Instance()->ClientTime.EorzeaTime);
            int currentHour = eorzeaTime.Hour;

            var jobFilter = GetOverlayJobFilter();

            var currentHourMissions = SheetMissionDict
                .Where(kvp => IsAvailableAtHour(kvp.Value, currentHour))
                .Where(kvp => kvp.Value.TerritoryId == territoryId)
                .Where(kvp => jobFilter == null || kvp.Value.Jobs.Any(j => jobFilter.Contains(j)))
                .OrderBy(kvp => kvp.Value.Jobs.FirstOrDefault())
                .ToList();

            // Get the set of mission keys currently shown (for deduplication check)
            var currentMissionKeys = currentHourMissions.Select(kvp => kvp.Key).ToHashSet();

            // Search forward for the next "interesting" hour
            List<KeyValuePair<uint, CosmicHelper.CosmicInfo>> nextHourMissions = new();
            int nextHour = -1;

            for (int offset = 1; offset <= 24; offset++)
            {
                int candidateHour = (currentHour + offset) % 24;

                var candidates = SheetMissionDict
                    .Where(kvp => IsAvailableAtHour(kvp.Value, candidateHour))
                    .Where(kvp => kvp.Value.TerritoryId == territoryId)
                    .Where(kvp => jobFilter == null || kvp.Value.Jobs.Any(j => jobFilter.Contains(j)))
                    .OrderBy(kvp => kvp.Value.Jobs.FirstOrDefault())
                    .ToList();

                if (candidates.Count == 0)
                    continue;

                // Esentially trying to find the first point where the current != the next as MUCH as we can. 
                // Trying to give more visible time coming for things
                bool isDifferent = candidates.Any(kvp => !currentMissionKeys.Contains(kvp.Key));

                if (isDifferent)
                {
                    nextHourMissions = candidates;
                    nextHour = candidateHour;
                    break;
                }
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            DrawMoonAndIcon(moonAsset, FontAwesomeIcon.Clock);

            ImGui.TableNextColumn();
            for (int i = 0; i < currentHourMissions.Count; i++)
            {
                if (i > 0) ImGui.SameLine(0, 2);

                var mission = currentHourMissions[i];
                if (CosmicHelper.JobIconDict.TryGetValue(mission.Value.Jobs[0], out var jobIcon))
                {
                    var imageSize = new Vector2(23, 23);
                    ImGui.Image(jobIcon.GetWrapOrEmpty().Handle, imageSize);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"[{mission.Key}]");
                        ImGui.SameLine(0, 2);
                        ImGui.Text($"{mission.Value.Name}");
                        var expires = EorzeaHoursUntil(eorzeaTime, (int)mission.Value.EndTime);
                        ImGui.Text($"Expires in {FormatRealTime(expires)}");
                        ImGui.EndTooltip();
                    }
                }
            }

            ImGui.TableNextColumn();
            for (int i = 0; i < nextHourMissions.Count; i++)
            {
                if (i > 0) ImGui.SameLine(0, 2);

                var mission = nextHourMissions[i];
                if (CosmicHelper.JobIconDict.TryGetValue(mission.Value.Jobs[0], out var jobIcon))
                {
                    var imageSize = new Vector2(23, 23);
                    ImGui.Image(jobIcon.GetWrapOrEmpty().Handle, imageSize);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"[{mission.Key}]");
                        ImGui.SameLine(0, 2);
                        ImGui.Text($"{mission.Value.Name}");
                        var startsIn = EorzeaHoursUntil(eorzeaTime, (int)mission.Value.StartTime);
                        // Show which hour slot this is if it's not the immediate next hour
                        int hoursAhead = ((nextHour - currentHour) + 24) % 24;
                        string timeLabel = hoursAhead == 1 ? "Starts in" : $"Starts in ~{hoursAhead}h |";
                        ImGui.Text($"{timeLabel} {FormatRealTime(startsIn)}");
                        ImGui.EndTooltip();
                    }
                }
            }
        }
        private static readonly (uint TerritoryId, string Asset, string Name, Func<bool> IsEnabled)[] Planets = new[]
        {
            ((uint)1237, "ICE.Resources.Sinus_Ardorum.png", "Sinus Ardorum", new Func<bool>(() => C.ShowSinusMissions)),
            ((uint)1291, "ICE.Resources.Phaenna.png", "Phaenna", new Func<bool>(() => C.ShowPhaennaMissions)),
            ((uint)1310, "ICE.Resources.Oizys.png", "Oizys", new Func<bool>(() => C.ShowOizysMissions)),
        };
        private void DrawMoonAndIcon(string moonAsset, FontAwesomeIcon icon)
        {
            var moonTexture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), moonAsset).GetWrapOrEmpty();
            ImGui.Image(moonTexture.Handle, new Vector2(23, 23));
            var moonName = Planets.FirstOrDefault(p => p.Asset == moonAsset).Name;
            if (ImGui.IsItemHovered() && moonName != null)
            {
                ImGui.BeginTooltip();
                ImGui.Text(moonName);
                ImGui.EndTooltip();
            }
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.Text(icon.ToIconString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(icon == FontAwesomeIcon.Cloud ? "Weather" : "Timed");
                ImGui.EndTooltip();
            }
        }
        private static double EorzeaHoursUntil(DateTimeOffset eorzeaTime, int targetHour)
        {
            double currentFractional = eorzeaTime.Hour + eorzeaTime.Minute / 60.0 + eorzeaTime.Second / 3600.0;
            double hoursUntil = targetHour - currentFractional;
            if (hoursUntil <= 0)
                hoursUntil += 24;
            return hoursUntil;
        }
        private static string FormatRealTime(double eorzeaHours)
        {
            // 1 Eorzea hour = 175 real seconds
            int totalSeconds = (int)(eorzeaHours * 175);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return minutes > 0 ? $"{minutes}m {seconds:D2}s" : $"{seconds}s";
        }
        private bool IsAvailableAtHour(CosmicInfo mission, int hour)
        {
            // If StartTime <= EndTime: normal range (e.g., 8-16)
            if (mission.StartTime <= mission.EndTime)
            {
                return hour >= mission.StartTime && hour < mission.EndTime;
            }
            // If StartTime > EndTime: crosses midnight (e.g., 20-4)
            else
            {
                return hour >= mission.StartTime || hour < mission.EndTime;
            }
        }
        private void ClassExpDetails()
        {
            var ScoreInfo = CosmicHelper.Cosmic_ClassInfo();

            if (C.ShowCurrentScore)
            {
                if (CosmicHelper.SheetMissionDict.TryGetValue(CosmicHelper.CurrentLunarMission, out var sheetInfo))
                {
                    foreach (var job in sheetInfo.Jobs)
                    {
                        if (ScoreInfo.TryGetValue(job, out var classInfo))
                        {
                            if (CosmicHelper.JobIconDict.TryGetValue(job, out var icon))
                            {
                                ImGui.Image(icon.GetWrapOrEmpty().Handle, new(25, 25));
                                ImGui.SameLine();
                                ImGui.AlignTextToFramePadding();
                            }

                            ImGui_Ice.Draw_XPBar(classInfo.Score, 0, 500_000, $"{classInfo.Score:N0} / {500_000:N0}");
                        }
                    }
                }
                else
                {
                    var job = (uint)Player.Job;
                    if (ScoreInfo.TryGetValue(job, out var classInfo))
                    {
                        if (CosmicHelper.JobIconDict.TryGetValue(job, out var icon))
                        {
                            ImGui.Image(icon.GetWrapOrEmpty().Handle, new(25, 25));
                            ImGui.SameLine();
                            ImGui.AlignTextToFramePadding();
                        }

                        ImGui_Ice.Draw_XPBar(classInfo.Score, 0, 500_000, $"{classInfo.Score:N0} / {500_000:N0}");
                    }
                }
            }

            if (C.ShowTotalScore)
            {
                int maxScore = 5_500_000;
                int currentTotal = 0;
                int actualTotal = 0;
                int totalCompleted = 0;

                foreach (var job in ScoreInfo)
                {
                    currentTotal += Math.Min(job.Value.Score, 500_000);
                    if (job.Value.Score >= 500_000)
                        totalCompleted += 1;
                    actualTotal += job.Value.Score;
                }

                if (totalCompleted != 11)
                {
                    ImGui_Ice.Draw_XPBar(currentTotal, 0, maxScore, label: $"Total: {currentTotal:N0} / {maxScore:N0} [{totalCompleted} / 11]");
                }
                else
                {
                    ImGui_Ice.Draw_XPBar(actualTotal, 0, maxScore, label: $"Total Score: {actualTotal:N0}");
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();

                    foreach (var job in ScoreInfo)
                    {
                        var jobIdInfo = job.Key;
                        var jobScore = job.Value.Score;
                        var jobImage = CosmicHelper.JobIconDict[jobIdInfo];
                        ImGui.Image(jobImage.GetWrapOrEmpty().Handle, new Vector2(23, 23));
                        ImGui.SameLine();
                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"Score: {jobScore:N0}");
                    }

                    ImGui.EndTooltip();
                }
            }
        }
        private void ClassRelicDetails()
        {
            if (!C.ShowExpBars) return;

            if (C.ShowExpBars_HideWhenMaxed)
            {
                var expInfo = CosmicHelper.Cosmic_ClassInfo();
                var currentJobId = (uint)Player.Job;
                if (expInfo.TryGetValue(currentJobId, out var jobInfo)
                    && jobInfo.CurrentExp.Count > 0
                    && jobInfo.CurrentExp.Values.All(exp => exp.Current >= exp.Needed))
                {
                    return;
                }
            }

            {
                var currentJobId = (uint)Player.Job;
                var flags = C.Overlay_RelicXpExpanded ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
                var open = ImGui.CollapsingHeader("Relic Tool XP", flags);
                if (open != C.Overlay_RelicXpExpanded)
                {
                    C.Overlay_RelicXpExpanded = open;
                    C.Save();
                }
                if (open)
                {
                    ImGui_Ice.Draw_ExpTable(currentJobId);
                }
            }
        }


    }
}