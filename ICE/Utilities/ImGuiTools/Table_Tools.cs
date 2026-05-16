using Dalamud.Interface;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Utilities.ImGuiTools;

public static partial class ImGui_Ice
{
    public static void Table_FontFullCenter(FontAwesomeIcon icon)
    {
        var cursorPosX = ImGui.GetCursorPosX();
        var availWidth = ImGui.GetContentRegionAvail().X;
        var textWidth = ImGui.CalcTextSize(icon.ToIconString()).X;

        ImGui.SetCursorPosX(cursorPosX + (availWidth - textWidth) * 0.5f);
        ImGui.AlignTextToFramePadding();

        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(icon.ToIconString());
        ImGui.PopFont();
    }
    public static bool Table_CenterCheckbox(string id, ref bool value)
    {
        var cursorPos = ImGui.GetCursorPos();
        var availWidth = ImGui.GetContentRegionAvail().X;

        var checkboxSize = ImGui.GetFrameHeight();
        ImGui.SetCursorPosX(cursorPos.X + (availWidth - checkboxSize) * 0.5f);
        ImGui.AlignTextToFramePadding();

        return ImGui.Checkbox($"##{id}", ref value);
    }
    public static void Table_FullCenterText(string text)
    {
        var cursorPosX = ImGui.GetCursorPosX();
        var availWidth = ImGui.GetContentRegionAvail().X;
        var textWidth = ImGui.CalcTextSize(text).X;

        ImGui.SetCursorPosX(cursorPosX + (availWidth - textWidth) * 0.5f);
        ImGui.AlignTextToFramePadding();

        ImGui.TextUnformatted(text);
    }
    public static void Table_FullCenterText(string icon, Vector4 color)
    {
        var cursorPosX = ImGui.GetCursorPosX();
        var availWidth = ImGui.GetContentRegionAvail().X;
        var textWidth = ImGui.CalcTextSize(icon).X;

        ImGui.SetCursorPosX(cursorPosX + (availWidth - textWidth) * 0.5f);
        ImGui.AlignTextToFramePadding();

        FontAwesome.Print(color, icon);
    }
    public static void Table_FullCenterFont(FontAwesomeIcon icon)
    {
        var iconText = icon.ToIconString();

        ImGui.PushFont(UiBuilder.IconFont);
        var textSize = ImGui.CalcTextSize(iconText);
        ImGui.PopFont();

        var availWidth = ImGui.GetContentRegionAvail().X;
        var cursorPosX = ImGui.GetCursorPosX();

        ImGui.SetCursorPosX(cursorPosX + (availWidth - textSize.X) * 0.5f);
        ImGui.AlignTextToFramePadding();

        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(iconText);
        ImGui.PopFont();
    }
    public static void Table_FontCenter(FontAwesomeIcon icon)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(icon.ToIconString());
        ImGui.PopFont();
    }
    public static bool Table_CenteredButton(string label, Vector2? buttonSize = null)
    {
        var cursorPosX = ImGui.GetCursorPosX();
        var availWidth = ImGui.GetContentRegionAvail().X;

        Vector2 actualButtonSize;
        if (buttonSize.HasValue)
        {
            actualButtonSize = buttonSize.Value;
        }
        else
        {
            var textSize = ImGui.CalcTextSize(label);
            var framePadding = ImGui.GetStyle().FramePadding;
            actualButtonSize = new Vector2(textSize.X + framePadding.X * 2 + 10f, textSize.Y + framePadding.Y * 2);
        }

        ImGui.SetCursorPosX(cursorPosX + (availWidth - actualButtonSize.X) * 0.5f);
        return ImGui.Button(label, actualButtonSize);
    }
    public static void CompletionStatusButton(CosmicHelper.CosmicInfo missionInfo)
    {
        const float imageSize = 23f;

        if (missionInfo.CompletionStatus is CosmicHelper.Status.Gold)
        {
            if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex &&
                tex.TryGetWrap(out var wrap, out _))
            {
                var cellWidth = ImGui.GetContentRegionAvail().X;
                var cellHeight = ImGui.GetFrameHeight();
                var cursor = ImGui.GetCursorPos();

                ImGui.SetCursorPosX(cursor.X + (cellWidth - imageSize) * 0.5f);
                ImGui.SetCursorPosY(cursor.Y + (cellHeight - imageSize) * 0.5f);
                ImGui.Image(wrap.Handle, new Vector2(imageSize),
                            new Vector2(0.2347f, 0.3500f),
                            new Vector2(0.2959f, 0.6500f));
            }
        }
        else if (missionInfo.CompletionStatus is CosmicHelper.Status.Completed)
        {
            ImGui.AlignTextToFramePadding();
            Table_FullCenterText(FontAwesome.Check, EColor.Green);
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            Table_FullCenterText(FontAwesome.Cross, EColor.Red);
        }
    }
    public static void CompletionStatusIcon(CosmicHelper.CosmicInfo missionInfo)
    {
        const float imageSize = 23f;

        if (missionInfo.CompletionStatus is CosmicHelper.Status.Gold)
        {
            if (Svc.Texture.GetFromGame("ui/uld/WKSMission_hr1.tex") is { } tex &&
                tex.TryGetWrap(out var wrap, out _))
            {
                ImGui.Image(wrap.Handle, new Vector2(imageSize), new Vector2(0.2347f, 0.3500f), new Vector2(0.2959f, 0.6500f));
            }
        }
        else if (missionInfo.CompletionStatus is CosmicHelper.Status.Completed)
        {
            ImGui.AlignTextToFramePadding();
            FontAwesome.Print(EColor.Green, FontAwesome.Check);
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            FontAwesome.Print(EColor.Red, FontAwesome.Cross);
        }
    }
    public static void DrawColoredStar(TurninState state)
    {
        Vector4 color = state switch
        {
            TurninState.Bronze => new Vector4(0.8f, 0.5f, 0.3f, 1.0f),  // Bronze
            TurninState.Silver => new Vector4(0.75f, 0.75f, 0.75f, 1.0f), // Silver
            TurninState.Gold => new Vector4(1.0f, 0.84f, 0.0f, 1.0f),    // Gold
            TurninState.Critical => new Vector4(1.0f, 0.84f, 0.0f, 1.0f), // Gold
            _ => new Vector4(0, 0, 0, 0) // Transparent/none
        };

        if (state != TurninState.None)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.PushFont(UiBuilder.IconFont); // Make sure you're using the icon font
            ImGui.Text(FontAwesomeIcon.Star.ToIconString());
            ImGui.PopFont();
            ImGui.PopStyleColor();
        }
    }
}
