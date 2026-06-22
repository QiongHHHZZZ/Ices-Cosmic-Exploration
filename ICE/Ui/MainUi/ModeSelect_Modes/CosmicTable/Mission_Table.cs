using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.GatheringHelper;
using ICE.Utilities.ImGuiTools;
using OtterGui;
using OtterGui.Table;
using OtterGui.Text;
using System.Collections.Generic;
using System.Reflection;
using static ICE.ConfigFiles.Config;
using static ICE.Localization.L10n;
using static ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable.Mission_Table;
using static ICE.Utilities.Cosmic_Helper.CosmicHelper;

namespace ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable
{
    internal class VerticalCenterColumnString : ColumnString<MissionInfo>
    {
        public override void PreDraw()
        {
            var pos = ImGui.GetCursorPosY();
            var offset = (ImageSize - ImGui.GetTextLineHeight()) / 2;
            ImGui.SetCursorPosY(pos + offset);
        }
    }

    internal abstract class LocalizedColumnFlags<T> : ColumnFlags<T, MissionInfo> where T : struct, Enum
    {
        public override bool DrawFilter()
        {
            using var id = ImUtf8.PushId(FilterLabel);
            using var style = OtterGui.Raii.ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
            ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
            var all = FilterValue.HasFlag(AllFlags);
            using var color = OtterGui.Raii.ImRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !all);
            using var combo = ImUtf8.Combo(""u8, Label, ComboFlags);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                SetValue(AllFlags, true);
                return true;
            }

            if (!all)
                ImUtf8.HoverTooltip(T("Right-click to clear filters."));

            if (!combo)
                return false;

            color.Pop();

            var ret = false;
            if (ImUtf8.Checkbox(T("Enable All"), ref all))
            {
                SetValue(AllFlags, all);
                ret = true;
            }

            using var indent = OtterGui.Raii.ImRaii.PushIndent(10f);
            for (var i = 0; i < Names.Length; ++i)
            {
                if (!DrawCheckbox(i, out var tmp))
                    continue;

                SetValue(Values[i], tmp);
                ret = true;
            }

            return ret;
        }
    }

    internal class ItemFilterColumn : LocalizedColumnFlags<ItemFilter>
    {
        private ItemFilter[] FlagValues = Array.Empty<ItemFilter>();
        private string[] FlagNames = Array.Empty<string>();

        private string ReturnFlagNames(ItemFilter item)
        {
            return item switch
            {
                ItemFilter.NoItems => "No Items",
                ItemFilter.Enabled => "Enabled",
                ItemFilter.Disabled => "Disabled",
                // ItemFilter.NotCompleted => "Not Completed",
                // ItemFilter.Completed => "Completed",
                // ItemFilter.Gold => "Gold",
                _ => "Unknown",
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

        protected sealed override string[] Names => FlagNames.Select(n => T(n)).ToArray();

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
    internal class MissionFilterColumn : LocalizedColumnFlags<MissionFilter>
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

        protected sealed override string[] Names => FlagNames.Select(n => T(n)).ToArray();

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
    internal class JobFilterColumn : LocalizedColumnFlags<JobFilter>
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

        protected sealed override string[] Names => FlagNames.Select(n => T(n)).ToArray();

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

        public readonly EnabledColumn _enabledColumn;
        public readonly NameColumn _nameColumn = new() { Label = T("Mission Table Name") };
        public readonly IdColumn _idColumn = new() { Label = T("Mission Table ID") };
        public readonly JobColumn _jobColumn = new() { Label = T("Mission Table Job") };
        public readonly MissionColumn _missionColumn = new() { Label = T("Mission Table Rank") };
        public readonly CompletionColumn _completionColumn = new() { Label = T("Mission Table Status") };
        public readonly ClassScoreColumn _classScoreColumn = new() { Label = T("Mission Table Class Score") };
        public readonly CosmocreditColumn _cosmoColumn = new() { Label = T("Mission Table Cosmo") };
        public readonly LunarCreditColumn _lunarColumn = new() { Label = T("Mission Table Lunar") };
        public readonly DroneCreditColumn _droneColumn = new() { Label = T("Mission Table Dronebits") };
        public readonly PlanetTokensColumn _planetTokenColumn = new() { Label = T("Mission Table Planet Tokens") };
        public readonly SPMColumn _spmColumn = new() { Label = T("Mission Table SPM") };
        public readonly TurninColumn _turninColumn = new() { Label = T("Mission Table Turnin Goal") };
        public readonly PlanetColumn _planetColumn = new() { Label = T("Mission Table Moons") };
        public readonly ProfileColumn _profileColumn = new() { Label = T("Mission Table Profile") };
        public readonly NotesColumn _notesColumn = new() { Label = T("Mission Table Notes") };
        public readonly AllRelicExpColum _allExpColumn = new() { Label = T("Mission Table Exp") };

        public Mission_Table(List<MissionInfo> itemList) : base("Item_Table_V2", itemList)
        {
            _enabledColumn = new EnabledColumn(this) { Label = T("Mission Table Enabled") };

            List<Column<MissionInfo>> headers = [
                _enabledColumn, _completionColumn, _idColumn, _planetColumn,
                _jobColumn, _missionColumn, _nameColumn, _classScoreColumn,
                _cosmoColumn, _lunarColumn, _droneColumn, _planetTokenColumn,
                _spmColumn,  _turninColumn, _allExpColumn];


            var tierFlags = new (int tier, ItemFilter flag)[]
            {
                (1, ItemFilter.HasI),   (2, ItemFilter.HasII),  (3, ItemFilter.HasIII),
                (4, ItemFilter.HasIV),  (5, ItemFilter.HasV),   (6, ItemFilter.HasVI),
                (7, ItemFilter.HasVII)
            };

            foreach (var (tier, flag) in tierFlags)
            {
                string tierName = tier switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V", 6 => "VI", 7 => "VII", _ => "?" };
                headers.Add(new RelicExpColumn(tier, flag) { Label = T("Mission Table Exp {0}", tierName) });
            }

            headers.Add(_profileColumn, _notesColumn);
            this.Headers = [.. headers];

            Sortable = true;
            Flags |= ImGuiTableFlags.Hideable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit;
        }

        public void Dispose()
        {
        }
        public sealed class EnabledColumn : ItemFilterColumn
        {
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            private readonly Mission_Table _table;
            public EnabledColumn(Mission_Table table)
            {
                _table = table;
                Flags = ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.Enabled, ItemFilter.Disabled);
                SetNames("Enabled", "Disabled");
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
                => lhs.Enabled().CompareTo(rhs.Enabled());

            public override void DrawColumn(MissionInfo item, int _)
            {
                ImGui.PushID(item.Id);

                var mission = CosmicHelper.CurrentLunarMission;

                if (mission != 0 && mission == item.Id)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                }
                else if (CosmicHandler.All_AvailableMissions().Contains(item.Id))
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                }


                bool disabled = C.SelectedMode == ModeSelect.MissionGoldMode
                             || C.SelectedMode == ModeSelect.LevelMode
                             || (C.SelectedMode == ModeSelect.RelicMode && !C.XPRelicOnlyEnabled);

                if (!disabled)
                {
                    bool enabled = C.MissionConfig[item.Id].Enabled;
                    if (ImGui_Ice.Table_CenterCheckbox("##EnableMission", ref enabled))
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
                        _table.SetFilterDirty();
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
                return item.Enabled() ? FilterValue.HasFlag(ItemFilter.Enabled) : FilterValue.HasFlag(ItemFilter.Disabled);
            }
        }
        public sealed class NameColumn : VerticalCenterColumnString
        {
            public NameColumn() => Flags |= ImGuiTableColumnFlags.NoHide;
            public override string ToName(MissionInfo mission) => mission.SheetInfo.Name;
            public override void DrawColumn(MissionInfo mission, int _)
            {
                if (UnsupportedMissions.Ids.Contains(mission.Id))
                {
                    using (var warningPush = ImRaii.PushColor(ImGuiCol.Text, EColor.Red))
                    {
                        ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("This mission is currently missing stuff to allow it to work. It might be planet locked, or could be just needs mapped out\nI'll get to it when my world gets to it o/"));
                        ImGui.EndTooltip();
                    }
                }
                if (mission.SheetInfo.Jobs.Contains(18) && !GatheringUtil.FishingPreset.ContainsKey(mission.Id))
                {
                    using (var warningPush = ImRaii.PushColor(ImGuiCol.Text, EColor.Yellow))
                    {
                        ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("This mission doesn't have a fishing preset yet, this is your warning for this."));
                        ImGui.EndTooltip();
                    }
                }
                if (ImGui.Button(mission.SheetInfo.Name))
                {
                    IceLogging.Verbose("Testing... if this fires off multiple times", "DEBUG TEST");
                    Window_ExternalDetails.SelectedMission = mission.Id;
                    P.externalDetails.IsOpen = true;
                    IceLogging.Verbose($"Collasped condition: {P.externalDetails.CollapsedCondition.ToString()}");
                }
                if (mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Gather) || mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Fish))
                {
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"Flag_{mission.Id}"))
                    {
                        Window_ExternalDetails.SelectedMission = mission.Id;
                        Utils.SetGatheringRing(mission.SheetInfo.TerritoryId, (int)mission.SheetInfo.MapPosition.X, (int)mission.SheetInfo.MapPosition.Y, mission.SheetInfo.Radius, mission.SheetInfo.Name);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"X: {mission.SheetInfo.MapPosition.X:N0}, Z: {mission.SheetInfo.MapPosition.Y:N0}");
                        ImGui.EndTooltip();
                    }
                }
                if (GatheringUtil.CriticalSpots.TryGetValue(mission.SheetInfo.Critical_MapKey, out var criticalInfo))
                {
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.FlagCheckered, $"CriticalFlag_{mission.Id}"))
                    {
                        Utils.SetGatheringRing(mission.SheetInfo.TerritoryId, criticalInfo.X, criticalInfo.Y, criticalInfo.Radius, $"Red Alert: {mission.SheetInfo.Name}", criticalInfo.IconId);
                    }
