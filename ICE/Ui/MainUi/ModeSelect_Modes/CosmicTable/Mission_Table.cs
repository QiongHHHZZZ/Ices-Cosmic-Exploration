using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ICE.OldYamlConfig;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Reflection;
using static ICE.ConfigFiles.Config;
using static ICE.Localization.L10n;
using static ICE.Utilities.Cosmic_Helper.CosmicHelper;

namespace ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable
{
    internal class VerticalCenterColumnString : ColumnString<MissionInfo>
    {
        public override bool DrawFilter()
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(Label);
            return false;
        }

        public override void PreDraw()
        {
            var pos = ImGui.GetCursorPosY();
            var offset = (ImageSize - ImGui.GetTextLineHeight()) / 2;
            ImGui.SetCursorPosY(pos + offset);
        }
    }
    internal class ItemFilterColumn : ColumnFlags<ItemFilter, MissionInfo>
    {
        private ItemFilter[] FlagValues = Array.Empty<ItemFilter>();
        private string[] FlagNames = Array.Empty<string>();

        private string ReturnFlagNames(ItemFilter item)
        {
            return item switch
            {
                ItemFilter.NoItems => T("No Items"),
                ItemFilter.Enabled => T("Enabled"),
                ItemFilter.Disabled => T("Disabled"),
                // ItemFilter.NotCompleted => "Not Completed",
                // ItemFilter.Completed => "Completed",
                // ItemFilter.Gold => "Gold",
                _ => T("Unknown"),
            };
        }

        protected void SetFlags(params ItemFilter[] flags)
        {
            FlagValues = flags;
            AllFlags = FlagValues.Aggregate((f, g) => f | g);
        }

        protected void SetFlagsAndNames(params ItemFilter[] flags)
        {
            SetFlags(flags);
            SetNames(flags.Select(f => ReturnFlagNames(f)).ToArray());
        }

        protected void SetNames(params string[] names) => FlagNames = names;

        protected sealed override IReadOnlyList<ItemFilter> Values => FlagValues;

        protected sealed override string[] Names => FlagNames;

        public sealed override bool DrawFilter()
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(Label);
            return false;
        }

        public sealed override ItemFilter FilterValue => C.ItemFilter;

        protected sealed override void SetValue(ItemFilter f, bool v)
        {
            var tmp = v ? FilterValue | f : FilterValue & ~f;
            if (tmp == FilterValue)
                return;

            C.ItemFilter = tmp;
            C.SaveDebounced();
        }
    }
    internal class MissionFilterColumn : ColumnFlags<MissionFilter, MissionInfo>
    {
        private MissionFilter[] FlagValues = Array.Empty<MissionFilter>();
        private string[] FlagNames = Array.Empty<string>();

        protected void SetFlags(params MissionFilter[] flags)
        {
            FlagValues = flags;
            AllFlags = FlagValues.Aggregate((f, g) => f | g);
        }

        protected void SetFlagsAndNames(params MissionFilter[] flags)
        {
            SetFlags(flags);
            SetNames(flags.Select(f => f.ToString()).ToArray());
        }

        protected void SetNames(params string[] names) => FlagNames = names;

        protected sealed override IReadOnlyList<MissionFilter> Values => FlagValues;

        protected sealed override string[] Names => FlagNames;

        public sealed override bool DrawFilter()
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(Label);
            return false;
        }

        public sealed override MissionFilter FilterValue => C.MissionFilter;

        protected sealed override void SetValue(MissionFilter f, bool v)
        {
            var tmp = v ? FilterValue | f : FilterValue & ~f;
            if (tmp == FilterValue)
                return;

            C.MissionFilter = tmp;
            C.SaveDebounced();
        }
    }
    internal class JobFilterColumn : ColumnFlags<JobFilter, MissionInfo>
    {
        private JobFilter[] FlagValues = Array.Empty<JobFilter>();
        private string[] FlagNames = Array.Empty<string>();

        protected void SetFlags(params JobFilter[] flags)
        {
            FlagValues = flags;
            AllFlags = FlagValues.Aggregate((f, g) => f | g);
        }

        protected void SetFlagsAndNames(params JobFilter[] flags)
        {
            SetFlags(flags);
            SetNames(flags.Select(f => f.ToString()).ToArray());
        }

        protected void SetNames(params string[] names) => FlagNames = names;

        protected sealed override IReadOnlyList<JobFilter> Values => FlagValues;

        protected sealed override string[] Names => FlagNames;

        public sealed override bool DrawFilter()
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(Label);
            return false;
        }

        public sealed override JobFilter FilterValue => C.JobFilter;

        protected sealed override void SetValue(JobFilter f, bool v)
        {
            var tmp = v ? FilterValue | f : FilterValue & ~f;
            if (tmp == FilterValue)
                return;

            C.JobFilter = tmp;
            C.SaveDebounced();
        }
    }

    internal class Mission_Table : Table<MissionInfo>, IDisposable
    {
        // TODO: Create default width's for all of these...

        public enum TableViewMode
        {
            Compact,
            Full,
            Custom,
        }

        public TableViewMode ViewMode { get; set; } = TableViewMode.Full;
        public bool[] CustomColumnVisibility { get; private set; } = [];
        private readonly HashSet<uint> _availableMissionIds = [];
        private long _nextAvailableMissionRefresh;
        private static readonly Dictionary<uint, double> ScorePerMinuteCache = [];

        public readonly EnabledColumn _enabledColumn;
        public readonly NameColumn _nameColumn = new() { Label = T("Mission Table Name") };
        public readonly IdColumn _idColumn = new() { Label = T("Mission Table ID") };
        public readonly JobColumn _jobColumn = new() { Label = T("Mission Table Job") };
        public readonly MissionColumn _missionColumn = new() { Label = T("Rank") };
        public readonly CompletionColumn _completionColumn = new() { Label = T("Completed") };
        public readonly ClassScoreColumn _classScoreColumn = new() { Label = "技巧点\n任务" };
        public readonly CosmocreditColumn _cosmoColumn = new() { Label = "信用点\n宇宙" };
        public readonly LunarCreditColumn _lunarColumn = new() { Label = "信用点\n星球" };
        public readonly ScoreSummaryColumn _scoreSummaryColumn = new() { Label = "技巧点\n任务|每分钟" };
        public readonly CreditSummaryColumn _creditSummaryColumn = new() { Label = "信用点\n宇宙|星球" };
        public readonly DroneCreditColumn _droneColumn = new() { Label = T("Dronebits") };
        public readonly PlanetTokensColumn _planetTokenColumn = new() { Label = T("Mission Table Planet Tokens") };
        public readonly SPMColumn _spmColumn = new() { Label = "技巧点\n每分钟" };
        public readonly TurninColumn _turninColumn = new() { Label = T("Mission Table Turnin Goal") };
        public readonly PlanetColumn _planetColumn = new() { Label = T("Mission Table Moons") };
        public readonly ProfileColumn _profileColumn = new() { Label = T("Mission Table Profile") };
        public readonly AllRelicExpColum _allExpColumn = new() { Label = T("Mission Table Exp") };

        public Mission_Table(List<MissionInfo> itemList) : base("Item_Table_V2", itemList)
        {
            _enabledColumn = new EnabledColumn(this) { Label = T("Mission Table Enabled") };

            List<Column<MissionInfo>> headers = [
                _enabledColumn, _completionColumn, _idColumn, _planetColumn,
                _jobColumn, _missionColumn, _nameColumn, _classScoreColumn, _spmColumn,
                _cosmoColumn, _lunarColumn, _droneColumn, _planetTokenColumn,
                _turninColumn, _allExpColumn];


            /*
            var tierFlags = new (int tier, ItemFilter flag)[]
            {
                (1, ItemFilter.HasI),   (2, ItemFilter.HasII),  (3, ItemFilter.HasIII),
                (4, ItemFilter.HasIV),  (5, ItemFilter.HasV),   (6, ItemFilter.HasVI),
                (7, ItemFilter.HasVII)
            };

            foreach (var (tier, flag) in tierFlags)
            {
                string tierName = tier switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V", 6 => "VI", 7 => "VII", _ => "?" };
                headers.Add(new RelicExpColumn(tier, flag) { Label = $"Exp {tierName}" });
            }
            */

            headers.Add(_profileColumn);
            Headers = [.. headers];
            CustomColumnVisibility = Enumerable.Repeat(true, Headers.Length).ToArray();

            Sortable = true;
            Flags |= ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoSavedSettings;
        }

        protected override void PreDraw()
        {
            ApplyViewModeToColumnFlags();
            RefreshAvailableMissionCache();
        }

        protected override int GetStretchColumnIndex(IReadOnlyList<int> visibleColumns)
        {
            for (var i = 0; i < visibleColumns.Count; i++)
            {
                if (Headers[visibleColumns[i]] == _nameColumn)
                    return i;
            }

            return -1;
        }

        protected override float GetMinimumStretchColumnWidth(Column<MissionInfo> header, int index)
            => 90f * ImGuiHelpers.GlobalScale;

        protected override float GetMinimumColumnWidth(Column<MissionInfo> header, int index)
        {
            var scale = ImGuiHelpers.GlobalScale;
            var width = header switch
            {
                EnabledColumn => 44f,
                CompletionColumn => 44f,
                IdColumn => 46f,
                PlanetColumn => 44f,
                JobColumn => 44f,
                MissionColumn => 48f,
                NameColumn => 90f,
                ClassScoreColumn => 44f,
                SPMColumn => 56f,
                ScoreSummaryColumn => 86f,
                CosmocreditColumn => 46f,
                LunarCreditColumn => 46f,
                CreditSummaryColumn => 84f,
                DroneCreditColumn => 48f,
                PlanetTokensColumn => 48f,
                TurninColumn => 106f,
                AllRelicExpColum => 168f,
                ProfileColumn => 104f,
                _ => 44f,
            };

            return width * scale;
        }

        private void RefreshAvailableMissionCache()
        {
            var now = Environment.TickCount64;
            if (now < _nextAvailableMissionRefresh)
                return;

            _nextAvailableMissionRefresh = now + 250;
            _availableMissionIds.Clear();
            foreach (var missionId in CosmicHandler.All_AvailableMissions())
                _availableMissionIds.Add(missionId);
        }

        private void ApplyViewModeToColumnFlags()
        {
            EnsureCustomColumnVisibility();

            for (int i = 0; i < Headers.Length; i++)
            {
                var header = Headers[i];
                bool visible = IsColumnVisible(header, i);
                header.IsVisible = visible;
                header.Flags = visible
                    ? header.Flags & ~ImGuiTableColumnFlags.DefaultHide & ~ImGuiTableColumnFlags.Disabled
                    : header.Flags | ImGuiTableColumnFlags.DefaultHide | ImGuiTableColumnFlags.Disabled;
            }
        }

        private bool IsColumnVisible(Column<MissionInfo> header, int index)
        {
            if (header == _enabledColumn && !CanToggleMissionEnabled())
                return false;

            if (header.Flags.HasFlag(ImGuiTableColumnFlags.NoHide))
                return true;

            EnsureCustomColumnVisibility();
            return ViewMode switch
            {
                TableViewMode.Compact => !IsCompactHiddenColumn(header),
                TableViewMode.Custom => CustomColumnVisibility[index],
                _ => true,
            };
        }

        public void DrawCustomColumnSelector()
        {
            EnsureCustomColumnVisibility();

            for (int i = 0; i < Headers.Length; i++)
            {
                var header = Headers[i];
                if (header == _enabledColumn && !CanToggleMissionEnabled())
                    continue;

                bool visible = CustomColumnVisibility[i];
                bool locked = header.Flags.HasFlag(ImGuiTableColumnFlags.NoHide);

                ImGui.PushID(i);

                if (locked)
                    ImGui.BeginDisabled();

                if (ImGui.Checkbox($"{header.Label}##customColumn", ref visible) && !locked)
                    CustomColumnVisibility[i] = visible;

                if (locked)
                    ImGui.EndDisabled();

                ImGui.PopID();
            }
        }

        private static bool CanToggleMissionEnabled()
            => C.SelectedMode != ModeSelect.MissionGoldMode
            && C.SelectedMode != ModeSelect.LevelMode
            && (C.SelectedMode != ModeSelect.RelicMode || C.XPRelicOnlyEnabled);

        private void EnsureCustomColumnVisibility()
        {
            if (CustomColumnVisibility.Length == Headers.Length)
                return;

            CustomColumnVisibility = Enumerable.Repeat(true, Headers.Length).ToArray();
        }

        private bool IsCompactHiddenColumn(Column<MissionInfo> header)
        {
            return header == _classScoreColumn
                || header == _spmColumn
                || header == _scoreSummaryColumn
                || header == _cosmoColumn
                || header == _lunarColumn
                || header == _creditSummaryColumn
                || header == _droneColumn
                || header == _planetTokenColumn
                || header == _allExpColumn
                || header is RelicExpColumn;
        }

        protected override float GetColumnWidth(Column<MissionInfo> header, int index)
        {
            var scale = ImGuiHelpers.GlobalScale;
            var width = header switch
            {
                EnabledColumn => 48f,
                CompletionColumn => 44f,
                IdColumn => 52f,
                PlanetColumn => 44f,
                JobColumn => 44f,
                MissionColumn => 48f,
                NameColumn => 420f,
                ClassScoreColumn => 44f,
                ScoreSummaryColumn => 98f,
                CosmocreditColumn => 46f,
                LunarCreditColumn => 46f,
                CreditSummaryColumn => 92f,
                DroneCreditColumn => 48f,
                PlanetTokensColumn => 48f,
                SPMColumn => 56f,
                TurninColumn => 106f,
                AllRelicExpColum => 184f,
                RelicExpColumn => 78f,
                ProfileColumn => 104f,
                _ => 76f,
            };

            return width * scale;
        }

        protected override uint GetRowBackgroundColor(MissionInfo item, int itemIndex, int rowNumber, bool hovered)
        {
            var activeMission = CosmicHelper.CurrentLunarMission != 0 && CosmicHelper.CurrentLunarMission == item.Id;
            var availableMission = _availableMissionIds.Contains(item.Id);
            if (activeMission || availableMission)
            {
                return hovered
                    ? ImGui.GetColorU32(new Vector4(0.12f, 0.45f, 0.25f, 0.72f))
                    : ImGui.GetColorU32(new Vector4(0.03f, 0.34f, 0.13f, 0.38f));
            }

            return base.GetRowBackgroundColor(item, itemIndex, rowNumber, hovered);
        }

        private static double GetScorePerMinute(MissionInfo item)
        {
            if (ScorePerMinuteCache.TryGetValue(item.Id, out var cachedScore))
                return cachedScore;

            var scoreInfo = item.SheetInfo.ScoreInfo();
            if (item.SheetInfo.IsCritical && scoreInfo.TryGetValue(TurninState.Critical, out var criticalScore))
                return ScorePerMinuteCache[item.Id] = criticalScore.Score;
            if (scoreInfo.TryGetValue(TurninState.SequenceGold, out var seqGold) && seqGold.Score != 0)
                return ScorePerMinuteCache[item.Id] = seqGold.Score;
            return ScorePerMinuteCache[item.Id] = scoreInfo.Values.MaxBy(r => r.Score)?.Score ?? 0;
        }

        private static void DrawSplitValues(string left, string right)
        {
            var start = ImGui.GetCursorScreenPos();
            var width = ImGuiUtil.CurrentColumnWidth;
            var half = MathF.Max(1f, width * 0.5f);
            var lineColor = ImGui.GetColorU32(new Vector4(0.24f, 0.31f, 0.43f, 0.45f));

            ImGui.GetWindowDrawList().AddLine(
                new Vector2(start.X + half, start.Y - 2f * ImGuiHelpers.GlobalScale),
                new Vector2(start.X + half, start.Y + ImGui.GetFrameHeight()),
                lineColor);

            using (ImGuiUtil.PushColumnWidth(half))
                ImGuiUtil.Center(left);

            ImGui.SetCursorScreenPos(new Vector2(start.X + half, start.Y));
            using (ImGuiUtil.PushColumnWidth(half))
                ImGuiUtil.Center(right);
        }

        public void Dispose()
        {

        }

        public sealed class EnabledColumn : ItemFilterColumn
        {
            private readonly Mission_Table _table;
            public EnabledColumn(Mission_Table table)
            {
                _table = table;
                Flags = ImGuiTableColumnFlags.NoHide;
                SetFlags(ItemFilter.Enabled, ItemFilter.Disabled);
                SetNames(T("Enabled"), T("Disabled"));
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
                => lhs.Enabled.CompareTo(rhs.Enabled);

            public override void DrawColumn(MissionInfo item, int _)
            {
                ImGui.PushID(item.Id);

                bool disabled = C.SelectedMode == ModeSelect.MissionGoldMode
                             || C.SelectedMode == ModeSelect.LevelMode
                             || (C.SelectedMode == ModeSelect.RelicMode && !C.XPRelicOnlyEnabled);

                if (!disabled)
                {
                    bool enabled = C.MissionConfig[item.Id].Enabled;
                    var cursorPos = ImGui.GetCursorPos();
                    var checkboxSize = ImGui.GetFrameHeight();
                    ImGui.SetCursorPosX(cursorPos.X + MathF.Max(0, (ImGuiUtil.CurrentColumnWidth - checkboxSize) * 0.5f));
                    ImGui.AlignTextToFramePadding();

                    if (ImGui.Checkbox("##EnableMission", ref enabled))
                    {
                        C.MissionConfig[item.Id].Enabled = enabled;
                        if (enabled == true)
                        {
                            foreach (var prevMission in CosmicHelper.SheetMissionDict[item.Id].SequenceMissions_Previous)
                            {
                                C.MissionConfig[prevMission].Enabled = true;
                            }
                        }

                        C.SaveDebounced();
                        _table.SetFilterDirty(false);
                    }
                    if (ImGui.IsItemClicked())
                    {
                        Window_ExternalDetails.SelectedMission = item.Id;
                    }
                }
                ImGui.PopID();
            }

            public override bool FilterFunc(MissionInfo item)
            {
                return item.Enabled ? FilterValue.HasFlag(ItemFilter.Enabled) : FilterValue.HasFlag(ItemFilter.Disabled);
            }
        }
        public sealed class NameColumn : VerticalCenterColumnString
        {
            public NameColumn() => Flags |= ImGuiTableColumnFlags.NoHide;
            public override string ToName(MissionInfo mission) => mission.SheetInfo.Name;
            public override void DrawColumn(MissionInfo mission, int _)
            {
                var scale = ImGuiHelpers.GlobalScale;
                var cellStart = ImGui.GetCursorScreenPos();
                var iconGap = 4f * scale;
                var iconCount = 0;
                if (mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Gather) || mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Fish))
                    iconCount++;
                if (CosmicHelper.CriticalLocations.ContainsKey(mission.Id))
                    iconCount++;
                iconCount += CountNoteIcons(mission);

                var reservedIconWidth = iconCount > 0
                    ? iconCount * ImGui.GetFrameHeight() + MathF.Max(0, iconCount - 1) * ImGui.GetStyle().ItemSpacing.X + iconGap
                    : 0f;
                var buttonWidth = MathF.Max(110f * scale, ImGuiUtil.CurrentColumnWidth - reservedIconWidth - iconGap);

                if (ImGui.Button($"{mission.SheetInfo.Name}##MissionName", new Vector2(buttonWidth, 0)))
                {
                    IceLogging.Verbose("Testing... if this fires off multiple times", "DEBUG TEST");
                    Window_ExternalDetails.SelectedMission = mission.Id;
                    P.externalDetails.IsOpen = true;
                    IceLogging.Verbose($"Collasped condition: {P.externalDetails.CollapsedCondition.ToString()}");
                }
                if (ImGui.IsItemHovered() && ImGui.CalcTextSize(mission.SheetInfo.Name).X > buttonWidth - ImGui.GetStyle().FramePadding.X * 2)
                    ImGui.SetTooltip(mission.SheetInfo.Name);

                var iconX = cellStart.X + buttonWidth + iconGap;
                if (iconCount > 0)
                    ImGui.SetCursorScreenPos(new Vector2(iconX, cellStart.Y));
                var drewIcon = false;

                if (mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Gather) || mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Fish))
                {
                    if (drewIcon)
                        ImGui.SameLine();

                    if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"Flag_{mission.Id}"))
                    {
                        Window_ExternalDetails.SelectedMission = mission.Id;
                        Utils.SetGatheringRing(mission.SheetInfo.TerritoryId, (int)mission.SheetInfo.MapPosition.X, (int)mission.SheetInfo.MapPosition.Y, mission.SheetInfo.Radius, mission.SheetInfo.Name);
                    }
                    drewIcon = true;
                }
                if (CosmicHelper.CriticalLocations.TryGetValue(mission.Id, out var criticalLoc))
                {
                    if (drewIcon)
                        ImGui.SameLine();

                    if (ImGuiEx.IconButton(FontAwesomeIcon.FlagCheckered, $"CriticalFlag_{mission.Id}"))
                    {
                        Utils.SetFlagForNPC(mission.SheetInfo.TerritoryId, criticalLoc.MapInfo.X, criticalLoc.MapInfo.Y);
                    }
                    drewIcon = true;
                }

                DrawNoteIcons(mission, drewIcon);
            }
        }
        public sealed class IdColumn : VerticalCenterColumnString
        {
            public override string ToName(MissionInfo item) => item.Id.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.Id.CompareTo(rhs.Id);

            public override void DrawColumn(MissionInfo item, int _)
            {
                ImGuiUtil.Center($"{item.Id}");
            }
        }
        public sealed class CompletionColumn : ItemFilterColumn
        {
            public CompletionColumn()
            {
                Flags = ImGuiTableColumnFlags.None;
                SetFlags(ItemFilter.NotCompleted, ItemFilter.Completed, ItemFilter.Gold);
                SetNames(T("Not Completed"), T("Completed"), T("Gold"));
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.CompletionStatus.CompareTo(rhs.SheetInfo.CompletionStatus);
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var iconSize = frameHeight - 2f * ImGuiHelpers.GlobalScale;
                var size = new Vector2(iconSize);

                var columnWidth = ImGuiUtil.CurrentColumnWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0, (columnWidth - iconSize) * 0.5f));

                if (status is Status.Gold)
                {
                    if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex && tex.TryGetWrap(out var wrap, out _))
                    {
                        ImGui.Image(wrap.Handle, size, new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
                    }
                }
                else
                {
                    var icon = status is Status.None ? FontAwesomeIcon.Times : FontAwesomeIcon.Check;
                    var color = status is Status.None ? EColor.Red : EColor.Green;

                    using (ImRaii.PushFont(UiBuilder.IconFont))
                    using (ImRaii.PushColor(ImGuiCol.Text, color))
                    {
                        var iconText = icon.ToIconString();
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0, (columnWidth - ImGui.CalcTextSize(iconText).X) * 0.5f));
                        ImGuiEx.Icon(icon);
                    }
                }
            }
            public override bool FilterFunc(MissionInfo item)
            {
                var status = item.SheetInfo.CompletionStatus;

                if (FilterValue.HasFlag(ItemFilter.NotCompleted) && status == Status.None) return true;
                if (FilterValue.HasFlag(ItemFilter.Completed) && status == Status.Completed) return true;
                if (FilterValue.HasFlag(ItemFilter.Gold) && status == Status.Gold) return true;

                return false;
            }
        }
        public sealed class ClassScoreColumn : VerticalCenterColumnString
        {
            public override string ToName(MissionInfo mission) => mission.SheetInfo.ClassScore.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.ClassScore.CompareTo(rhs.SheetInfo.ClassScore);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.ClassScore}");
            }
        }
        public sealed class CosmocreditColumn : VerticalCenterColumnString
        {
            public override string ToName(MissionInfo mission) => mission.SheetInfo.CosmoCredit.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.CosmoCredit.CompareTo(rhs.SheetInfo.CosmoCredit);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.CosmoCredit}");
            }
        }
        public sealed class LunarCreditColumn : VerticalCenterColumnString
        {
            public override string ToName(MissionInfo mission) => mission.SheetInfo.LunarCredit.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.LunarCredit.CompareTo(rhs.SheetInfo.LunarCredit);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.LunarCredit}");
            }
        }
        public sealed class DroneCreditColumn : VerticalCenterColumnString
        {
            public override string ToName(MissionInfo mission) => mission.SheetInfo.DronebitReward.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.DronebitReward.CompareTo(rhs.SheetInfo.DronebitReward);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.DronebitReward}");
            }
        }
        public sealed class ScoreSummaryColumn : VerticalCenterColumnString
        {
            public override string ToName(MissionInfo mission) => $"{mission.SheetInfo.ClassScore} {GetScorePerMinute(mission):N2}";
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.ClassScore.CompareTo(rhs.SheetInfo.ClassScore);
            public override int SortKeyCount => 2;
            public override int CompareSortKey(MissionInfo lhs, MissionInfo rhs, int sortKey)
                => sortKey == 1
                    ? GetScorePerMinute(lhs).CompareTo(GetScorePerMinute(rhs))
                    : lhs.SheetInfo.ClassScore.CompareTo(rhs.SheetInfo.ClassScore);

            public override void DrawColumn(MissionInfo mission, int _)
            {
                DrawSplitValues($"{mission.SheetInfo.ClassScore}", GetScorePerMinute(mission) > 0 ? $"{GetScorePerMinute(mission):N1}" : "-");
            }
        }
        public sealed class CreditSummaryColumn : VerticalCenterColumnString
        {
            public override string ToName(MissionInfo mission) => $"{mission.SheetInfo.CosmoCredit} {mission.SheetInfo.LunarCredit}";
            public override int SortKeyCount => 2;
            public override int Compare(MissionInfo lhs, MissionInfo rhs)
            {
                var creditCompare = lhs.SheetInfo.CosmoCredit.CompareTo(rhs.SheetInfo.CosmoCredit);
                return creditCompare != 0 ? creditCompare : lhs.SheetInfo.LunarCredit.CompareTo(rhs.SheetInfo.LunarCredit);
            }
            public override int CompareSortKey(MissionInfo lhs, MissionInfo rhs, int sortKey)
                => sortKey == 1
                    ? lhs.SheetInfo.LunarCredit.CompareTo(rhs.SheetInfo.LunarCredit)
                    : lhs.SheetInfo.CosmoCredit.CompareTo(rhs.SheetInfo.CosmoCredit);

            public override void DrawColumn(MissionInfo mission, int _)
                => DrawSplitValues($"{mission.SheetInfo.CosmoCredit}", $"{mission.SheetInfo.LunarCredit}");
        }
        public sealed class PlanetTokensColumn : ItemFilterColumn
        {
            public PlanetTokensColumn()
            {
                Flags = ImGuiTableColumnFlags.None;
                SetFlags(ItemFilter.HasTokens, ItemFilter.NoTokens);
                SetNames(T("Has Tokens"), T("No Tokens"));
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.TokenItemAmount.CompareTo(rhs.SheetInfo.TokenItemAmount);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.TokenItemAmount}");
            }

            public override bool FilterFunc(MissionInfo mission)
            {
                return mission.SheetInfo.TokenItemAmount > 0 ? FilterValue.HasFlag(ItemFilter.HasTokens) : FilterValue.HasFlag(ItemFilter.NoTokens);
            }
        }
        public sealed class AllRelicExpColum : ItemFilterColumn
        {
            public AllRelicExpColum()
            {
                SetFlags(ItemFilter.HasI, ItemFilter.HasII, ItemFilter.HasIII, ItemFilter.HasIV, ItemFilter.HasV, ItemFilter.HasVI, ItemFilter.HasVII);
                SetNames("I", "II", "III", "IV", "V", "VI", "VII");
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
            {
                var lhsPairs = FirstRelicExp(lhs.SheetInfo.RelicXpInfo);
                var rhsPairs = FirstRelicExp(rhs.SheetInfo.RelicXpInfo);

                var keyCompare = lhsPairs.Key.CompareTo(rhsPairs.Key);
                if (keyCompare != 0) return keyCompare;

                return lhsPairs.Value.CompareTo(rhsPairs.Value);
            }

            private static KeyValuePair<int, int> FirstRelicExp(Dictionary<int, int> relicXpInfo)
            {
                var found = false;
                var bestKey = 0;
                var bestValue = 0;
                foreach (var (key, value) in relicXpInfo)
                {
                    if (found && key >= bestKey)
                        continue;

                    found = true;
                    bestKey = key;
                    bestValue = value;
                }

                return found ? new KeyValuePair<int, int>(bestKey, bestValue) : default;
            }

            private static string RomanNumeral(int tier) => tier switch
            {
                1 => "I",
                2 => "II",
                3 => "III",
                4 => "IV",
                5 => "V",
                6 => "VI",
                7 => "VII",
                _ => "?"
            };

            public override void DrawColumn(MissionInfo item, int idx)
            {
                if (!HasVisibleRelicExp(item.SheetInfo.RelicXpInfo))
                {
                    ImGuiUtil.Center("-");
                    return;
                }

                DrawRelicExpPills(item.SheetInfo.RelicXpInfo);
            }

            private static bool HasVisibleRelicExp(Dictionary<int, int> relicXpInfo)
            {
                foreach (var value in relicXpInfo.Values)
                {
                    if (value != 0)
                        return true;
                }

                return false;
            }

            private static void DrawRelicExpPills(Dictionary<int, int> relicXpInfo)
            {
                var scale = ImGuiHelpers.GlobalScale;
                var drawList = ImGui.GetWindowDrawList();
                var start = ImGui.GetCursorScreenPos();
                var innerPadding = 3f * scale;
                var columnWidth = MathF.Max(1f, ImGuiUtil.CurrentColumnWidth - innerPadding * 2f);
                var spacing = 3f * scale;
                var height = MathF.Max(18f * scale, ImGui.GetTextLineHeight() + 4f * scale);
                var y = start.Y + MathF.Max(0, (ImGui.GetFrameHeight() - height) * 0.5f);

                var xPos = start.X + innerPadding;
                var right = start.X + innerPadding + columnWidth;

                for (var tier = 1; tier <= 7; tier++)
                {
                    if (!relicXpInfo.TryGetValue(tier, out var value) || value == 0)
                        continue;

                    var text = $"{RomanNumeral(tier)}:{value}";
                    var width = ImGui.CalcTextSize(text).X + 8f * scale;
                    if (xPos + width > right)
                        break;

                    var min = new Vector2(xPos, y);
                    var max = min + new Vector2(width, height);
                    drawList.AddRectFilled(min, max, ImGui.GetColorU32(ExpPillColor(tier)), 4f * scale);
                    drawList.AddRect(min, max, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.12f)), 4f * scale);

                    ImGui.SetCursorScreenPos(new Vector2(min.X + 4f * scale, min.Y + MathF.Max(0, (height - ImGui.GetTextLineHeight()) * 0.5f)));
                    ImGui.TextColored(Vector4.One, text);

                    xPos += width + spacing;
                }

                ImGui.SetCursorScreenPos(start);
            }

            private static Vector4 ExpPillColor(int tier) => tier switch
            {
                1 => new Vector4(0.9f, 0.8f, 0.1f, 0.82f),
                2 => new Vector4(0.9f, 0.5f, 0.1f, 0.82f),
                3 => new Vector4(0.8f, 0.2f, 0.2f, 0.82f),
                4 => new Vector4(0.6f, 0.2f, 0.8f, 0.82f),
                5 => new Vector4(0.2f, 0.4f, 0.9f, 0.82f),
                6 => new Vector4(0.4f, 0.8f, 1.0f, 0.82f),
                7 => new Vector4(0.2f, 0.8f, 0.3f, 0.82f),
                _ => new Vector4(0.5f, 0.5f, 0.5f, 0.82f),
            };

            public override bool FilterFunc(MissionInfo mission)
            {
                foreach (var (tier, value) in mission.SheetInfo.RelicXpInfo)
                {
                    if (value <= 0)
                        continue;

                    if (!FilterValue.HasFlag(TierFlag(tier)))
                        return false;
                }

                return true;
            }

            private static ItemFilter TierFlag(int tier) => tier switch
            {
                1 => ItemFilter.HasI,
                2 => ItemFilter.HasII,
                3 => ItemFilter.HasIII,
                4 => ItemFilter.HasIV,
                5 => ItemFilter.HasV,
                6 => ItemFilter.HasVI,
                7 => ItemFilter.HasVII,
                _ => 0,
            };
        }
        public sealed class RelicExpColumn : ItemFilterColumn
        {
            private readonly int _tier;
            private readonly ItemFilter _flag;

            public RelicExpColumn(int tier, ItemFilter flag)
            {
                Flags = ImGuiTableColumnFlags.None;
                _tier = tier;
                _flag = flag;
                SetFlags(ItemFilter.HasI, ItemFilter.HasII, ItemFilter.HasIII, ItemFilter.HasIV, ItemFilter.HasV, ItemFilter.HasVI, ItemFilter.HasVII);
                SetNames("I", "II", "III", "IV", "V", "VI", "VII");
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
                => lhs.SheetInfo.RelicXpInfo.GetValueOrDefault(_tier)
                   .CompareTo(rhs.SheetInfo.RelicXpInfo.GetValueOrDefault(_tier));

            public override void DrawColumn(MissionInfo mission, int _)
            {
                var value = mission.SheetInfo.RelicXpInfo.GetValueOrDefault(_tier);

                if (value == 0)
                {
                    ImGuiUtil.Center("-");
                    return;
                }

                Vector4 pillColor = _tier switch
                {
                    1 => new Vector4(0.3f, 0.5f, 0.8f, 0.8f),
                    2 => new Vector4(0.3f, 0.7f, 0.5f, 0.8f),
                    3 => new Vector4(0.7f, 0.6f, 0.2f, 0.8f),
                    4 => new Vector4(0.7f, 0.3f, 0.6f, 0.8f),
                    5 => new Vector4(0.8f, 0.4f, 0.2f, 0.8f),
                    6 => new Vector4(0.8f, 0.2f, 0.2f, 0.8f),
                    7 => new Vector4(0.6f, 0.8f, 0.9f, 0.8f),
                    _ => new Vector4(0.5f, 0.5f, 0.5f, 0.8f),
                };

                var buttonWidth = ImGui.CalcTextSize($"{value}").X + ImGui.GetStyle().FramePadding.X * 2;
                var columnWidth = ImGuiUtil.CurrentColumnWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - buttonWidth) / 2);

                using (ImRaii.PushColor(ImGuiCol.Button, pillColor)
                             .Push(ImGuiCol.ButtonHovered, pillColor with { W = 1.0f })
                             .Push(ImGuiCol.ButtonActive, pillColor))
                {
                    ImGui.SmallButton($"{value}##exp{_tier}");
                }
            }

            public override bool FilterFunc(MissionInfo mission)
            {
                var hasExp = mission.SheetInfo.RelicXpInfo.GetValueOrDefault(_tier) > 0;
                return hasExp ? FilterValue.HasFlag(_flag) : true;
            }
        }
        public sealed class MissionColumn : MissionFilterColumn
        {
            public MissionColumn()
            {
                Flags = ImGuiTableColumnFlags.None;
                SetFlags(MissionFilter.RedAlert, MissionFilter.Sequence, MissionFilter.Weather, MissionFilter.Timed, MissionFilter.ARank, MissionFilter.BRank, MissionFilter.CRank, MissionFilter.DRank);
                SetNames(T("Red Alert"), T("Sequence"), T("Weather"), T("Timed"), T("A Rank"), T("B Rank"), T("C Rank"), T("D Rank"));
            }

            private static int GetMissionPriority(CosmicInfo info)
            {
                if (info.IsCritical) return 10;
                if (info.IsSequence) return 9;
                if (info.IsWeather) return 8;
                if (info.IsTimed) return 7;
                // Rank 6 = Provisional (handled above), 5 = Ex, 4 = A, 3 = B, 2 = C, 1 = D
                return (int)info.Rank;
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs) =>
                GetMissionPriority(lhs.SheetInfo).CompareTo(GetMissionPriority(rhs.SheetInfo));
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var frameHeight = ImGui.GetFrameHeight();
                var iconSize = frameHeight - 2f * ImGuiHelpers.GlobalScale;
                var size = new Vector2(iconSize);
                var columnWidth = ImGuiUtil.CurrentColumnWidth;
                var start = ImGui.GetCursorScreenPos();

                void SetCenteredX(float contentWidth)
                    => ImGui.SetCursorScreenPos(new Vector2(start.X + MathF.Max(0, (columnWidth - contentWidth) * 0.5f), ImGui.GetCursorScreenPos().Y));

                if (item.SheetInfo.IsCritical || item.SheetInfo.IsWeather)
                {
                    var texture = item.SheetInfo.IsCritical ?
                        Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), "ICE.Resources.Red_Alert.png").GetWrapOrEmpty()
                      : CosmicHelper.WeatherIconDict[item.SheetInfo.Weather].GetWrapOrEmpty();

                    SetCenteredX(iconSize);
                    ImGui.Image(texture.Handle, size);
                }
                else
                {
                    if (item.SheetInfo.IsProvisional)
                    {
                        var icon = item.SheetInfo.IsTimed ? FontAwesomeIcon.Clock : FontAwesomeIcon.ListOl;
                        var iconText = icon.ToIconString();
                        using (ImRaii.PushFont(UiBuilder.IconFont))
                        {
                            SetCenteredX(ImGui.CalcTextSize(iconText).X);
                            ImGui.TextUnformatted(iconText);
                        }
                    }
                    else
                    {
                        string rank = item.SheetInfo.Rank switch
                        {
                            5 or 4 => "A",
                            3 => "B",
                            2 => "C",
                            1 => "D",
                            _ => "???"
                        };
                        SetCenteredX(ImGui.CalcTextSize(rank).X);
                        ImGui.TextUnformatted(rank);
                    }
                }
            }
            public override bool FilterFunc(MissionInfo item)
            {
                var sheetInfo = item.SheetInfo;
                bool special = sheetInfo.IsProvisional || sheetInfo.IsCritical;

                if (FilterValue.HasFlag(MissionFilter.RedAlert) && sheetInfo.IsCritical) return true;
                if (FilterValue.HasFlag(MissionFilter.Sequence) && sheetInfo.IsSequence) return true;
                if (FilterValue.HasFlag(MissionFilter.Timed) && sheetInfo.IsTimed) return true;
                if (FilterValue.HasFlag(MissionFilter.Weather) && sheetInfo.IsWeather) return true;
                if (FilterValue.HasFlag(MissionFilter.ARank) && sheetInfo.ARank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.BRank) && sheetInfo.BRank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.CRank) && sheetInfo.CRank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.DRank) && sheetInfo.Drank && !special) return true;

                return false;
            }
        }
        public sealed class SPMColumn : VerticalCenterColumnString
        {
            private static bool TryGetBestScore(MissionInfo item, out TurninState state, out RewardInfo reward)
            {
                state = default;
                reward = null;

                var scoreInfo = item.SheetInfo.ScoreInfo();
                if (item.SheetInfo.IsCritical && scoreInfo.TryGetValue(TurninState.Critical, out var critical))
                {
                    state = TurninState.Critical;
                    reward = critical;
                    return critical.Score != 0;
                }

                if (scoreInfo.TryGetValue(TurninState.SequenceGold, out var seqGold) && seqGold.Score != 0)
                {
                    state = TurninState.SequenceGold;
                    reward = seqGold;
                    return true;
                }

                foreach (var entry in scoreInfo)
                {
                    if (reward != null && entry.Value.Score <= reward.Score)
                        continue;

                    state = entry.Key;
                    reward = entry.Value;
                }

                return reward?.Score != 0;
            }

            private static double GetScore(MissionInfo item)
            {
                return TryGetBestScore(item, out _, out var reward)
                    ? reward.Score
                    : 0;
            }

            public override string ToName(MissionInfo item) => $"{GetScore(item):N2}";
            public override int Compare(MissionInfo x, MissionInfo y) => GetScore(x).CompareTo(GetScore(y));
            public override void DrawColumn(MissionInfo item, int _)
            {
                if (TryGetBestScore(item, out var bestState, out var bestReward))
                {
                    string scoreText = $"{bestReward.Score:N2}";

                    var buttonWidth = ImGui.CalcTextSize(scoreText).X + ImGui.GetStyle().FramePadding.X * 2;
                    var columnWidth = ImGuiUtil.CurrentColumnWidth;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - buttonWidth) / 2);

                    Vector4 pillColor = bestState switch
                    {
                        TurninState.Bronze => new(0.6f, 0.35f, 0.15f, 1.0f), // darker bronze
                        TurninState.Silver => new(0.6f, 0.6f, 0.6f, 1.0f),
                        TurninState.Gold => new(0.85f, 0.70f, 0.0f, 1.0f), // slightly muted gold
                        TurninState.Critical => new(0.7f, 0.1f, 0.9f, 1.0f), // purple feels "special"
                        TurninState.SequenceGold => new(0.95f, 0.60f, 0.0f, 1.0f),  // amber-gold, more orange warmth
                        _ => new(0.5f, 0.5f, 0.5f, 0.8f)
                    };

                    bool isBright = bestState is TurninState.Gold or TurninState.Silver or TurninState.SequenceGold;
                    Vector4 textColor = isBright ? new(0.1f, 0.1f, 0.1f, 1.0f) : new(1.0f, 1.0f, 1.0f, 1.0f);

                    using (ImRaii.PushColor(ImGuiCol.Button, pillColor)
                        .Push(ImGuiCol.ButtonHovered, pillColor with { W = 1.0f })
                        .Push(ImGuiCol.ButtonActive, pillColor)
                        .Push(ImGuiCol.Text, textColor))
                    {
                        ImGui.SmallButton(scoreText);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Average Rewards per minute"));
                        if (ImGui.BeginTable($"Score Info Table_{item.SheetInfo.MissionId}", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                        {
                            ImGui.TableSetupColumn(T("Kind"));
                            ImGui.TableSetupColumn(T("Score"));
                            ImGui.TableSetupColumn(T("Credits"));
                            ImGui.TableSetupColumn(T("Planetary"));
                            ImGui.TableSetupColumn(T("Tokens"));

                            ImGui.TableHeadersRow();

                            foreach (var entry in item.SheetInfo.ScoreInfo())
                            {
                                if (entry.Value.Score == 0)
                                    continue;

                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.Text($"{T(entry.Key.ToString())} [{entry.Value.Completions:N0}]");

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
                        ImGui.EndTooltip();
                    }
                }
                else
                {
                    ImGuiUtil.Center("-");
                }

            }
        }
        public sealed class TurninColumn : ItemFilterColumn
        {
            public TurninColumn()
            {
                Flags = ImGuiTableColumnFlags.None;
                SetFlags(ItemFilter.TurninGold, ItemFilter.TurninSilver, ItemFilter.TurninBronze);
                SetNames(T("Gold"), T("Silver"), T("Bronze"));
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs)
            {
                if (C.MissionConfig.TryGetValue(lhs.Id, out var lhsConfig) && C.MissionConfig.TryGetValue(rhs.Id, out var rhsConfig))
                {
                    return lhsConfig.TurninGoal.CompareTo(rhsConfig.TurninGoal);
                }
                else
                {
                    return 0;
                }
            }
            public override void DrawColumn(MissionInfo item, int idx)
            {
                if (item.SheetInfo.Attributes.HasFlag(MissionAttributes.Score_TimeRemaining) || item.SheetInfo.IsCritical)
                    ImGuiUtil.Center(T("Auto"));
                else
                {
                    Vector4 BronzeColor = new Vector4(0.804f, 0.498f, 0.196f, 1.0f);
                    Vector4 SilverColor = new Vector4(0.753f, 0.753f, 0.753f, 1.0f);
                    Vector4 GoldColor = new Vector4(1.0f, 0.843f, 0.0f, 1.0f);
                    Vector4 DisabledColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);

                    ImGui.PushID($"Mission_{item.Id}");

                    if (C.MissionConfig.TryGetValue(item.Id, out var configInfo))
                    {
                        var highestTurnin = configInfo.TurninGoal;
                        var goldEnabled = highestTurnin >= TurninState.Gold;
                        var silverEnabled = highestTurnin >= TurninState.Silver;
                        var bronzeEnabled = highestTurnin >= TurninState.Bronze;
                        var spacing = ImGui.GetStyle().ItemSpacing.X;
                        var buttonWidth = ImGui.GetFrameHeight();
                        using (ImRaii.PushFont(UiBuilder.IconFont))
                            buttonWidth = ImGui.CalcTextSize(FontAwesomeIcon.Trophy.ToIconString()).X + ImGui.GetStyle().FramePadding.X * 2f;

                        var totalWidth = buttonWidth * 3f + spacing * 2f;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0, (ImGuiUtil.CurrentColumnWidth - totalWidth) * 0.5f));

                        using (ImRaii.PushColor(ImGuiCol.Text, goldEnabled ? GoldColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Gold"))
                            {
                                configInfo.TurninGoal = TurninState.Gold;
                                C.SaveDebounced();
                            }
                        }
                        ImGui.SameLine(0, spacing);
                        using (ImRaii.PushColor(ImGuiCol.Text, silverEnabled ? SilverColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Silver"))
                            {
                                configInfo.TurninGoal = TurninState.Silver;
                                C.SaveDebounced();
                            }

                        }
                        ImGui.SameLine(0, spacing);
                        using (ImRaii.PushColor(ImGuiCol.Text, bronzeEnabled ? BronzeColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Bronze"))
                            {
                                configInfo.TurninGoal = TurninState.Bronze;
                                C.SaveDebounced();
                            }
                        }

                    }

                    ImGui.PopID();
                }
            }
        }
        public sealed class PlanetColumn : ItemFilterColumn
        {
            public PlanetColumn()
            {
                Flags = ImGuiTableColumnFlags.None;
                SetFlags(ItemFilter.Sinus, ItemFilter.Phaenna, ItemFilter.Oizys, ItemFilter.Auxesia);
                SetNames(T("Sinus"), T("Phaenna"), T("Oizys"), T("Auxesia"));
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.TerritoryId.CompareTo(rhs.SheetInfo.TerritoryId);
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight - 2);

                var columnWidth = ImGuiUtil.CurrentColumnWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

                string planetIcon = item.SheetInfo.TerritoryId switch
                {
                    1237 => "ICE.Resources.Sinus_Ardorum.png",
                    1291 => "ICE.Resources.Phaenna.png",
                    1310 => "ICE.Resources.Oizys.png",
                    1319 => "ICE.Resources.Auxesia.png",
                    _ => "ICE.Resources.Sinus_Ardorum.png",
                };

                var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), planetIcon).GetWrapOrEmpty();
                ImGui.Image(texture.Handle, size);
            }
            public override bool FilterFunc(MissionInfo item)
            {
                return item.SheetInfo.TerritoryId switch
                {
                    1237 => FilterValue.HasFlag(ItemFilter.Sinus),
                    1291 => FilterValue.HasFlag(ItemFilter.Phaenna),
                    1310 => FilterValue.HasFlag(ItemFilter.Oizys),
                    1319 => FilterValue.HasFlag(ItemFilter.Auxesia),
                    _ => false
                };
            }
        }
        public sealed class JobColumn : JobFilterColumn
        {
            public JobColumn()
            {
                Flags = ImGuiTableColumnFlags.None;
                SetFlagsAndNames(JobFilter.CRP, JobFilter.BSM, JobFilter.ARM, JobFilter.GSM,
                                 JobFilter.LTW, JobFilter.WVR, JobFilter.ALC, JobFilter.CUL,
                                 JobFilter.MIN, JobFilter.BTN, JobFilter.FSH);
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.Jobs.First().CompareTo(rhs.SheetInfo.Jobs.First());

            public override void DrawColumn(MissionInfo item, int idx)
            {
                var frameHeight = ImGui.GetFrameHeight();
                var iconSize = frameHeight - 2f * ImGuiHelpers.GlobalScale;
                var size = new Vector2(iconSize);
                var jobs = item.SheetInfo.Jobs;
                if (jobs.Count == 0)
                    return;

                var columnWidth = ImGuiUtil.CurrentColumnWidth;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + MathF.Max(0, (columnWidth - iconSize) * 0.5f));

                var job = jobs.First();
                var icon = CosmicHelper.ClassInfoDict[job].JobIcon;
                ImGui.Image(icon.GetWrapOrEmpty().Handle, size);
                if (jobs.Count > 1 && ImGui.IsItemHovered())
                {
                    var jobNames = string.Join(" / ", jobs.Select(GetJobName));
                    ImGui.SetTooltip(jobNames);
                }
            }
            public override bool FilterFunc(MissionInfo item)
            {
                return item.SheetInfo.Jobs.Any(job =>
                {
                    var flag = job switch
                    {
                        8 => JobFilter.CRP,
                        9 => JobFilter.BSM,
                        10 => JobFilter.ARM,
                        11 => JobFilter.GSM,
                        12 => JobFilter.LTW,
                        13 => JobFilter.WVR,
                        14 => JobFilter.ALC,
                        15 => JobFilter.CUL,
                        16 => JobFilter.MIN,
                        17 => JobFilter.BTN,
                        18 => JobFilter.FSH,
                        _ => JobFilter.None,
                    };
                    return FilterValue.HasFlag(flag);
                });
            }
        }
        public sealed class ProfileColumn : ItemFilterColumn
        {
            public ProfileColumn()
            {
                Flags = ImGuiTableColumnFlags.None;
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.Jobs.First().CompareTo(rhs.SheetInfo.Jobs.First());
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var sheetInfo = item.SheetInfo;
                var buttonInset = 3f * ImGuiHelpers.GlobalScale;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + buttonInset);
                var buttonSize = new Vector2(MathF.Max(1f, ImGuiUtil.CurrentColumnWidth - buttonInset * 2f), 0);
                bool craftProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Craft);
                bool gatherProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Gather);
                bool collectable = sheetInfo.Attributes.HasFlag(MissionAttributes.Collectables) || sheetInfo.Attributes.HasFlag(MissionAttributes.ReducedItems);
                bool fishProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Fish);

                ImGui.PushID($"Mission: {item.Id}");


                if (sheetInfo.Attributes.HasFlag(MissionAttributes.Craft))
                {
                    if (ImGui.Button($"{T("Craft Settings")}##Craft_{item.Id}", buttonSize))
                    {
                        ImGui.OpenPopup("Craft Settings: Recipies");
                    }
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip(T("Open Craft Settings"));

                    using var popupStyle = ImGuiUtil.PushDefaultPopupStyle();
                    if (ImGui.BeginPopup("Craft Settings: Recipies"))
                    {
                        ImGui.TextDisabled($"{item.Id}");
                        ImGui.SameLine();
                        ImGui.Text(T("Mission: {0}", sheetInfo.Name));

                        CrafterManagement(sheetInfo, item.Id);

                        ImGui.EndPopup();
                    }
                }

                if (sheetInfo.Jobs.Count > 1)
                {
                    // ImGui.SameLine();
                }

                if (gatherProfile)
                {
                    if (!collectable)
                    {
                        string profileName = "???";
                        if (C.MissionConfig.TryGetValue(item.Id, out var config))
                        {
                            if (C.GatherProfiles.TryGetValue(config.GProfileId, out var profileSetting))
                            {
                                profileName = profileSetting.Name;
                            }

                            if (ImGui.Button($"{profileName}##{item.Id}_{item.SheetInfo.Name}", buttonSize))
                            {
                                ImGui.OpenPopup(T("Select Gather Profile"));
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Select gathering profile"));
                            }
                            if (ImGui.BeginPopup(T("Select Gather Profile")))
                            {
                                ImGui.Text(T("Mission: [{0}] {1}", item.Id, item.SheetInfo.Name));
                                ImGui.Text(T("Currently Selected: {0}", profileName));
                                ImGui.Separator();

                                foreach (var profile in C.GatherProfiles)
                                {
                                    var id = profile.Key;
                                    bool profileSelected = config.GProfileId == id;
                                    ImGui.PushID($"{id}_{profile.Value.Name}");
                                    if (ImGui.RadioButton(profile.Value.Name, profileSelected))
                                    {
                                        config.GProfileId = id;
                                        C.Save();
                                    }
                                    ImGui.PopID();
                                }

                                ImGui.EndPopup();
                            }
                        }
                    }
                    else
                    {
                        ImGuiUtil.Center(T("Auto"));
                    }
                }
                else if (sheetInfo.Attributes.HasFlag(MissionAttributes.Fish))
                {
                    if (C.MissionConfig.TryGetValue(item.Id, out var config))
                    {
                        if (ImGui.Button(T("Fishing Settings"), buttonSize))
                        {
                            ImGui.OpenPopup(T("Select Fishing Profile"));
                        }
                        if (ImGui.BeginPopup(T("Select Fishing Profile")))
                        {
                            ImGui.Text(T("Fishing profile: {0}", sheetInfo.Name));
                            ImGui.Separator();
                            bool builtInPreset = config.Use_BuildinPreset;
                            if (ImGui.Checkbox(T("Use Built In Preset"), ref builtInPreset))
                            {
                                config.Use_BuildinPreset = builtInPreset;
                                C.Save();
                            }
                            ImGuiEx.HelpMarker(T("Having this enabled means it will use the default preset that is included with the plugin for autohook. \n" +
                                                "If you would like to use one that you already have in autohook, you can un-checkmark this and type the name of it below"));
                            using (ImRaii.Disabled(builtInPreset))
                            {
                                string presetName = config.AutoHookPresetName;
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputText(T("Preset Name"), ref presetName))
                                {
                                    config.AutoHookPresetName = presetName;
                                    C.Save();
                                }
                            }

                            ImGui.EndPopup();
                        }
                    }
                }
            }
        }
        public sealed class NotesColumn : ItemFilterColumn
        {
            public NotesColumn()
            {
                Flags = ImGuiTableColumnFlags.None;
                SetFlags(ItemFilter.BestSPM, ItemFilter.Sequence, ItemFilter.Unlock, ItemFilter.NoNotes);
                SetNames(T("Best Score Per Minute"), T("Sequence"), T("Needs Unlocked"), T("No Notes"));
            }
            public override void DrawColumn(MissionInfo item, int idx)
                => DrawNoteIcons(item, false);
        }

        private static void DrawNoteIcons(MissionInfo item, bool inlineAfterPreviousItem)
        {
            var sheetInfo = item.SheetInfo;
            var hasSPM = sheetInfo.BestSPM.SPM > 0;
            var hasSequence = sheetInfo.SequenceMissions_Next.Count > 0 || sheetInfo.SequenceMissions_Previous.Count > 0;
            var hasUnlockable = sheetInfo.MissionUnlock.Count > 0;
            var drewAny = false;

            void SameLineForNextIcon()
            {
                if (inlineAfterPreviousItem || drewAny)
                    ImGui.SameLine();
            }

            if (hasSPM)
            {
                SameLineForNextIcon();
                ImGuiEx.Icon(FontAwesomeIcon.Trophy);
                drewAny = true;
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("Average SPM: {0:N2}", sheetInfo.BestSPM.SPM));
                    ImGui.Text(T(sheetInfo.BestSPM.NoteInfo));
                    ImGui.EndTooltip();
                }
            }

            if (hasSequence)
            {
                SameLineForNextIcon();
                ImGuiEx.Icon(FontAwesomeIcon.ListOl);
                drewAny = true;
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    if (sheetInfo.SequenceMissions_Next.Count > 0)
                    {
                        ImGui.Text(T("Next Sequence:"));
                        foreach(var mission in sheetInfo.SequenceMissions_Next)
                        {
                            var seqInfo = CosmicHelper.SheetMissionDict[mission];
                            ImGui.Text($"[{mission}] {seqInfo.Name}");
                        }
                    }
                    if (sheetInfo.SequenceMissions_Previous.Count > 0)
                    {
                        ImGui.Text(T("Previous Sequence:"));
                        foreach (var mission in sheetInfo.SequenceMissions_Previous)
                        {
                            var seqInfo = CosmicHelper.SheetMissionDict[mission];
                            ImGui.Text($"[{mission}] {seqInfo.Name}");
                        }
                    }
                    ImGui.EndTooltip();
                }
            }

            if (hasUnlockable)
            {
                SameLineForNextIcon();
                if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex)
                {
                    var frameHeight = ImGui.GetFrameHeight();
                    var size = new Vector2(frameHeight);
                    if (tex.TryGetWrap(out var wrap, out var exc))
                    {
                        ImGui.Image(wrap.Handle, size, new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
                    }
                }
                drewAny = true;
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(T("The following missions are required to have gold before you can do this one"));
                    foreach (var mission in sheetInfo.MissionUnlock)
                    {
                        ImGui_Ice.CompletionStatusIcon(CosmicHelper.SheetMissionDict[mission]);
                        ImGui.SameLine();
                        ImGui.Text($"[{mission}] - {CosmicHelper.SheetMissionDict[mission].Name}");
                    }
                    ImGui.EndTooltip();
                }
            }
        }

        private static int CountNoteIcons(MissionInfo item)
        {
            var sheetInfo = item.SheetInfo;
            var count = 0;
            if (sheetInfo.BestSPM.SPM > 0)
                count++;
            if (sheetInfo.SequenceMissions_Next.Count > 0 || sheetInfo.SequenceMissions_Previous.Count > 0)
                count++;
            if (sheetInfo.MissionUnlock.Count > 0)
                count++;
            return count;
        }
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
                                string macroName = recipeConfig.MacroName;
                                ImGui.SameLine();
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputText(T("Macro Name"), ref macroName))
                                {
                                    recipeConfig.MacroName = macroName;
                                    C.Save();
                                }
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

                            if (mission.TemporaryActionCount != 0)
                            {
                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                var actionInfo = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(mission.TemporaryActionId);
                                var name = actionInfo.Name;
                                var icon = Svc.Texture.GetFromGameIcon((int)actionInfo.Icon).GetWrapOrEmpty();
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
                                    if (ImGui.SliderInt("##MaxSkillUsage", ref maxUsage, -1, (int)mission.TemporaryActionCount, skillUsageLabel))
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

                                    if (mission.TemporaryActionId == 41269 && !globalArtisan)
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
}
