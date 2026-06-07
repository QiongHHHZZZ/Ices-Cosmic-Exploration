using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.ModeSelect_Modes.CosmicTable;

internal static class Table
{
    public const float ArrowWidth = 10;
}

internal static class ImGuiUtil
{
    private static float _currentColumnWidth;
    private static Vector2 _defaultPopupWindowPadding = new(8f, 8f);
    private static Vector2 _defaultPopupItemSpacing = new(8f, 4f);

    public static float CurrentColumnWidth => _currentColumnWidth > 0
        ? _currentColumnWidth
        : ImGui.GetColumnWidth();

    public static IDisposable PushColumnWidth(float width)
        => new ColumnWidthScope(width);

    public static void SetDefaultPopupStyle(Vector2 windowPadding, Vector2 itemSpacing)
    {
        _defaultPopupWindowPadding = windowPadding;
        _defaultPopupItemSpacing = itemSpacing;
    }

    public static IDisposable PushDefaultPopupStyle()
        => ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, _defaultPopupWindowPadding)
            .Push(ImGuiStyleVar.ItemSpacing, _defaultPopupItemSpacing);

    public static void Center(string text)
    {
        var textWidth = ImGui.CalcTextSize(text).X;
        var columnWidth = CurrentColumnWidth;
        var offset = MathF.Max(0, (columnWidth - textWidth) * 0.5f);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.TextUnformatted(text);
    }

    private sealed class ColumnWidthScope : IDisposable
    {
        private readonly float _previous;

        public ColumnWidthScope(float width)
        {
            _previous = _currentColumnWidth;
            _currentColumnWidth = width;
        }

        public void Dispose()
            => _currentColumnWidth = _previous;
    }
}

internal class Column<TItem>
{
    public string Label = string.Empty;
    public ImGuiTableColumnFlags Flags = ImGuiTableColumnFlags.NoResize;
    public bool IsVisible = true;

    public virtual float Width => -1f;

    public string FilterLabel => $"##{Label}Filter";

    public virtual bool DrawFilter()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(Label);
        return false;
    }

    public virtual bool FilterFunc(TItem item) => true;

    public virtual int Compare(TItem lhs, TItem rhs) => 0;

    public virtual int SortKeyCount => 1;

    public virtual int CompareSortKey(TItem lhs, TItem rhs, int sortKey) => Compare(lhs, rhs);

    public virtual void PreDraw()
    { }

    public virtual void DrawColumn(TItem item, int idx)
    { }

    public virtual void PreSort()
    { }

    public virtual void PostSort()
    { }

    public int CompareInv(TItem lhs, TItem rhs) => Compare(rhs, lhs);

    public int CompareInvSortKey(TItem lhs, TItem rhs, int sortKey) => CompareSortKey(rhs, lhs, sortKey);
}

internal class ColumnString<TItem> : Column<TItem>
{
    public ColumnString()
        => Flags &= ~ImGuiTableColumnFlags.NoResize;

    public string FilterValue = string.Empty;
    protected Regex? FilterRegex;

    public virtual string ToName(TItem item)
        => item?.ToString() ?? string.Empty;

    public override int Compare(TItem lhs, TItem rhs)
        => string.Compare(ToName(lhs), ToName(rhs), StringComparison.InvariantCulture);

    public override bool DrawFilter()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
        try
        {
            ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
            var tmp = FilterValue;
            if (!ImGui.InputTextWithHint(FilterLabel, Label, ref tmp, 256) || tmp == FilterValue)
                return false;

            FilterValue = tmp;
            try
            {
                FilterRegex = new Regex(FilterValue, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            catch
            {
                FilterRegex = null;
            }

            return true;
        }
        finally
        {
            ImGui.PopStyleVar();
        }
    }

    public override bool FilterFunc(TItem item)
    {
        if (FilterValue.Length == 0)
            return true;

        var name = ToName(item);
        return FilterRegex?.IsMatch(name) ?? name.Contains(FilterValue, StringComparison.OrdinalIgnoreCase);
    }

    public override void DrawColumn(TItem item, int idx)
    {
        PreDraw();
        ImGui.TextUnformatted(ToName(item));
    }
}

internal class ColumnFlags<T, TItem> : Column<TItem> where T : struct, Enum
{
    public T AllFlags = default;
    protected ImGuiComboFlags ComboFlags = ImGuiComboFlags.NoArrowButton;

    protected virtual IReadOnlyList<T> Values => Enum.GetValues<T>();
    protected virtual string[] Names => Enum.GetNames<T>();

    public virtual T FilterValue => default;

    protected virtual void SetValue(T value, bool enable)
    { }

