using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs;

internal class Ui_WorldIconTEst
{
    private static Vector3 _markerPos = Vector3.Zero;

    public static void DrawControls()
    {
        if (ImGui.Button(T("Set marker to current position")))
            _markerPos = Svc.Objects.LocalPlayer?.Position ?? Vector3.Zero;
    }

    public static void DrawOverlay()
    {
        // if (_markerPos != Vector3.Zero)
            // PictoManager.DrawIcon(CosmicHelper.JobIconDict[8].GetWrapOrEmpty().Handle, _markerPos, new(24, 24));
    }

    // --- Map Helpers ---

    private unsafe static bool TryGetMapScreenRect(out Vector2 mapPos, out Vector2 mapSize)
    {
        mapPos = Vector2.Zero;
        mapSize = Vector2.Zero;

        var mapAddon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("AreaMap").Address;
        if (mapAddon == null || !mapAddon->IsVisible) return false;

        var mapNode = mapAddon->GetNodeById(5);
        if (mapNode == null) return false;

        mapPos = new Vector2(mapNode->ScreenX, mapNode->ScreenY);
        mapSize = new Vector2(mapNode->Width * mapNode->ScaleX, mapNode->Height * mapNode->ScaleY);
        return true;
    }

    private static bool TryGetCurrentMap(out Map map)
    {
        map = default;

        var territory = Svc.Data.GetExcelSheet<TerritoryType>()?.GetRow(Svc.ClientState.TerritoryType);

        if (territory == null) return false;

        map = territory.Value.Map.Value;
        return true;
    }

    private static Vector2 WorldToScreenOnMap(float worldX, float worldZ, Map map, Vector2 mapPos, Vector2 mapSize)
    {
        var nx = NormalizedFromMapCoord(MapCoordFromWorld(worldX, map.OffsetX, map.SizeFactor));
        var ny = NormalizedFromMapCoord(MapCoordFromWorld(worldZ, map.OffsetY, map.SizeFactor));

        return mapPos + new Vector2(nx * mapSize.X, ny * mapSize.Y);
    }

    private static float MapCoordFromWorld(float worldPos, short offset, ushort sizeFactor)
    {
        var scale = sizeFactor / 100f;
        return ((worldPos + offset) * scale / 2048f) * 41f + 1f;
    }

    private static float NormalizedFromMapCoord(float mapCoord)
    {
        return (mapCoord - 1f) / 41f;
    }

    // --- Drawing ---

    private static void DrawIconAt(ISharedImmediateTexture texture, Vector2 screenPos, Vector2 size)
    {
        var topLeft = screenPos - size / 2; // centered
        ImGui.GetForegroundDrawList().AddImage(texture.GetWrapOrEmpty().Handle, topLeft, topLeft + size);
    }
}