#if DEBUG
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Critical Route: {0}", mission.SheetInfo.Critical_MapKey));
                        ImGui.Separator();
                        ImGui.Text(T("Map Coordinates: {0} | {1}", criticalInfo.X, criticalInfo.Y));
                        ImGui.Separator();
                        ImGui.Text(T("World Position: {0:N2} | {1:N2} | {2:N2}", criticalInfo.WorldCords.X, criticalInfo.WorldCords.Y, criticalInfo.WorldCords.Z));
                        ImGui.EndTooltip();
                    }
#endif
                }
            }
        }
        public sealed class IdColumn : VerticalCenterColumnString
        {
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public IdColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
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
                SetFlags(ItemFilter.NotCompleted, ItemFilter.Completed, ItemFilter.Gold);
                SetNames("Not Completed", "Completed", "Gold");
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.CompletionStatus.CompareTo(rhs.SheetInfo.CompletionStatus);
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

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

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetStyle().FramePadding.X);

                    using (ImRaii.PushFont(UiBuilder.IconFont))
                    using (ImRaii.PushColor(ImGuiCol.Text, color))
                    {
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
            public ClassScoreColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override string ToName(MissionInfo mission) => mission.SheetInfo.ClassScore.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.ClassScore.CompareTo(rhs.SheetInfo.ClassScore);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.ClassScore}");
            }
        }
        public sealed class CosmocreditColumn : VerticalCenterColumnString
        {
            public CosmocreditColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override string ToName(MissionInfo mission) => mission.SheetInfo.CosmoCredit.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.CosmoCredit.CompareTo(rhs.SheetInfo.CosmoCredit);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.CosmoCredit}");
            }
        }
        public sealed class LunarCreditColumn : VerticalCenterColumnString
        {
            public LunarCreditColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override string ToName(MissionInfo mission) => mission.SheetInfo.LunarCredit.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.LunarCredit.CompareTo(rhs.SheetInfo.LunarCredit);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.LunarCredit}");
            }
        }
        public sealed class DroneCreditColumn : VerticalCenterColumnString
        {
            public DroneCreditColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override string ToName(MissionInfo mission) => mission.SheetInfo.DronebitReward.ToString();
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.DronebitReward.CompareTo(rhs.SheetInfo.DronebitReward);
            public override void DrawColumn(MissionInfo mission, int _)
            {
                ImGuiUtil.Center($"{mission.SheetInfo.DronebitReward}");
            }
        }
        public sealed class PlanetTokensColumn : ItemFilterColumn
        {
            public PlanetTokensColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.HasTokens, ItemFilter.NoTokens);
                SetNames("Has Tokens", "No Tokens");
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
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
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.HasI, ItemFilter.HasII, ItemFilter.HasIII, ItemFilter.HasIV, ItemFilter.HasV, ItemFilter.HasVI, ItemFilter.HasVII);
                SetNames("I", "II", "III", "IV", "V", "VI", "VII");
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
            {
                var lhsPairs = lhs.SheetInfo.RelicXpInfo.OrderBy(x => x.Key).ThenBy(x => x.Value).FirstOrDefault();
                var rhsPairs = rhs.SheetInfo.RelicXpInfo.OrderBy(x => x.Key).ThenBy(x => x.Value).FirstOrDefault();

                var keyCompare = lhsPairs.Key.CompareTo(rhsPairs.Key);
                if (keyCompare != 0) return keyCompare;

                return lhsPairs.Value.CompareTo(rhsPairs.Value);
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
                var exps = item.SheetInfo.RelicXpInfo
                    .Where(x => x.Value != 0)
                    .OrderBy(x => x.Key)
                    .ToList();

                if (exps.Count == 0)
                {
                    ImGuiUtil.Center("-");
                    return;
                }

                float spacing = ImGui.GetStyle().ItemSpacing.X;
                float padding = ImGui.GetStyle().FramePadding.X;

                // Calculate total width of all pills + spacing between them
                float totalWidth = exps.Sum(x => ImGui.CalcTextSize($"{RomanNumeral(x.Key)}:{x.Value}").X + padding * 2);
                totalWidth += spacing * (exps.Count - 1);

                float columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - totalWidth) / 2);

                for (int i = 0; i < exps.Count; i++)
                {
                    var (tier, value) = (exps[i].Key, exps[i].Value);

                    Vector4 pillColor = tier switch
                    {
                        1 => new Vector4(0.9f, 0.8f, 0.1f, 0.8f), // I   - Yellow
                        2 => new Vector4(0.9f, 0.5f, 0.1f, 0.8f), // II  - Orange
                        3 => new Vector4(0.8f, 0.2f, 0.2f, 0.8f), // III - Red
                        4 => new Vector4(0.6f, 0.2f, 0.8f, 0.8f), // IV  - Purple
                        5 => new Vector4(0.2f, 0.4f, 0.9f, 0.8f), // V   - Blue
                        6 => new Vector4(0.4f, 0.8f, 1.0f, 0.8f), // VI  - Light Blue
                        7 => new Vector4(0.2f, 0.8f, 0.3f, 0.8f), // VII - Green
                        _ => new Vector4(0.5f, 0.5f, 0.5f, 0.8f),
                    };

                    string roman = tier switch
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


                    using (ImRaii.PushColor(ImGuiCol.Button, pillColor)
                                 .Push(ImGuiCol.ButtonHovered, pillColor with { W = 1.0f })
                                 .Push(ImGuiCol.ButtonActive, pillColor))
                    {
                        ImGui.SmallButton($"{roman}:{value}##exp{tier}_{idx}");
                    }

                    if (i < exps.Count - 1)
                        ImGui.SameLine();
                }
            }

            public override bool FilterFunc(MissionInfo mission)
            {
                var exps = mission.SheetInfo.RelicXpInfo.Where(x => x.Value > 0).ToList();

                if (exps.Count == 0) return true;

                return exps.Any(x => FilterValue.HasFlag(TierToFlag(x.Key)));
            }
        }
        public sealed class RelicExpColumn : ItemFilterColumn
        {
            private readonly int _tier;
            private readonly ItemFilter _flag;

            public RelicExpColumn(int tier, ItemFilter flag)
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                _tier = tier;
                _flag = flag;
                SetFlags(ItemFilter.HasI, ItemFilter.HasII, ItemFilter.HasIII, ItemFilter.HasIV, ItemFilter.HasV, ItemFilter.HasVI, ItemFilter.HasVII);
                SetNames("I", "II", "III", "IV", "V", "VI", "VII");
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );

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
                var columnWidth = ImGui.GetColumnWidth();
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
                var exps = mission.SheetInfo.RelicXpInfo.Where(x => x.Value > 0).ToList();

                if (exps.Count == 0) return true;

                return exps.Any(x => FilterValue.HasFlag(TierToFlag(x.Key)));
            }
        }
        public sealed class MissionColumn : MissionFilterColumn
        {
            public MissionColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(MissionFilter.RedAlert, MissionFilter.Sequence, MissionFilter.Weather, MissionFilter.Timed, MissionFilter.ARank, MissionFilter.BRank, MissionFilter.CRank, MissionFilter.DRank, MissionFilter.Master);
                SetNames("Red Alert", "Sequence", "Weather", "Timed", "A Rank", "B Rank", "C Rank", "D Rank", "Master");
            }
            public override float Width => Math.Max(ImGui.CalcTextSize(Label + "XX").X + ImGui.GetStyle().CellPadding.X * 2, ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2);

            private static int GetMissionPriority(CosmicInfo info)
            {
                if (info.IsCritical) return 10;
                if (info.IsSequence) return 9;
                if (info.IsWeather) return 8;
                if (info.IsTimed) return 7;
                if (info.IsMaster) return 6;
                // 5 = Ex, 4 = A, 3 = B, 2 = C, 1 = D
                return (int)info.Rank;
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs) =>
                GetMissionPriority(lhs.SheetInfo).CompareTo(GetMissionPriority(rhs.SheetInfo));
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

                if (item.SheetInfo.IsCritical || item.SheetInfo.IsWeather)
                {
                    var texture = item.SheetInfo.IsCritical ?
                        Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), "ICE.Resources.Red_Alert.png").GetWrapOrEmpty()
                      : CosmicHelper.WeatherIconDict[item.SheetInfo.Weather].GetWrapOrEmpty();

                    ImGui.Image(texture.Handle, size);
                }
                else
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetStyle().FramePadding.X);
                    if (item.SheetInfo.IsProvisional)
                    {
                        var icon = item.SheetInfo.IsTimed ? FontAwesomeIcon.Clock : FontAwesomeIcon.ListOl;
                        ImGuiEx.Icon(icon);
                        if (item.SheetInfo.IsTimed)
                        {
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Time Slot"));
                                ImGui.Text($"{item.SheetInfo.StartTime:D2}:00 - {item.SheetInfo.EndTime:D2}:00");
                                ImGui.EndTooltip();
                            }
                        }
                        else if (item.SheetInfo.IsSequence)
                        {
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Sequence Missions"));
                                if (item.SheetInfo.SequenceMissions_Previous.Count() > 0)
                                {
                                    ImGui.Separator();
                                    ImGui.Text(T("Previous Missions"));
                                    foreach (var mission in item.SheetInfo.SequenceMissions_Previous)
                                    {
                                        ImGuiEx.IconWithText(FontAwesomeIcon.ListOl, $"[{mission}] {CosmicHelper.SheetMissionDict[mission].Name}");
                                    }
                                }
                                if (item.SheetInfo.SequenceMissions_Next.Count() > 0)
                                {
                                    ImGui.Separator();
                                    ImGui.Text(T("Next Missions"));
                                    foreach (var mission in item.SheetInfo.SequenceMissions_Next)
                                    {
                                        ImGuiEx.IconWithText(FontAwesomeIcon.ListOl, $"[{mission}] {CosmicHelper.SheetMissionDict[mission].Name}");
                                    }
                                }
                                ImGui.EndTooltip();
                            }
                        }
                    }
                    else
                    {
                        string rank = item.SheetInfo.Rank switch
                        {
                            6 => "M",
                            5 or 4 => "A",
                            3 => "B",
                            2 => "C",
                            1 => "D",
                            _ => "???"
                        };
                        ImGui.Text(rank);
                    }
                }
            }
            public override bool FilterFunc(MissionInfo item)
            {
                var sheetInfo = item.SheetInfo;
                bool special = sheetInfo.IsProvisional || sheetInfo.IsCritical || sheetInfo.IsMaster;

                if (FilterValue.HasFlag(MissionFilter.RedAlert) && sheetInfo.IsCritical) return true;
                if (FilterValue.HasFlag(MissionFilter.Sequence) && sheetInfo.IsSequence) return true;
                if (FilterValue.HasFlag(MissionFilter.Timed) && sheetInfo.IsTimed) return true;
                if (FilterValue.HasFlag(MissionFilter.Weather) && sheetInfo.IsWeather) return true;
                if (FilterValue.HasFlag(MissionFilter.Master) && sheetInfo.IsMaster) return true;
                if (FilterValue.HasFlag(MissionFilter.ARank) && sheetInfo.ARank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.BRank) && sheetInfo.BRank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.CRank) && sheetInfo.CRank && !special) return true;
                if (FilterValue.HasFlag(MissionFilter.DRank) && sheetInfo.Drank && !special) return true;

                return false;
            }
        }
        public sealed class SPMColumn : VerticalCenterColumnString
        {
            public SPMColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            private double GetScore(MissionInfo item)
            {
                var scoreInfo = item.SheetInfo.ScoreInfo();
                if (item.SheetInfo.IsCritical)
                    return scoreInfo[TurninState.Critical].Score;
                if (scoreInfo.TryGetValue(TurninState.SequenceGold, out var seqGold) && seqGold.Score != 0)
                    return seqGold.Score;
                if (item.SheetInfo.IsMaster)
                    return scoreInfo[TurninState.Master_Score].Score;
                return scoreInfo.Values.MaxBy(r => r.Score)?.Score ?? 0;
            }
            private string GetName(TurninState state)
            {
                return state switch
                {
                    TurninState.SequenceGold => "Gold Sequence",
                    TurninState.Master_Score => "Master",
                    _ => state.ToString()
                };
            }

            public override string ToName(MissionInfo item) => $"{GetScore(item):N2}";
            public override int Compare(MissionInfo x, MissionInfo y) => GetScore(x).CompareTo(GetScore(y));
            public override void DrawColumn(MissionInfo item, int _)
            {
                var scoreInfo = item.SheetInfo.ScoreInfo();
                var bestScore = scoreInfo.MaxBy(r => r.Value.Score);
                if (scoreInfo.TryGetValue(TurninState.SequenceGold, out var seqGold) && seqGold.Score != 0)
                    bestScore = new(TurninState.SequenceGold, seqGold);

                if (bestScore.Value.Score != 0)
                {
                    string scoreText = $"{bestScore.Value.Score:N2}";

                    var buttonWidth = ImGui.CalcTextSize(scoreText).X + ImGui.GetStyle().FramePadding.X * 2;
                    var columnWidth = ImGui.GetColumnWidth();
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - buttonWidth) / 2);

                    Vector4 pillColor = bestScore.Key switch
                    {
                        TurninState.Bronze => new(0.6f, 0.35f, 0.15f, 1.0f), // darker bronze
                        TurninState.Silver => new(0.6f, 0.6f, 0.6f, 1.0f),
                        TurninState.Gold => new(0.85f, 0.70f, 0.0f, 1.0f), // slightly muted gold
                        TurninState.Critical => new(0.7f, 0.1f, 0.9f, 1.0f), // purple feels "special"
                        TurninState.SequenceGold => new(0.95f, 0.60f, 0.0f, 1.0f),  // amber-gold, more orange warmth
                        TurninState.Master_Score => new(1.0f, 0.95f, 0.8f, 1.0f), // Radiant White/Gold
                        _ => new(0.5f, 0.5f, 0.5f, 0.8f)
                    };

                    bool isBright = bestScore.Key is TurninState.Gold or TurninState.Silver or TurninState.SequenceGold or TurninState.Master_Score;
                    Vector4 textColor = isBright ? new(0.1f, 0.1f, 0.1f, 1.0f) : new(1.0f, 1.0f, 1.0f, 1.0f);

                    using (ImRaii.PushColor(ImGuiCol.Button, pillColor)
                        .Push(ImGuiCol.ButtonHovered, pillColor with { W = 1.0f })
                        .Push(ImGuiCol.ButtonActive, pillColor)
                        .Push(ImGuiCol.Text, textColor))
                    {
                        if (ImGui.SmallButton($"{scoreText}##SPM_{item.Id}_{item.SheetInfo.Name}"))
                        {
                            P.externalDetails.OpenToStatsTab(item.Id);
                        }
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Click button to view external details"));
                        ImGui.Separator();
                        ImGui.Text(T("[Average] Rewards per minute"));
                        if (C.MissionConfig.TryGetValue(item.Id, out var config))
                        {
                            ImGui.Text(T("Total Completions: {0:N0}/{1:N0}", config.TotalCompletions, config.TotalAttempts));
                        }
                        if (ImGui.BeginTable($"Score Info Table_{item.SheetInfo.MissionId}", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
                        {
                            ImGui.TableSetupColumn(T("Kind"));
                            ImGui.TableSetupColumn(T("Score"));
                            ImGui.TableSetupColumn(T("Credits"));
                            ImGui.TableSetupColumn(T("Planetary"));
                            ImGui.TableSetupColumn(T("Tokens"));

                            ImGui.TableHeadersRow();

                            foreach (var entry in scoreInfo.Where(x => x.Value.Score != 0))
                            {
                                if (item.SheetInfo.IsMaster && entry.Key != TurninState.Master_Score)
                                    continue;
                                if (item.SheetInfo.IsCritical && entry.Key != TurninState.Critical)
                                    continue;

                                ImGui.TableNextRow();
                                ImGui.TableSetColumnIndex(0);
                                ImGui.Text($"{GetName(entry.Key)} [{entry.Value.Completions:N0}]");

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
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.TurninGold, ItemFilter.TurninSilver, ItemFilter.TurninBronze);
                SetNames("Gold", "Silver", "Bronze");
            }
            public override float Width
            {
                get
                {
                    int amount = C.MissionFilter.HasFlag(MissionFilter.Master) ? 4 : 3;

                    var iconWidth = ImGui.GetFrameHeight(); // IconButton is square, frameHeight x frameHeight
                    var spacing = ImGui.GetStyle().ItemSpacing.X;
                    var cellPadding = ImGui.GetStyle().CellPadding.X * 2;

                    var headerWidth = ImGui.CalcTextSize(Label).X + cellPadding;
                    var contentWidth = iconWidth * amount + spacing * 3 + cellPadding; // 4 icons (clock+3 trophies) worst case

                    return Math.Max(headerWidth, contentWidth);
                }
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
                {

                    ImGuiUtil.Center(T("Auto"));
                }
                else if (item.SheetInfo.IsMaster)
                {
                    string masterPopup = "Master Settings: Popup";

                    ImGui.PushID($"Mission_{item.Id}");
                    if (ImGui.Button(T("Master Settings")))
                    {
                        ImGui.OpenPopup(masterPopup);
                    }
                    if (ImGui.BeginPopup(masterPopup))
                    {
                        ImGui.Text($"[{item.Id}] - {item.SheetInfo.Name}");

                        if (C.MissionConfig.TryGetValue(item.Id, out var configInfo))
                        {
                            var selectedMode = configInfo.TurninGoal;
                            var timeExpired = selectedMode == TurninState.TimeExpired;
                            var scoreMode = selectedMode == TurninState.Master_Score;
                            var quickTurnin = selectedMode == TurninState.Gold;
                            var itemTurnin = selectedMode == TurninState.Master_Items;


                            if (ImGui.RadioButton(T("Timed Turnin") + "##TurninGoalRadio", timeExpired))
                            {
                                configInfo.TurninGoal = TurninState.TimeExpired;
                                C.SaveDebounced();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Will turnin once the timer runs out\nCurrently there isn't a way to stop artisan from crafting, it's been requested\nPlease give it time"));
                            }

                            ImGui.Separator();
                            if (ImGui.RadioButton(T("Score Goal") + "##ScoreGoalRadio", scoreMode))
                            {
                                configInfo.TurninGoal = TurninState.Master_Score;
                                C.SaveDebounced();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Will turnin when 1 of the 2 things are met:\n1: Score that you personally have set has been met\n2: Timer has ran out\nYou can set your score with this mode yourself, due to not knowing the scoring break points\nYet"));
                            }
                            ImGui.SameLine();
                            var masterScore = configInfo.Master_Score;
                            ImGui.SetNextItemWidth(150);
                            if (ImGui.InputUInt(T("Score Goal") + "##ScoreGoalInput", ref masterScore))
                            {
                                configInfo.Master_Score = masterScore;
                                C.SaveDebounced();
                            }

                            if (item.SheetInfo.Jobs.ContainsAny(CosmicHelper.CrafterJobList))
                            {
                                ImGui.Separator();
                                if (ImGui.RadioButton(T("After X Crafts"), itemTurnin))
                                {
                                    configInfo.TurninGoal = TurninState.Master_Items;
                                    C.SaveDebounced();
                                }
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip(T("Will turn in after X amount of crafts have been completed\nGood if you're goal is to just craft a certain amount and not worry bout score\nDO NOT. SET THIS TO SOME REDICULOUS AMOUNT AND ASK WHY IT DOESN'T WORK"));
                                }
                                ImGui.SameLine();
                                var itemCount = configInfo.Master_Items;
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputUInt($"##ItemCount_{item.Id}", ref itemCount, 1))
                                {
                                    configInfo.Master_Items = itemCount;
                                    C.SaveDebounced();
                                }
                            }

                            ImGui.Separator();
                            if (ImGui.RadioButton(T("Quick Turnin") + "##QuickTurninRadio", quickTurnin))
                            {
                                configInfo.TurninGoal = TurninState.Gold;
                                C.SaveDebounced();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Will turnin the mission as soon as it can\nVery useful for quick score farming, mount tokens.\nFor BTN/MIN, this will gather the non-collectable item"));
                            }
                        }

                        ImGui.EndPopup();
                    }

                    ImGui.PopID();
                }
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

                        using (ImRaii.PushColor(ImGuiCol.Text, goldEnabled ? GoldColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Gold"))
                            {
                                configInfo.TurninGoal = TurninState.Gold;
                                C.SaveDebounced();
                            }
                        }
                        ImGui.SameLine();
                        using (ImRaii.PushColor(ImGuiCol.Text, silverEnabled ? SilverColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Silver"))
                            {
                                configInfo.TurninGoal = TurninState.Silver;
                                C.SaveDebounced();
                            }

                        }
                        ImGui.SameLine();
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
                Flags = ImGuiTableColumnFlags.NoResize;
                var moons = CosmicMoonRegistry.All;
                SetFlags(moons.Select(m => m.PlanetFilter).ToArray());
                SetNames(moons.Select(m => T(m.DisplayName)).ToArray());
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "X").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );

            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.TerritoryId.CompareTo(rhs.SheetInfo.TerritoryId);
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight - 2);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

                var planetIcon = CosmicMoonRegistry.GetIconResource(item.SheetInfo.TerritoryId);

                var texture = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), planetIcon).GetWrapOrEmpty();
                ImGui.Image(texture.Handle, size);
            }
            public override bool FilterFunc(MissionInfo item) =>
                CosmicMoonRegistry.ItemFilterIncludesTerritory(FilterValue, item.SheetInfo.TerritoryId);
        }
        public sealed class JobColumn : JobFilterColumn
        {
            public JobColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlagsAndNames(JobFilter.CRP, JobFilter.BSM, JobFilter.ARM, JobFilter.GSM,
                                 JobFilter.LTW, JobFilter.WVR, JobFilter.ALC, JobFilter.CUL,
                                 JobFilter.MIN, JobFilter.BTN, JobFilter.FSH);
            }
            public override float Width => Math.Max(ImGui.CalcTextSize(Label + "XX").X + ImGui.GetStyle().CellPadding.X * 2, ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2);
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.Jobs.First().CompareTo(rhs.SheetInfo.Jobs.First());

            public override void DrawColumn(MissionInfo item, int idx)
            {
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);
                var jobs = item.SheetInfo.Jobs;

                var tightSpacing = 2f; // adjust this if I don't like how close/far they are
                var totalWidth = frameHeight * jobs.Count + tightSpacing * (jobs.Count - 1);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - totalWidth) / 2);

                using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(tightSpacing, 0));
                foreach (var job in jobs)
                {
                    var icon = CosmicHelper.ClassInfoDict[job].JobIcon;
                    ImGui.Image(icon.GetWrapOrEmpty().Handle, size);
                    ImGui.SameLine();
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
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.Jobs.First().CompareTo(rhs.SheetInfo.Jobs.First());
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var sheetInfo = item.SheetInfo;
                bool craftProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Craft);
                bool gatherProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Gather);
                bool collectable = sheetInfo.Attributes.HasFlag(MissionAttributes.Collectables) || sheetInfo.Attributes.HasFlag(MissionAttributes.ReducedItems);
                bool fishProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Fish);
                bool master = sheetInfo.IsMaster;

                ImGui.PushID($"Mission: {item.Id}");

                var frameHeight = ImGui.GetFrameHeight();
                var jobIconSize = new Vector2(frameHeight);



                if (sheetInfo.Attributes.HasFlag(MissionAttributes.Craft))
                {
                    var job = item.SheetInfo.Jobs.Where(x => CosmicHelper.CrafterJobList.Contains(x)).First();
                    var icon = CosmicHelper.ClassInfoDict[job].JobIcon;

                    if (ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), T("Craft Recipes"), $"{item.Id}_{item.SheetInfo.Name}", jobIconSize))
                    {
                        ImGui.OpenPopup("Craft Settings: Recipies");
                    }

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
                    ImGui.SameLine();
                }

                if (gatherProfile)
                {
                    var job = item.SheetInfo.Jobs.Where(x => CosmicHelper.GatheringJobList.Contains(x)).First();
                    var icon = CosmicHelper.ClassInfoDict[job].JobIcon;

                    if (!collectable)
                    {
                        string profileName = "???";
                        if (C.MissionConfig.TryGetValue(item.Id, out var config))
                        {
                            if (C.GatherProfiles.TryGetValue(config.GProfileId, out var profileSetting))
                            {
                                profileName = profileSetting.Name;
                            }

                            if (ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), T("Gather Profile"), $"{item.Id}_{item.SheetInfo.Name}", jobIconSize, 0.5f, 2))
                            {
                                ImGui.OpenPopup($"Select Gather Profile");
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Select gathering profile"));
                            }
                            if (ImGui.BeginPopup($"Select Gather Profile"))
                            {
                                ImGui.Text(T("Mission: [{0}] {1}", item.Id, item.SheetInfo.Name));
                                ImGui.Text(T("Currently Selected: {0}", T(profileName)));
                                ImGui.Separator();

                                foreach (var profile in C.GatherProfiles)
                                {
                                    var id = profile.Key;
                                    bool profileSelected = config.GProfileId == id;
                                    ImGui.PushID($"{id}_{profile.Value.Name}");
                                    if (ImGui.RadioButton($"{T(profile.Value.Name)}##{profile.Value.Name}_{id}", profileSelected))
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
                        ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), T("Collectable / Auto"), $"{item.Id}_{item.SheetInfo.Name}", jobIconSize);
                    }
                }
                else if (fishProfile)
                {
                    if (C.MissionConfig.TryGetValue(item.Id, out var config))
                    {
                        var job = item.SheetInfo.Jobs.Where(x => CosmicHelper.GatheringJobList.Contains(x)).First();
                        var icon = CosmicHelper.ClassInfoDict[job].JobIcon;

                        if (ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), T("Fishing Profile"), $"{item.Id}_{item.SheetInfo.Name}", jobIconSize))
                        {
                            ImGui.OpenPopup("Select Fishing Profile");
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
                            ImGuiEx.HelpMarker(T("Having this enabled means it will use the default preset that is included with the plugin for autohook. \nIf you would like to use one that you already have in autohook, you can un-checkmark this and type the name of it below"));
                            using (ImRaii.Disabled(builtInPreset))
                            {
                                string presetName = config.AutoHookPresetName;
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputText(T("Preset Name"), ref presetName))
                                {
                                    config.AutoHookPresetName = presetName;
                                    C.SaveDebounced();
                                }
                                if (ImGui.Button(T("Try and apply above profile")))
                                {
                                    P.AutoHook.SetPreset(presetName);
                                }
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip(T("Allows testing to make sure that you have the preset name\ntyped in correctly. This is *case* specific so"));
                                }
                                ImGui.SameLine();
                                if (ImGui.Button(T("Clear Profile")))
                                {
                                    config.AutoHookPresetName = string.Empty;
                                    C.SaveDebounced();
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
                Flags = ImGuiTableColumnFlags.NoResize;
                SetFlags(ItemFilter.BestSPM, ItemFilter.Sequence, ItemFilter.Unlock, ItemFilter.NoNotes);
                SetNames("Best Score Per Minute", "Sequence", "Needs Unlocked", "No Notes");
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var sheetInfo = item.SheetInfo;
                var HasSPM = sheetInfo.BestSPM.SPM > 0;
                var HasSequence = sheetInfo.SequenceMissions_Next.Count() > 0 || sheetInfo.SequenceMissions_Previous.Count() > 0;
                var HasUnlockable = sheetInfo.MissionUnlock.Count() > 0;

                if (HasSPM)
                {
                    ImGuiEx.Icon(FontAwesomeIcon.Trophy);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Average SPM: {0:N2}", sheetInfo.BestSPM.SPM));
                        ImGui.Text(T(sheetInfo.BestSPM.NoteInfo));
                        ImGui.EndTooltip();
                    }
                }
                if (HasSequence)
                {
                    if (HasSPM)
                        ImGui.SameLine();

                    ImGuiEx.Icon(FontAwesomeIcon.ListOl);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        if (sheetInfo.SequenceMissions_Next.Count() > 0)
                        {
                            ImGui.Text(T("Next Sequence:"));
                            foreach (var mission in sheetInfo.SequenceMissions_Next)
                            {
                                var seqInfo = CosmicHelper.SheetMissionDict[mission];
                                ImGui.Text($"[{mission}] {seqInfo.Name}");
                            }
                        }
                        if (sheetInfo.SequenceMissions_Previous.Count() > 0)
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
                if (HasUnlockable)
                {
                    if (HasSPM || HasSequence)
                    {
                        ImGui.SameLine();
                    }
                    if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex)
                    {
                        var frameHeight = ImGui.GetFrameHeight();
                        var size = new Vector2(frameHeight);
                        if (tex.TryGetWrap(out var wrap, out var exc))
                        {
                            ImGui.Image(wrap.Handle, size, new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
                        }
                    }
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
            public override bool FilterFunc(MissionInfo item)
            {
                var sheetInfo = item.SheetInfo;

                var HasSPM = sheetInfo.BestSPM.SPM > 0;
                var HasSequence = sheetInfo.SequenceMissions_Next.Count() > 0 || sheetInfo.SequenceMissions_Previous.Count() > 0;
                var HasUnlockable = sheetInfo.MissionUnlock.Count() > 0;

                return (HasSPM && FilterValue.HasFlag(ItemFilter.BestSPM))
                    || (HasSequence && FilterValue.HasFlag(ItemFilter.Sequence))
                    || (HasUnlockable && FilterValue.HasFlag(ItemFilter.Unlock))
                    || ((!HasSPM && !HasSequence && !HasUnlockable) && FilterValue.HasFlag(ItemFilter.NoNotes));
            }
        }
        private static ItemFilter TierToFlag(int tier) => tier switch
        {
            1 => ItemFilter.HasI,
            2 => ItemFilter.HasII,
            3 => ItemFilter.HasIII,
            4 => ItemFilter.HasIV,
            5 => ItemFilter.HasV,
            6 => ItemFilter.HasVI,
            7 => ItemFilter.HasVII,
            _ => ItemFilter.HasI
        };
    }
    internal class Completion_Table : Table<MissionInfo>, IDisposable
    {
        public readonly EnabledColumn _enabledColumn = new() { Label = T("Mission Table Enabled") };
        public readonly NameColumn _nameColumn = new() { Label = T("Mission Table Name") };
        public readonly IdColumn _idColumn = new() { Label = T("Mission Table ID") };
        public readonly JobColumn _jobColumn = new() { Label = T("Mission Table Job") };
        public readonly MissionColumn _missionColumn = new() { Label = T("Mission Table Rank") };
        public readonly CompletionColumn _completionColumn = new() { Label = T("Mission Table Status") };
        public readonly TurninColumn _turninColumn = new() { Label = T("Mission Table Turnin Goal") };
        public readonly ProfileColumn _profileColumn = new() { Label = T("Mission Table Profile") };
        public readonly NotesColumn _notesColumn = new() { Label = T("Mission Table Notes") };

        public Completion_Table(List<MissionInfo> itemList) : base("CompletionTable", itemList)
        {
            List<Column<MissionInfo>> headers =
            [
                _enabledColumn, _jobColumn, _missionColumn, _idColumn,
                _completionColumn, _nameColumn, _turninColumn, _profileColumn, _notesColumn
            ];

            this.Headers = [.. headers];
            Sortable = true;
            Flags |= ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;
        }

        public void Dispose()
        {
        }

        public sealed class EnabledColumn : Column<MissionInfo>
        {
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight()
            );
            private readonly Mission_Table _table;
            public EnabledColumn()
            {
                Flags = ImGuiTableColumnFlags.NoHide;
            }

            public override int Compare(MissionInfo lhs, MissionInfo rhs)
                => lhs.Enabled().CompareTo(rhs.Enabled());

            public override void DrawColumn(MissionInfo item, int _)
            {
                ImGui.PushID(item.Id);

                var mission = CosmicHelper.CurrentLunarMission;

                if (mission != 0 && mission == item.Id)
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                }
                else if (CosmicHandler.All_AvailableMissions().Contains(item.Id))
                {
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg1, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                }


                bool disabled = C.SelectedMode == ModeSelect.MissionGoldMode
                             || C.SelectedMode == ModeSelect.LevelMode
                             || (C.SelectedMode == ModeSelect.RelicMode && !C.XPRelicOnlyEnabled);

                if (!disabled)
                {
                    bool enabled = C.MissionConfig[item.Id].Enabled;
                    if (ImGui_Ice.Table_CenterCheckbox("##EnableMission", ref enabled))
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
                        _table.SetFilterDirty();
                    }
                    if (ImGui.IsItemClicked())
                    {
                        Window_ExternalDetails.SelectedMission = item.Id;
                    }
                }
                ImGui.PopID();
            }
        }
        public sealed class JobColumn : Column<MissionInfo>
        {
            public JobColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.Jobs.First().CompareTo(rhs.SheetInfo.Jobs.First());
            public override float Width =>
                Math.Max(
                    ImGui.CalcTextSize("Jobs").X + ImGui.GetStyle().CellPadding.X * 2 + 20f,
                    ImGui.GetFrameHeight()
                );

            public override void DrawColumn(MissionInfo item, int idx)
            {
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);
                var jobs = item.SheetInfo.Jobs;

                var tightSpacing = 2f; // adjust this if I don't like how close/far they are
                var totalWidth = frameHeight * jobs.Count + tightSpacing * (jobs.Count - 1);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - totalWidth) / 2);

                using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(tightSpacing, 0));
                foreach (var job in jobs)
                {
                    var icon = CosmicHelper.ClassInfoDict[job].JobIcon;
                    ImGui.Image(icon.GetWrapOrEmpty().Handle, size);
                    ImGui.SameLine();
                }
            }
        }
        public sealed class MissionColumn : Column<MissionInfo>
        {
            public MissionColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }

            private static int GetMissionPriority(CosmicInfo info)
            {
                if (info.IsCritical) return 10;
                if (info.IsSequence) return 9;
                if (info.IsWeather) return 8;
                if (info.IsTimed) return 7;
                if (info.IsMaster) return 6;
                // 5 = Ex, 4 = A, 3 = B, 2 = C, 1 = D
                return (int)info.Rank;
            }
            public override float Width => ImGui.CalcTextSize("Type").X + ImGui.GetStyle().CellPadding.X * 2 + 10f;

            public override int Compare(MissionInfo lhs, MissionInfo rhs) =>
                GetMissionPriority(lhs.SheetInfo).CompareTo(GetMissionPriority(rhs.SheetInfo));
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

                if (item.SheetInfo.IsCritical || item.SheetInfo.IsWeather)
                {
                    var texture = item.SheetInfo.IsCritical ?
                        Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), "ICE.Resources.Red_Alert.png").GetWrapOrEmpty()
                      : CosmicHelper.WeatherIconDict[item.SheetInfo.Weather].GetWrapOrEmpty();

                    ImGui.Image(texture.Handle, size);
                }
                else
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetStyle().FramePadding.X);
                    if (item.SheetInfo.IsProvisional)
                    {
                        var icon = item.SheetInfo.IsTimed ? FontAwesomeIcon.Clock : FontAwesomeIcon.ListOl;
                        ImGuiEx.Icon(icon);
                        if (item.SheetInfo.IsTimed)
                        {
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Time Slot"));
                                ImGui.Text($"{item.SheetInfo.StartTime:D2}:00 - {item.SheetInfo.EndTime:D2}:00");
                                ImGui.EndTooltip();
                            }
                        }
                        else if (item.SheetInfo.IsSequence)
                        {
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Sequence Missions"));
                                if (item.SheetInfo.SequenceMissions_Previous.Count() > 0)
                                {
                                    ImGui.Separator();
                                    ImGui.Text(T("Previous Missions"));
                                    foreach (var mission in item.SheetInfo.SequenceMissions_Previous)
                                    {
                                        ImGuiEx.IconWithText(FontAwesomeIcon.ListOl, $"[{mission}] {CosmicHelper.SheetMissionDict[mission].Name}");
                                    }
                                }
                                if (item.SheetInfo.SequenceMissions_Next.Count() > 0)
                                {
                                    ImGui.Separator();
                                    ImGui.Text(T("Next Missions"));
                                    foreach (var mission in item.SheetInfo.SequenceMissions_Next)
                                    {
                                        ImGuiEx.IconWithText(FontAwesomeIcon.ListOl, $"[{mission}] {CosmicHelper.SheetMissionDict[mission].Name}");
                                    }
                                }
                                ImGui.EndTooltip();
                            }
                        }
                    }
                    else
                    {
                        string rank = item.SheetInfo.Rank switch
                        {
                            6 => "M",
                            5 or 4 => "A",
                            3 => "B",
                            2 => "C",
                            1 => "D",
                            _ => "???"
                        };
                        ImGui.Text(rank);
                    }
                }
            }
        }
        public sealed class IdColumn : Column<MissionInfo>
        {
            public IdColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }

            public override float Width => Math.Max(
                ImGui.CalcTextSize("ID").X + ImGui.GetStyle().CellPadding.X * 2 + 20f,
                ImGui.CalcTextSize("9999").X + ImGui.GetStyle().CellPadding.X * 2
            );

            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.Id.CompareTo(rhs.Id);

            public override void DrawColumn(MissionInfo item, int _)
            {
                ImGuiUtil.Center($"{item.Id}");
            }
        }
        public sealed class CompletionColumn : Column<MissionInfo>
        {
            public CompletionColumn()
            {

            }
            public override float Width =>
                Math.Max(
                    ImGui.CalcTextSize("Completed").X + ImGui.GetStyle().CellPadding.X * 2 + 20f,
                    ImGui.GetFrameHeight()
                );
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.CompletionStatus.CompareTo(rhs.SheetInfo.CompletionStatus);
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var status = item.SheetInfo.CompletionStatus;
                var frameHeight = ImGui.GetFrameHeight();
                var size = new Vector2(frameHeight);

                var columnWidth = ImGui.GetColumnWidth();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (columnWidth - frameHeight) / 2);

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

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetStyle().FramePadding.X);

                    using (ImRaii.PushFont(UiBuilder.IconFont))
                    using (ImRaii.PushColor(ImGuiCol.Text, color))
                    {
                        ImGuiEx.Icon(icon);
                    }
                }
            }
        }
        public sealed class NameColumn : VerticalCenterColumnString
        {
            public NameColumn()
            {
                Flags = ImGuiTableColumnFlags.NoHide;
            }
            public override string ToName(MissionInfo mission) => mission.SheetInfo.Name;
            public override void DrawColumn(MissionInfo mission, int _)
            {
                if (UnsupportedMissions.Ids.Contains(mission.Id))
                {
                    using (var warningPush = ImRaii.PushColor(ImGuiCol.Text, EColor.Red))
                    {
                        ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("This mission is currently missing stuff to allow it to work. It might be planet locked, or could be just needs mapped out\nI'll get to it when my world gets to it o/"));
                        ImGui.EndTooltip();
                    }
                }
                if (mission.SheetInfo.Jobs.Contains(18) && !GatheringUtil.FishingPreset.ContainsKey(mission.Id))
                {
                    using (var warningPush = ImRaii.PushColor(ImGuiCol.Text, EColor.Yellow))
                    {
                        ImGuiEx.Icon(FontAwesomeIcon.ExclamationTriangle);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("This mission doesn't have a fishing preset yet, this is your warning for this."));
                        ImGui.EndTooltip();
                    }
                }
                if (ImGui.Button(mission.SheetInfo.Name))
                {
                    IceLogging.Verbose("Testing... if this fires off multiple times", "DEBUG TEST");
                    Window_ExternalDetails.SelectedMission = mission.Id;
                    P.externalDetails.IsOpen = true;
                    IceLogging.Verbose($"Collasped condition: {P.externalDetails.CollapsedCondition.ToString()}");
                }
                if (mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Gather) || mission.SheetInfo.Attributes.HasFlag(MissionAttributes.Fish))
                {
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"Flag_{mission.Id}"))
                    {
                        Window_ExternalDetails.SelectedMission = mission.Id;
                        Utils.SetGatheringRing(mission.SheetInfo.TerritoryId, (int)mission.SheetInfo.MapPosition.X, (int)mission.SheetInfo.MapPosition.Y, mission.SheetInfo.Radius, mission.SheetInfo.Name);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"X: {mission.SheetInfo.MapPosition.X:N0}, Z: {mission.SheetInfo.MapPosition.Y:N0}");
                        ImGui.EndTooltip();
                    }
                }
                if (GatheringUtil.CriticalSpots.TryGetValue(mission.SheetInfo.Critical_MapKey, out var criticalInfo))
                {
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.FlagCheckered, $"CriticalFlag_{mission.Id}"))
                    {
                        Utils.SetGatheringRing(mission.SheetInfo.TerritoryId, criticalInfo.X, criticalInfo.Y, criticalInfo.Radius, $"Red Alert: {mission.SheetInfo.Name}", criticalInfo.IconId);
                    }