    protected virtual bool DrawCheckbox(int idx, out bool ret)
    {
        ret = FilterValue.HasFlag(Values[idx]);
        return ImGui.Checkbox(Names[idx], ref ret);
    }

    public override bool DrawFilter()
    {
        ImGui.PushID(FilterLabel);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
        try
        {
            ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
            var all = FilterValue.HasFlag(AllFlags);
            if (!all)
                ImGui.PushStyleColor(ImGuiCol.FrameBg, 0x803030A0);

            var comboOpen = ImGui.BeginCombo("", Label, ComboFlags);
            if (!all)
                ImGui.PopStyleColor();

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                SetValue(AllFlags, true);
                if (comboOpen)
                    ImGui.EndCombo();
                return true;
            }

            if (!all && ImGui.IsItemHovered())
                ImGui.SetTooltip(T("Right-click to clear filters."));

            if (!comboOpen)
                return false;

            var changed = false;
            if (ImGui.Checkbox(T("Enable All"), ref all))
            {
                SetValue(AllFlags, all);
                changed = true;
            }

            ImGui.Indent(10f);
            try
            {
                for (var i = 0; i < Names.Length; ++i)
                {
                    if (!DrawCheckbox(i, out var tmp))
                        continue;

                    SetValue(Values[i], tmp);
                    changed = true;
                }
            }
            finally
            {
                ImGui.Unindent(10f);
                ImGui.EndCombo();
            }

            return changed;
        }
        finally
        {
            ImGui.PopStyleVar();
            ImGui.PopID();
        }
    }
}

internal class Table<TItem>
{
    protected bool FilterDirty = true;
    protected bool SortDirty = true;
    protected readonly IReadOnlyCollection<TItem> Items;
    public List<(TItem Item, int Index)> FilteredItems;

    protected readonly string Label;
    public Column<TItem>[] Headers;

    protected float ItemHeight { get; set; }
    public float ExtraHeight { get; set; }

    private int _currentIdx;
    private bool _allowHorizontalScroll;
    private bool _resetScrollY;

    protected bool Sortable
    {
        get => Flags.HasFlag(ImGuiTableFlags.Sortable);
        set => Flags = value ? Flags | ImGuiTableFlags.Sortable : Flags & ~ImGuiTableFlags.Sortable;
    }

    protected int SortIdx = -1;
    private int _sortKeyIdx;
    private bool _sortAscending = true;
    private float _tableScrollX;
    private readonly List<int> _visibleColumnBuffer = new();
    private readonly List<float> _columnWidthBuffer = new();
    private static readonly Dictionary<string, (bool Matches, string Group, string SubLabel)> GroupedHeaderPartsCache = new();
    private static readonly Dictionary<string, string[]> HeaderSubLabelsCache = new();

    public ImGuiTableFlags Flags = ImGuiTableFlags.RowBg
        | ImGuiTableFlags.Sortable
        | ImGuiTableFlags.BordersOuter
        | ImGuiTableFlags.ScrollY
        | ImGuiTableFlags.ScrollX
        | ImGuiTableFlags.PreciseWidths
        | ImGuiTableFlags.SizingFixedFit
        | ImGuiTableFlags.BordersInnerV;

    public int TotalItems => Items.Count;
    public int CurrentItems => FilteredItems.Count;
    public int TotalColumns => Headers.Length;
    public int VisibleColumns { get; private set; }

    public Table(string label, IReadOnlyCollection<TItem> items, params Column<TItem>[] headers)
    {
        Label = label;
        Items = items;
        Headers = headers;
        FilteredItems = new List<(TItem, int)>(Items.Count);
        VisibleColumns = Headers.Length;
    }

    public void Draw(float itemHeight)
    {
        var scale = ImGuiHelpers.GlobalScale;
        ItemHeight = MathF.Max(itemHeight, ImGui.GetFrameHeight() + 2f * scale);
        ImGui.PushID(Label);
        try
        {
            UpdateFilter();
            DrawTableInternal();
        }
        finally
        {
            ImGui.PopID();
        }
    }

    public virtual bool WouldBeVisible(TItem value)
    {
        for (var i = 0; i < Headers.Length; i++)
        {
            if (!Headers[i].FilterFunc(value))
                return false;
        }

        return true;
    }

    protected virtual void DrawFilters()
        => throw new NotImplementedException();

    protected virtual void PreDraw()
    { }

    public void SetFilterDirty(bool resetScroll = true)
    {
        FilterDirty = true;
        _resetScrollY |= resetScroll;
    }

