using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Collections.Generic;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings.Settings_Misc;

public static partial class SettingsUi
{
    public sealed class SettingEntry
    {
        public required string Label;
        public required string Category;
        public string[] Keywords = Array.Empty<string>();
        public required Action Draw;

        public string SearchHaystack => _haystack ??= string.Join(' ', new[] { Label, Category }.Concat(Keywords)).ToLowerInvariant();
        private string? _haystack;
    }

    public static List<SettingEntry> BuildRegistry() => new()
    {
        // Overlay Window
        Overlay_AutoOpen,
        Overlay_CogsIcon,
        Overlay_ShowSeconds,
        Overlay_ExpBars,
        Overlay_Scores,
        Overlay_AutoResize,
        Overlay_HighlightWeather,
        Overlay_WeatherSelected,
        Overlay_JobFilter,
        Overlay_HudClipping,

        // Auto-Use
        AutoUse_RemoveStellar,
        AutoUse_StartOnMoon,

        // Post Mission Settings
        Post_GoldRemover,

        // Safety Settings
        Safety_MissionDelay,
        Safety_CraftDelay,
        Safety_RelicDelay,
        Safety_GatherDelay,
        Safety_CloseReward,

        // Record Settings
        Record_TimeHistory,

        // Post Mission Commands
        Post_Commands,

        // Notification Master Settings
        Notification_Flash,
        Notification_Toast,
        Notification_Foreground,

        // Dev Favorites
        Fun_CrazyTaxi,
        Fun_Placebo,
        Fun_FakeFisher,

#if DEBUG
        // Debug
        Debug_Forecast,
        Debug_GatherInfo,
        Debug_HighlightMissions,
        Debug_OnlyGrab,
#endif
    };

    private static readonly Dictionary<string, FontAwesomeIcon> CategoryIcons = new()
    {
        [OverlayCategory] = FontAwesomeIcon.WindowMaximize,
        [AutoUseCategory] = FontAwesomeIcon.PersonRays,
        [PostMissionCategory] = FontAwesomeIcon.Medal,
        [SafetyCategory] = FontAwesomeIcon.ExclamationTriangle,
        [RecordCategory] = FontAwesomeIcon.Clock,
        [PostCommandsCategory] = FontAwesomeIcon.Play,
        [FunCategory] = FontAwesomeIcon.Heart,
        [NotificationCategory] = FontAwesomeIcon.AlarmClock,
#if DEBUG
        [DebugCategory] = FontAwesomeIcon.Bug,
#endif
    };

    private static List<SettingEntry>? _allSettings;
    private static List<SettingEntry> AllSettings => _allSettings ??= BuildRegistry();

    private static readonly Dictionary<string, List<SettingEntry>> _categoryCache = new();

    private static string _searchQuery = string.Empty;
    private static List<SettingEntry> _filtered = new();
    private static bool _filterDirty = true;

    public static void Draw()
    {
        DrawSearchBar();
        ImGui.Separator();
        using (var child = ImRaii.Child("Settings Ui: Window", default, true))
        {
            DrawEntries();
        }
    }

    // I need a way to keep the old way of viewing the overlay but still allowing for it to be here
    // Honestly, this is just good to have so I can quickly grab sections of blocks for other areas... hmm things to consider
    public static void OverlaySettings() => DrawCategory(OverlayCategory);
    public static void DrawCategory(string category)
    {
        if (!_categoryCache.TryGetValue(category, out var entries))
        {
            entries = AllSettings.Where(s => s.Category == category).ToList();
            _categoryCache[category] = entries;
        }

        foreach (var entry in entries)
            entry.Draw();
    }

    private static void DrawEntries()
    {
        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            DrawGrouped(AllSettings);
            return;
        }

        if (_filterDirty)
            Refilter();

        if (_filtered.Count == 0)
        {
            ImGui.TextDisabled(T("No settings match \"{0}\".", _searchQuery));
            return;
        }

        DrawGrouped(_filtered);
    }

    private static void DrawSearchBar()
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputTextWithHint("##settings-search", T("Search settings..."), ref _searchQuery, 128))
            _filterDirty = true;

        if (string.IsNullOrEmpty(_searchQuery))
            return;

        ImGui.SameLine();
        if (ImGui.SmallButton(T("Clear")))
        {
            _searchQuery = string.Empty;
            _filterDirty = true;
        }
    }

    private static void Refilter()
    {
        var needle = _searchQuery.Trim().ToLowerInvariant();
        _filtered = AllSettings
            .Where(s => s.SearchHaystack.Contains(needle, StringComparison.Ordinal))
            .ToList();
        _filterDirty = false;
    }

    private static void DrawGrouped(List<SettingEntry> settings)
    {
        foreach (var group in settings.GroupBy(s => s.Category))
        {
            if (!DrawCategoryHeader(group.Key))
                continue;

            ImGui.Indent();
            foreach (var entry in group)
                entry.Draw();
            ImGui.Unindent();
            ImGui.Spacing();
        }
    }

    /// <summary>
    /// CollapsingHeader with an icon + label. The header itself gets an invisible label (ID only),
    /// and the icon/text are painted over it with the draw list, same trick as ECommons' IconButtonWithText.
    /// It looks really nice, and keeps the astetic I was aiming for 
    /// </summary>
    private static bool DrawCategoryHeader(string category)
    {
        var open = ImGui.CollapsingHeader($"##header-{category}", ImGuiTreeNodeFlags.DefaultOpen);

        var rectMin = ImGui.GetItemRectMin();
        var rectHeight = ImGui.GetItemRectMax().Y - rectMin.Y;
        var drawList = ImGui.GetWindowDrawList();
        var color = ImGui.GetColorU32(ImGuiCol.Text);

        // Skip past the tree arrow.
        var x = rectMin.X + ImGui.GetTreeNodeToLabelSpacing();

        if (CategoryIcons.TryGetValue(category, out var icon))
        {
            var iconString = icon.ToIconString();
            Vector2 iconSize;
            using (ImRaii.PushFont(UiBuilder.IconFont))
            {
                iconSize = ImGui.CalcTextSize(iconString);
                drawList.AddText(new Vector2(x, rectMin.Y + (rectHeight - iconSize.Y) / 2f), color, iconString);
            }

            x += iconSize.X + 4 * ImGuiHelpers.GlobalScale;
        }

        var categoryLabel = T(category);
        var textSize = ImGui.CalcTextSize(categoryLabel);
        drawList.AddText(new Vector2(x, rectMin.Y + (rectHeight - textSize.Y) / 2f), color, categoryLabel);

        return open;
    }

    /// <summary>
    /// Just an easy way for me to wire up all the checkboxes... since a lot of these are just simple
    /// </summary>
    private static SettingEntry Toggle(string label, string category, string[] keywords, Func<bool> get, Action<bool> set, string? tooltip = null)
    {
        return new SettingEntry
        {
            Label = T(label),
            Category = category,
            Keywords = keywords,
            Draw = () =>
            {
                var value = get();
                var changed = ImGui.Checkbox(T(label), ref value);

                if (tooltip != null && ImGui.IsItemHovered())
                    ImGui.SetTooltip(tooltip);

                if (!changed)
                    return;

                set(value);
                C.Save();
            }
        };
    }
}