#if DEBUG
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Critical Route: {0}", mission.SheetInfo.Critical_MapKey));
                        ImGui.Separator();
                        ImGui.Text(T("Map Coordinates: {0} | {1}", criticalInfo.X, criticalInfo.Y));
                        ImGui.Separator();
                        ImGui.Text(T("World Position: {0:N2} | {1:N2} | {2:N2}", criticalInfo.WorldCords.X, criticalInfo.WorldCords.Y, criticalInfo.WorldCords.Z));
                        ImGui.EndTooltip();
                    }
#endif
                }
            }
        }
        public sealed class TurninColumn : Column<MissionInfo>
        {
            public TurninColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width
            {
                get
                {
                    int amount = 4;

                    var iconWidth = ImGui.GetFrameHeight(); // IconButton is square, frameHeight x frameHeight
                    var spacing = ImGui.GetStyle().ItemSpacing.X;
                    var cellPadding = ImGui.GetStyle().CellPadding.X * 2;

                    var headerWidth = ImGui.CalcTextSize(Label).X + cellPadding;
                    var contentWidth = iconWidth * amount + spacing * 3 + cellPadding; // 4 icons (clock+3 trophies) worst case

                    return Math.Max(headerWidth, contentWidth);
                }
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
                {

                    ImGuiUtil.Center(T("Auto"));
                }
                else if (item.SheetInfo.IsMaster)
                {
                    string masterPopup = "Master Settings: Popup";

                    ImGui.PushID($"Mission_{item.Id}");
                    if (ImGui.Button(T("Master Settings")))
                    {
                        ImGui.OpenPopup(masterPopup);
                    }
                    if (ImGui.BeginPopup(masterPopup))
                    {
                        ImGui.Text($"[{item.Id}] - {item.SheetInfo.Name}");

                        if (C.MissionConfig.TryGetValue(item.Id, out var configInfo))
                        {
                            var selectedMode = configInfo.TurninGoal;
                            var timeExpired = selectedMode == TurninState.TimeExpired;
                            var scoreMode = selectedMode == TurninState.Master_Score;
                            var quickTurnin = selectedMode == TurninState.Gold;
                            var itemTurnin = selectedMode == TurninState.Master_Items;


                            if (ImGui.RadioButton(T("Timed Turnin") + "##TurninGoalRadio", timeExpired))
                            {
                                configInfo.TurninGoal = TurninState.TimeExpired;
                                C.SaveDebounced();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Will turnin once the timer runs out\nCurrently there isn't a way to stop artisan from crafting, it's been requested\nPlease give it time"));
                            }

                            ImGui.Separator();
                            if (ImGui.RadioButton(T("Score Goal") + "##ScoreGoalRadio", scoreMode))
                            {
                                configInfo.TurninGoal = TurninState.Master_Score;
                                C.SaveDebounced();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Will turnin when 1 of the 2 things are met:\n1: Score that you personally have set has been met\n2: Timer has ran out\nYou can set your score with this mode yourself, due to not knowing the scoring break points\nYet"));
                            }
                            ImGui.SameLine();
                            var masterScore = configInfo.Master_Score;
                            ImGui.SetNextItemWidth(150);
                            if (ImGui.InputUInt(T("Score Goal") + "##ScoreGoalInput", ref masterScore))
                            {
                                configInfo.Master_Score = masterScore;
                                C.SaveDebounced();
                            }

                            if (item.SheetInfo.Jobs.ContainsAny(CosmicHelper.CrafterJobList))
                            {
                                ImGui.Separator();
                                if (ImGui.RadioButton(T("After X Crafts"), itemTurnin))
                                {
                                    configInfo.TurninGoal = TurninState.Master_Items;
                                    C.SaveDebounced();
                                }
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip(T("Will turn in after X amount of crafts have been completed\nGood if you're goal is to just craft a certain amount and not worry bout score\nDO NOT. SET THIS TO SOME REDICULOUS AMOUNT AND ASK WHY IT DOESN'T WORK"));
                                }
                                ImGui.SameLine();
                                var itemCount = configInfo.Master_Items;
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputUInt($"##ItemCount_{item.Id}", ref itemCount, 1))
                                {
                                    configInfo.Master_Items = itemCount;
                                    C.SaveDebounced();
                                }
                            }

                            ImGui.Separator();
                            if (ImGui.RadioButton(T("Quick Turnin") + "##QuickTurninRadio", quickTurnin))
                            {
                                configInfo.TurninGoal = TurninState.Gold;
                                C.SaveDebounced();
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Will turnin the mission as soon as it can\nVery useful for quick score farming, mount tokens.\nFor BTN/MIN, this will gather the non-collectable item"));
                            }
                        }

                        ImGui.EndPopup();
                    }

                    ImGui.PopID();
                }
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

                        using (ImRaii.PushColor(ImGuiCol.Text, goldEnabled ? GoldColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Gold"))
                            {
                                configInfo.TurninGoal = TurninState.Gold;
                                C.SaveDebounced();
                            }
                        }
                        ImGui.SameLine();
                        using (ImRaii.PushColor(ImGuiCol.Text, silverEnabled ? SilverColor : DisabledColor))
                        {
                            if (ImGuiEx.IconButton(FontAwesomeIcon.Trophy, "##Silver"))
                            {
                                configInfo.TurninGoal = TurninState.Silver;
                                C.SaveDebounced();
                            }

                        }
                        ImGui.SameLine();
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
        public sealed class ProfileColumn : Column<MissionInfo>
        {
            public ProfileColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override int Compare(MissionInfo lhs, MissionInfo rhs) => lhs.SheetInfo.Jobs.First().CompareTo(rhs.SheetInfo.Jobs.First());
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var sheetInfo = item.SheetInfo;
                bool craftProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Craft);
                bool gatherProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Gather);
                bool collectable = sheetInfo.Attributes.HasFlag(MissionAttributes.Collectables) || sheetInfo.Attributes.HasFlag(MissionAttributes.ReducedItems);
                bool fishProfile = sheetInfo.Attributes.HasFlag(MissionAttributes.Fish);
                bool master = sheetInfo.IsMaster;

                ImGui.PushID($"Mission: {item.Id}");

                var frameHeight = ImGui.GetFrameHeight();
                var jobIconSize = new Vector2(frameHeight);



                if (sheetInfo.Attributes.HasFlag(MissionAttributes.Craft))
                {
                    var job = item.SheetInfo.Jobs.Where(x => CosmicHelper.CrafterJobList.Contains(x)).First();
                    var icon = CosmicHelper.ClassInfoDict[job].JobIcon;

                    if (ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), T("Craft Recipes"), $"{item.Id}_{item.SheetInfo.Name}", jobIconSize))
                    {
                        ImGui.OpenPopup("Craft Settings: Recipies");
                    }

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
                    ImGui.SameLine();
                }

                if (gatherProfile)
                {
                    var job = item.SheetInfo.Jobs.Where(x => CosmicHelper.GatheringJobList.Contains(x)).First();
                    var icon = CosmicHelper.ClassInfoDict[job].JobIcon;

                    if (!collectable)
                    {
                        string profileName = "???";
                        if (C.MissionConfig.TryGetValue(item.Id, out var config))
                        {
                            if (C.GatherProfiles.TryGetValue(config.GProfileId, out var profileSetting))
                            {
                                profileName = profileSetting.Name;
                            }

                            if (ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), T("Gather Profile"), $"{item.Id}_{item.SheetInfo.Name}", jobIconSize, 0.5f, 2))
                            {
                                ImGui.OpenPopup($"Select Gather Profile");
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip(T("Select gathering profile"));
                            }
                            if (ImGui.BeginPopup($"Select Gather Profile"))
                            {
                                ImGui.Text(T("Mission: [{0}] {1}", item.Id, item.SheetInfo.Name));
                                ImGui.Text(T("Currently Selected: {0}", T(profileName)));
                                ImGui.Separator();

                                foreach (var profile in C.GatherProfiles)
                                {
                                    var id = profile.Key;
                                    bool profileSelected = config.GProfileId == id;
                                    ImGui.PushID($"{id}_{profile.Value.Name}");
                                    if (ImGui.RadioButton($"{T(profile.Value.Name)}##{profile.Value.Name}_{id}", profileSelected))
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
                        ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), T("Collectable / Auto"), $"{item.Id}_{item.SheetInfo.Name}", jobIconSize);
                    }
                }
                else if (fishProfile)
                {
                    if (C.MissionConfig.TryGetValue(item.Id, out var config))
                    {
                        var job = item.SheetInfo.Jobs.Where(x => CosmicHelper.GatheringJobList.Contains(x)).First();
                        var icon = CosmicHelper.ClassInfoDict[job].JobIcon;

                        if (ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), T("Fishing Profile"), $"{item.Id}_{item.SheetInfo.Name}", jobIconSize))
                        {
                            ImGui.OpenPopup("Select Fishing Profile");
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
                            ImGuiEx.HelpMarker(T("Having this enabled means it will use the default preset that is included with the plugin for autohook. \nIf you would like to use one that you already have in autohook, you can un-checkmark this and type the name of it below"));
                            using (ImRaii.Disabled(builtInPreset))
                            {
                                string presetName = config.AutoHookPresetName;
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.InputText(T("Preset Name"), ref presetName))
                                {
                                    config.AutoHookPresetName = presetName;
                                    C.SaveDebounced();
                                }
                                if (ImGui.Button(T("Try and apply above profile")))
                                {
                                    P.AutoHook.SetPreset(presetName);
                                }
                                if (ImGui.IsItemHovered())
                                {
                                    ImGui.SetTooltip(T("Allows testing to make sure that you have the preset name\ntyped in correctly. This is *case* specific so"));
                                }
                                ImGui.SameLine();
                                if (ImGui.Button(T("Clear Profile")))
                                {
                                    config.AutoHookPresetName = string.Empty;
                                    C.SaveDebounced();
                                }
                            }

                            ImGui.EndPopup();
                        }
                    }
                }
            }
        }
        public sealed class NotesColumn : Column<MissionInfo>
        {
            public NotesColumn()
            {
                Flags = ImGuiTableColumnFlags.NoResize;
            }
            public override float Width => Math.Max(
                ImGui.CalcTextSize(Label + "xxx").X + ImGui.GetStyle().CellPadding.X * 2,
                ImGui.GetFrameHeight() + ImGui.GetStyle().CellPadding.X * 2
            );
            public override void DrawColumn(MissionInfo item, int idx)
            {
                var sheetInfo = item.SheetInfo;
                var HasSPM = sheetInfo.BestSPM.SPM > 0;
                var HasSequence = sheetInfo.SequenceMissions_Next.Count() > 0 || sheetInfo.SequenceMissions_Previous.Count() > 0;
                var HasUnlockable = sheetInfo.MissionUnlock.Count() > 0;

                if (HasSPM)
                {
                    ImGuiEx.Icon(FontAwesomeIcon.Trophy);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(T("Average SPM: {0:N2}", sheetInfo.BestSPM.SPM));
                        ImGui.Text(T(sheetInfo.BestSPM.NoteInfo));
                        ImGui.EndTooltip();
                    }
                }
                if (HasSequence)
                {
                    if (HasSPM)
                        ImGui.SameLine();

                    ImGuiEx.Icon(FontAwesomeIcon.ListOl);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        if (sheetInfo.SequenceMissions_Next.Count() > 0)
                        {
                            ImGui.Text(T("Next Sequence:"));
                            foreach (var mission in sheetInfo.SequenceMissions_Next)
                            {
                                var seqInfo = CosmicHelper.SheetMissionDict[mission];
                                ImGui.Text($"[{mission}] {seqInfo.Name}");
                            }
                        }
                        if (sheetInfo.SequenceMissions_Previous.Count() > 0)
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
                if (HasUnlockable)
                {
                    if (HasSPM || HasSequence)
                    {
                        ImGui.SameLine();
                    }
                    if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex)
                    {
                        var frameHeight = ImGui.GetFrameHeight();
                        var size = new Vector2(frameHeight);
                        if (tex.TryGetWrap(out var wrap, out var exc))
                        {
                            ImGui.Image(wrap.Handle, size, new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
                        }
                    }
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
        }
    }
}