    private void SortInternal()
    {
        if (!Sortable)
            return;

        if (!SortDirty)
            return;

        if (SortIdx < 0 || Headers.Length <= SortIdx)
        {
            SortDirty = false;
            return;
        }

        var header = Headers[SortIdx];
        header.PreSort();
        try
        {
            if (_sortAscending)
            {
                FilteredItems.Sort((lhs, rhs) =>
                {
                    var result = header.CompareSortKey(lhs.Item, rhs.Item, _sortKeyIdx);
                    return result != 0 ? result : lhs.Index.CompareTo(rhs.Index);
                });
            }
            else
            {
                FilteredItems.Sort((lhs, rhs) =>
                {
                    var result = header.CompareInvSortKey(lhs.Item, rhs.Item, _sortKeyIdx);
                    return result != 0 ? result : lhs.Index.CompareTo(rhs.Index);
                });
            }
        }
        finally
        {
            header.PostSort();
        }

        SortDirty = false;
    }

    private void UpdateFilter()
    {
        if (!FilterDirty)
            return;

        FilteredItems.Clear();
        var idx = 0;
        foreach (var item in Items)
        {
            if (WouldBeVisible(item))
                FilteredItems.Add((item, idx));
            idx++;
        }

        FilterDirty = false;
        SortDirty = true;
    }

    private void DrawItem((TItem Item, int Index) pair, IReadOnlyList<int> visibleColumns, IReadOnlyList<float> columnWidths, float rowHeight, float totalWidth, float drawWidth, float contentWidth, int rowNumber)
    {
        var drawList = ImGui.GetWindowDrawList();
        var rowMin = ImGui.GetCursorScreenPos();
        var rowMax = rowMin + new Vector2(drawWidth, rowHeight);

        var hovered = ImGui.IsMouseHoveringRect(rowMin, rowMax)
                   && ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByPopup);

        var rowColor = GetRowBackgroundColor(pair.Item, pair.Index, rowNumber, hovered);
        drawList.AddRectFilled(rowMin, rowMax, rowColor);
        drawList.AddLine(new Vector2(rowMin.X, rowMax.Y), rowMax, GetGridColor());

        ImGui.PushID(pair.Index);
        try
        {
            _currentIdx = pair.Index;
            var x = 0f;
            for (var visibleIndex = 0; visibleIndex < visibleColumns.Count; visibleIndex++)
            {
                var column = visibleColumns[visibleIndex];
                var width = columnWidths[visibleIndex];
                var cellMin = rowMin + new Vector2(x, 0);
                var cellMax = cellMin + new Vector2(width, rowHeight);

                drawList.AddLine(new Vector2(cellMax.X, cellMin.Y), cellMax, GetGridColor());

                ImGui.PushID(column);
                try
                {
                    var header = Headers[column];
                    ImGui.PushClipRect(cellMin, cellMax, true);
                    try
                    {
                        var padding = GetCellPadding();
                        var yOffset = MathF.Max(0, (rowHeight - ImGui.GetFrameHeight()) * 0.5f);
                        ImGui.SetCursorScreenPos(cellMin + new Vector2(padding.X, yOffset));
                        using (ImGuiUtil.PushColumnWidth(MathF.Max(1, width - padding.X * 2)))
                            header.DrawColumn(pair.Item, pair.Index);
                    }
                    finally
                    {
                        ImGui.PopClipRect();
                    }
                }
                finally
                {
                    ImGui.PopID();
                }

                x += width;
            }
        }
        finally
        {
            ImGui.PopID();
        }

        ImGui.SetCursorScreenPos(rowMin + new Vector2(0, rowHeight));
        ImGui.Dummy(new Vector2(contentWidth, 0.1f));
    }

    private void DrawEmptyRow(float rowHeight, float drawWidth, float contentWidth, int rowNumber)
    {
        var drawList = ImGui.GetWindowDrawList();
        var rowMin = ImGui.GetCursorScreenPos();
        var rowMax = rowMin + new Vector2(drawWidth, rowHeight);
        var rowColor = rowNumber % 2 == 0
            ? ImGui.GetColorU32(new Vector4(0.07f, 0.10f, 0.16f, 0.92f))
            : ImGui.GetColorU32(new Vector4(0.09f, 0.13f, 0.20f, 0.92f));

        drawList.AddRectFilled(rowMin, rowMax, rowColor);
        drawList.AddLine(new Vector2(rowMin.X, rowMax.Y), rowMax, GetGridColor());
        ImGui.SetCursorScreenPos(rowMin + new Vector2(0, rowHeight));
        ImGui.Dummy(new Vector2(contentWidth, 0.1f));
    }

    private void DrawTableInternal()
    {
        var size = ImGui.GetContentRegionAvail() - ExtraHeight * Vector2.UnitY * ImGuiHelpers.GlobalScale;
        if (size.X <= 0 || size.Y <= 0)
            return;

        PreDraw();

        var visibleColumns = GetVisibleColumnIndices();
        VisibleColumns = visibleColumns.Count;
        if (VisibleColumns == 0)
        {
            ImGui.TextDisabled(T("No columns selected."));
            return;
        }

        var columnWidths = GetColumnWidths(visibleColumns, out var totalWidth);
        var headerHeight = GetHeaderHeight();

        var scale = ImGuiHelpers.GlobalScale;
        var tableWidth = size.X;
        var bodyHeight = MathF.Max(0, size.Y - headerHeight - 2f * scale);
        var hasVerticalScrollbar = FilteredItems.Count * ItemHeight > bodyHeight + 0.5f;
        // Only reserve the vertical scrollbar gutter when it will actually be
        // shown.  Always reserving it creates a fake blank column; never
        // reserving it lets the rightmost cells render underneath the scrollbar.
        var scrollbarGutter = hasVerticalScrollbar ? ImGui.GetStyle().ScrollbarSize : 0f;
        var layoutWidth = MathF.Max(1f, tableWidth - scrollbarGutter);
        var stretchIndex = GetStretchColumnIndex(visibleColumns);
        if (stretchIndex >= 0 && MathF.Abs(totalWidth - layoutWidth) > 0.5f)
        {
            var minStretchWidth = GetMinimumStretchColumnWidth(Headers[visibleColumns[stretchIndex]], visibleColumns[stretchIndex]);
            var targetStretchWidth = MathF.Max(minStretchWidth, columnWidths[stretchIndex] + layoutWidth - totalWidth);
            totalWidth += targetStretchWidth - columnWidths[stretchIndex];
            columnWidths[stretchIndex] = targetStretchWidth;
        }

        if (totalWidth > layoutWidth)
        {
            var overflow = totalWidth - layoutWidth;
            if (stretchIndex >= 0)
                ShrinkColumn(stretchIndex, visibleColumns, columnWidths, ref overflow);

            for (var i = 0; i < columnWidths.Count && overflow > 0.5f; i++)
            {
                if (i != stretchIndex)
                    ShrinkColumn(i, visibleColumns, columnWidths, ref overflow);
            }

            totalWidth = 0f;
            for (var i = 0; i < columnWidths.Count; i++)
                totalWidth += columnWidths[i];
        }

        _allowHorizontalScroll = totalWidth > layoutWidth + 0.5f;

        var defaultWindowPadding = ImGui.GetStyle().WindowPadding;
        var defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        ImGuiUtil.SetDefaultPopupStyle(defaultWindowPadding, defaultItemSpacing);

        using var outerStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero)
            .Push(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.ChildRounding, 8f * scale)
            .Push(ImGuiStyleVar.ChildBorderSize, 1f * scale);
        if (!ImGui.BeginChild("##customTableFrame", new Vector2(tableWidth, size.Y), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            ImGui.EndChild();
            return;
        }

        try
        {
            DrawHeader(visibleColumns, columnWidths, totalWidth, headerHeight);
            ImGui.Dummy(new Vector2(0, 2f * scale));
            SortInternal();
            DrawBody(visibleColumns, columnWidths, totalWidth, headerHeight);
        }
        finally
        {
            ImGui.EndChild();
        }
    }

    private List<int> GetVisibleColumnIndices()
    {
        var visible = _visibleColumnBuffer;
        visible.Clear();
        for (var i = 0; i < Headers.Length; i++)
        {
            if (Headers[i].IsVisible && !Headers[i].Flags.HasFlag(ImGuiTableColumnFlags.Disabled))
                visible.Add(i);
        }

        return visible;
    }

    private List<float> GetColumnWidths(IReadOnlyList<int> visibleColumns, out float totalWidth)
    {
        var columnWidths = _columnWidthBuffer;
        columnWidths.Clear();
        totalWidth = 0f;

        for (var i = 0; i < visibleColumns.Count; i++)
        {
            var columnIndex = visibleColumns[i];
            var width = GetColumnWidth(Headers[columnIndex], columnIndex);
            columnWidths.Add(width);
            totalWidth += width;
        }

        return columnWidths;
    }

    private void ShrinkColumn(int widthIndex, IReadOnlyList<int> visibleColumns, IList<float> columnWidths, ref float overflow)
    {
        var columnIndex = visibleColumns[widthIndex];
        var minWidth = MathF.Min(columnWidths[widthIndex], GetMinimumColumnWidth(Headers[columnIndex], columnIndex));
        var reducible = MathF.Max(0, columnWidths[widthIndex] - minWidth);
        if (reducible <= 0)
            return;

        var reduction = MathF.Min(reducible, overflow);
        columnWidths[widthIndex] -= reduction;
        overflow -= reduction;
    }

    protected virtual int GetStretchColumnIndex(IReadOnlyList<int> visibleColumns)
        => -1;

    protected virtual float GetMinimumStretchColumnWidth(Column<TItem> header, int index)
        => 160f * ImGuiHelpers.GlobalScale;

    protected virtual float GetMinimumColumnWidth(Column<TItem> header, int index)
        => 28f * ImGuiHelpers.GlobalScale;

    private void DrawHeader(IReadOnlyList<int> visibleColumns, IReadOnlyList<float> columnWidths, float totalWidth, float headerHeight)
    {
        if (!ImGui.BeginChild("##customTableHeader", new Vector2(0, headerHeight), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            ImGui.EndChild();
            return;
        }

        try
        {
            var visibleOrigin = ImGui.GetCursorScreenPos();
            var origin = visibleOrigin - new Vector2(_tableScrollX, 0);
            var drawList = ImGui.GetWindowDrawList();
            var x = 0f;
            var fillWidth = MathF.Max(totalWidth, _tableScrollX + ImGui.GetWindowWidth());

            drawList.AddRectFilled(origin, origin + new Vector2(fillWidth, headerHeight), GetHeaderColor(), GetFrameRounding(), ImDrawFlags.RoundCornersTop);
            drawList.AddLine(origin + new Vector2(0, headerHeight - 1), origin + new Vector2(fillWidth, headerHeight - 1), GetBorderColor(), 1.2f * ImGuiHelpers.GlobalScale);

            for (var visibleIndex = 0; visibleIndex < visibleColumns.Count; visibleIndex++)
            {
                var column = visibleColumns[visibleIndex];
                var width = columnWidths[visibleIndex];
                var nextColumn = visibleIndex + 1 < visibleColumns.Count ? visibleColumns[visibleIndex + 1] : -1;
                var borderStartsAtSubRow = nextColumn >= 0 && ShareGroupedHeader(Headers[column].Label, Headers[nextColumn].Label);
                DrawHeaderCell(column, Headers[column], origin + new Vector2(x, 0), width, headerHeight, borderStartsAtSubRow);
                x += width;
            }

            DrawGroupedHeaderTitles(visibleColumns, columnWidths, origin, headerHeight);

            ImGui.SetCursorScreenPos(visibleOrigin);
            ImGui.Dummy(new Vector2(MathF.Max(totalWidth, ImGui.GetWindowWidth()), headerHeight));
        }
        finally
        {
            ImGui.EndChild();
        }
    }

    private void DrawGroupedHeaderTitles(IReadOnlyList<int> visibleColumns, IReadOnlyList<float> columnWidths, Vector2 origin, float headerHeight)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var drawList = ImGui.GetWindowDrawList();
        var x = 0f;
        var i = 0;

        while (i < visibleColumns.Count)
        {
            var column = visibleColumns[i];
            if (!TryGetGroupedHeaderParts(Headers[column].Label, out var group, out _))
            {
                x += columnWidths[i];
                i++;
                continue;
            }

            var groupStartX = x;
            var groupWidth = columnWidths[i];
            var j = i + 1;
            while (j < visibleColumns.Count
                && TryGetGroupedHeaderParts(Headers[visibleColumns[j]].Label, out var nextGroup, out _)
                && nextGroup == group)
            {
                groupWidth += columnWidths[j];
                j++;
            }

            var minX = origin.X + groupStartX;
            var maxX = minX + groupWidth;
            var separatorY = GetGroupedHeaderSeparatorY(origin.Y, headerHeight);
            drawList.AddLine(
                new Vector2(minX + scale, separatorY),
                new Vector2(maxX - scale, separatorY),
                GetHeaderSubRowSeparatorColor(),
                1.2f * scale);

            DrawCenteredHeaderText(group, minX, origin.Y + 5f * scale, groupWidth, GetHeaderGroupTextColor());

            x += groupWidth;
            i = j;
        }
    }

    private void DrawHeaderCell(int column, Column<TItem> header, Vector2 cellMin, float width, float headerHeight, bool borderStartsAtSubRow)
    {
        var cellMax = cellMin + new Vector2(width, headerHeight);
        var drawList = ImGui.GetWindowDrawList();
        var hitMin = GetHeaderHitMin(header.Label, cellMin, headerHeight);
        var hitMax = cellMax;
        var hovered = Sortable && ImGui.IsMouseHoveringRect(hitMin, hitMax)
                   && ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByPopup);

        if (hovered)
        {
            var rounding = GetFrameRounding();
            var flags = hitMin.Y <= cellMin.Y && column == 0
                ? ImDrawFlags.RoundCornersTopLeft
                : ImDrawFlags.RoundCornersNone;
            drawList.AddRectFilled(hitMin, hitMax, GetHeaderHoverColor(), rounding, flags);
        }
        var borderStartY = borderStartsAtSubRow
            ? GetGroupedHeaderSeparatorY(cellMin.Y, headerHeight)
            : cellMin.Y;
        drawList.AddLine(new Vector2(cellMax.X, borderStartY), cellMax, GetGridColor());

        ImGui.PushID(column);
        try
        {
            ImGui.PushClipRect(cellMin, cellMax, true);
            try
            {
                DrawHeaderLabel(header.Label, cellMin, width, headerHeight, SortIdx == column ? _sortKeyIdx : -1, _sortAscending);
            }
            finally
            {
                ImGui.PopClipRect();
            }

            DrawSortHitTargets(column, header, hitMin, width, hitMax.Y - hitMin.Y);
        }
        finally
        {
            ImGui.PopID();
        }
    }

    private static Vector2 GetHeaderHitMin(string label, Vector2 cellMin, float headerHeight)
        => IsGroupedOrSplitHeader(label)
            ? new Vector2(cellMin.X, GetGroupedHeaderSeparatorY(cellMin.Y, headerHeight))
            : cellMin;

    private static bool IsGroupedOrSplitHeader(string label)
    {
        if (TryGetGroupedHeaderParts(label, out _, out _))
            return true;

        var newline = label.IndexOf('\n');
        return newline >= 0 && label[(newline + 1)..].Contains('|');
    }

    private static bool ShareGroupedHeader(string lhsLabel, string rhsLabel)
    {
        return TryGetGroupedHeaderParts(lhsLabel, out var lhsGroup, out _)
            && TryGetGroupedHeaderParts(rhsLabel, out var rhsGroup, out _)
            && lhsGroup == rhsGroup;
    }

    private void DrawSortHitTargets(int column, Column<TItem> header, Vector2 cellMin, float width, float headerHeight)
    {
        if (!Sortable)
            return;

        var sortKeyCount = Math.Max(1, header.SortKeyCount);
        if (sortKeyCount == 1)
        {
            ImGui.SetCursorScreenPos(cellMin);
            if (ImGui.InvisibleButton($"##sort_{column}", new Vector2(width, headerHeight)))
                SetSort(column, 0);
            return;
        }

        var subWidth = width / sortKeyCount;
        for (var sortKey = 0; sortKey < sortKeyCount; sortKey++)
        {
            ImGui.SetCursorScreenPos(cellMin + new Vector2(subWidth * sortKey, 0));
            if (ImGui.InvisibleButton($"##sort_{column}_{sortKey}", new Vector2(subWidth, headerHeight)))
                SetSort(column, sortKey);
        }
    }

    private void SetSort(int column, int sortKey)
    {
        if (SortIdx == column && _sortKeyIdx == sortKey)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            SortIdx = column;
            _sortKeyIdx = sortKey;
            _sortAscending = true;
        }

        SortDirty = true;
    }

    private static void DrawHeaderLabel(string label, Vector2 cellMin, float width, float headerHeight, int sortedSubKey = -1, bool sortAscending = true)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var padding = GetCellPadding();
        if (TryGetGroupedHeaderParts(label, out _, out var subLabel))
        {
            var lineHeight = ImGui.GetTextLineHeight();
            var subY = cellMin.Y + headerHeight - lineHeight - 4f * scale;
            var text = sortedSubKey == 0 ? $"{subLabel}{(sortAscending ? "↑" : "↓")}" : subLabel;
            DrawCenteredHeaderText(text, cellMin.X, subY, width, sortedSubKey == 0 ? GetSortTextColor() : GetHeaderSubTextColor());
            return;
        }

        var newline = label.IndexOf('\n');
        if (newline >= 0)
        {
            var title = label[..newline];
            var subLabelLine = label[(newline + 1)..];
            if (!subLabelLine.Contains('|'))
                goto DrawRegularLabel;

            var drawList = ImGui.GetWindowDrawList();
            var subLabels = GetHeaderSubLabels(subLabelLine);
            var subCount = subLabels.Length;
            var lineHeight = ImGui.GetTextLineHeight();
            var subWidth = width / subCount;
            var titleY = cellMin.Y + 4f * scale;
            var subY = cellMin.Y + headerHeight - lineHeight - 4f * scale;
            var lineColor = GetGridColor();
            var separatorY = GetGroupedHeaderSeparatorY(cellMin.Y, headerHeight);

            drawList.AddLine(
                new Vector2(cellMin.X + scale, separatorY),
                new Vector2(cellMin.X + width - scale, separatorY),
                GetHeaderSubRowSeparatorColor(),
                1.2f * scale);

            for (var i = 1; i < subCount; i++)
            {
                var x = cellMin.X + subWidth * i;
                drawList.AddLine(new Vector2(x, separatorY), new Vector2(x, cellMin.Y + headerHeight - 3f * scale), lineColor);
            }

            DrawCenteredHeaderText(title, cellMin.X, titleY, width, GetHeaderGroupTextColor());

            for (var i = 0; i < subCount; i++)
            {
                var text = i == sortedSubKey ? $"{subLabels[i]}{(sortAscending ? "↑" : "↓")}" : subLabels[i];
                DrawCenteredHeaderText(text, cellMin.X + subWidth * i, subY, subWidth, i == sortedSubKey ? GetSortTextColor() : GetHeaderSubTextColor());
            }
            return;
        }

    DrawRegularLabel:
        var displayLabel = sortedSubKey == 0 ? $"{label} {(sortAscending ? "↑" : "↓")}" : label;
        var labelSize = ImGui.CalcTextSize(displayLabel);
        var headerTextY = label.Contains('\n')
            ? 2f * scale
            : MathF.Max(0, (headerHeight - labelSize.Y) * 0.5f);
        var textX = cellMin.X + padding.X + MathF.Max(0, (width - padding.X * 2f - labelSize.X) * 0.5f);
        ImGui.SetCursorScreenPos(new Vector2(textX, cellMin.Y + headerTextY));
        ImGui.TextColored(sortedSubKey == 0 ? GetSortTextColor() : GetHeaderTextColor(), displayLabel);
    }

    private static bool TryGetGroupedHeaderParts(string label, out string group, out string subLabel)
    {
        if (!GroupedHeaderPartsCache.TryGetValue(label, out var cached))
        {
            var newline = label.IndexOf('\n');
            if (newline < 0 || newline != label.LastIndexOf('\n'))
            {
                cached = (false, string.Empty, string.Empty);
            }
            else
            {
                var groupText = label[..newline].Trim();
                var subText = label[(newline + 1)..].Trim();
                cached = subText.Contains('|') || groupText.Length == 0 || subText.Length == 0
                    ? (false, string.Empty, string.Empty)
                    : (true, groupText, subText);
            }

            GroupedHeaderPartsCache[label] = cached;
        }

        group = cached.Group;
        subLabel = cached.SubLabel;
        return cached.Matches;
    }

    private static string[] GetHeaderSubLabels(string subLabelLine)
    {
        if (!HeaderSubLabelsCache.TryGetValue(subLabelLine, out var subLabels))
        {
            subLabels = subLabelLine.Split('|');
            HeaderSubLabelsCache[subLabelLine] = subLabels;
        }

        return subLabels;
    }

    private static void DrawCenteredHeaderText(string text, float x, float y, float width, Vector4 color)
    {
        var textWidth = ImGui.CalcTextSize(text).X;
        ImGui.SetCursorScreenPos(new Vector2(x + MathF.Max(0, (width - textWidth) * 0.5f), y));
        ImGui.TextColored(color, text);
    }

    private void DrawBody(IReadOnlyList<int> visibleColumns, IReadOnlyList<float> columnWidths, float totalWidth, float headerHeight)
    {
        var bodyHeight = MathF.Max(0, ImGui.GetContentRegionAvail().Y);
        var flags = _allowHorizontalScroll ? ImGuiWindowFlags.HorizontalScrollbar : ImGuiWindowFlags.None;
        if (!ImGui.BeginChild("##customTableBody", new Vector2(0, bodyHeight), false, flags))
        {
            ImGui.EndChild();
            return;
        }

        try
        {
            if (_resetScrollY)
            {
                ImGui.SetScrollY(0);
                _resetScrollY = false;
            }

            _tableScrollX = _allowHorizontalScroll ? ImGui.GetScrollX() : 0f;
            var contentWidth = _allowHorizontalScroll ? MathF.Max(totalWidth, ImGui.GetWindowWidth()) : totalWidth;
            var drawWidth = _allowHorizontalScroll ? MathF.Max(totalWidth, _tableScrollX + ImGui.GetWindowWidth()) : ImGui.GetWindowWidth();

            if (FilteredItems.Count == 0)
            {
                var messagePos = ImGui.GetCursorScreenPos() + new Vector2(8f * ImGuiHelpers.GlobalScale, 8f * ImGuiHelpers.GlobalScale);
                DrawVisibleEmptyRows(ItemHeight, drawWidth, contentWidth, 0);
                ImGui.SetCursorScreenPos(messagePos);
                ImGui.TextDisabled(T("No missions match the current filters."));
                return;
            }

            var scrollY = ImGui.GetScrollY();
            var firstRow = Math.Clamp((int)MathF.Floor(scrollY / ItemHeight), 0, Math.Max(0, FilteredItems.Count - 1));
            var rowsOnScreen = Math.Max(1, (int)MathF.Ceiling((ImGui.GetWindowHeight() + ItemHeight) / ItemHeight));
            var lastRow = Math.Min(FilteredItems.Count, firstRow + rowsOnScreen + 1);

            if (firstRow > 0)
                ImGui.Dummy(new Vector2(contentWidth, firstRow * ItemHeight));

            _currentIdx = firstRow;
            for (var row = firstRow; row < lastRow; row++)
                DrawItem(FilteredItems[row], visibleColumns, columnWidths, ItemHeight, totalWidth, drawWidth, contentWidth, row);

            var remainingRows = FilteredItems.Count - lastRow;
            if (remainingRows > 0)
                ImGui.Dummy(new Vector2(contentWidth, remainingRows * ItemHeight));
            else
                DrawVisibleEmptyRows(ItemHeight, drawWidth, contentWidth, lastRow);
        }
        finally
        {
            ImGui.EndChild();
        }
    }

    private void DrawVisibleEmptyRows(float rowHeight, float drawWidth, float contentWidth, int startRow)
    {
        var bottomScrollbarHeight = _allowHorizontalScroll ? ImGui.GetStyle().ScrollbarSize : 0f;
        var bottom = ImGui.GetWindowPos().Y + ImGui.GetWindowHeight() - bottomScrollbarHeight;
        var row = startRow;
        var rowMin = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();

        while (rowMin.Y < bottom)
        {
            var visualHeight = MathF.Min(rowHeight, bottom - rowMin.Y);
            var rowMax = rowMin + new Vector2(drawWidth, visualHeight);
            var rowColor = row % 2 == 0
                ? ImGui.GetColorU32(new Vector4(0.07f, 0.10f, 0.16f, 0.92f))
                : ImGui.GetColorU32(new Vector4(0.09f, 0.13f, 0.20f, 0.92f));

            drawList.AddRectFilled(rowMin, rowMax, rowColor);
            drawList.AddLine(new Vector2(rowMin.X, rowMax.Y), rowMax, GetGridColor());

            rowMin.Y += visualHeight;
            row++;
        }
    }

    protected virtual float GetColumnWidth(Column<TItem> header, int index)
    {
        if (header.Width > 0)
            return header.Width;

        var scale = ImGuiHelpers.GlobalScale;
        return MathF.Max(88f * scale, ImGui.CalcTextSize(header.Label).X + 34f * scale);
    }

    protected virtual uint GetRowBackgroundColor(TItem item, int itemIndex, int rowNumber, bool hovered)
    {
        if (hovered)
            return ImGui.GetColorU32(new Vector4(0.18f, 0.30f, 0.46f, 0.72f));

        return rowNumber % 2 == 0
            ? ImGui.GetColorU32(new Vector4(0.07f, 0.10f, 0.16f, 0.92f))
            : ImGui.GetColorU32(new Vector4(0.09f, 0.13f, 0.20f, 0.92f));
    }

    private static float GetHeaderHeight()
        => MathF.Max(ImGui.GetTextLineHeight() * 2f + 18f * ImGuiHelpers.GlobalScale, 46f * ImGuiHelpers.GlobalScale);

    private static float GetGroupedHeaderSeparatorY(float headerTopY, float headerHeight)
        => headerTopY + MathF.Round(headerHeight * 0.50f);

    private static Vector2 GetCellPadding()
        => new(6f * ImGuiHelpers.GlobalScale, 0);

    private static float GetFrameRounding()
        => 8f * ImGuiHelpers.GlobalScale;

    private static uint GetHeaderColor()
        => ImGui.GetColorU32(new Vector4(0.13f, 0.19f, 0.29f, 1.0f));

    private static uint GetHeaderHoverColor()
        => ImGui.GetColorU32(new Vector4(0.18f, 0.29f, 0.44f, 0.95f));

    private static uint GetGridColor()
        => ImGui.GetColorU32(new Vector4(0.24f, 0.31f, 0.43f, 0.55f));

    private static uint GetBorderColor()
        => ImGui.GetColorU32(new Vector4(0.36f, 0.52f, 0.72f, 0.72f));

    private static Vector4 GetHeaderTextColor()
        => new(0.88f, 0.94f, 1.0f, 1.0f);

    private static Vector4 GetHeaderGroupTextColor()
        => new(0.72f, 0.86f, 1.0f, 1.0f);

    private static Vector4 GetHeaderSubTextColor()
        => new(0.92f, 0.97f, 1.0f, 1.0f);

    private static uint GetHeaderSubRowSeparatorColor()
        => ImGui.GetColorU32(new Vector4(0.42f, 0.58f, 0.82f, 0.58f));

    private static Vector4 GetSortTextColor()
        => new(0.46f, 0.72f, 1.0f, 1.0f);
}
