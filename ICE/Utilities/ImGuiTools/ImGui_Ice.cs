using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Reflection;

namespace ICE.Utilities.ImGuiTools;

public static partial class ImGui_Ice
{
    private static Dictionary<string, bool> categoryStates = new();

    // General Functions that are used everywhere across the plugin
    /// <summary>
    /// Greyscale icon that I have saved for general ease of use. Looks better than the GC icons
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    public static IDalamudTextureWrap? GetGreyscaleJob(uint jobId = 0)
    {
        if (jobId == 0)
            jobId = C.SelectedJob;

        string greyJobIcon = jobId switch
        {
            8 => "ICE.Resources.GreyscaleJobs.CRP.png",
            9 => "ICE.Resources.GreyscaleJobs.BSM.png",
            10 => "ICE.Resources.GreyscaleJobs.ARM.png",
            11 => "ICE.Resources.GreyscaleJobs.GSM.png",
            12 => "ICE.Resources.GreyscaleJobs.LTW.png",
            13 => "ICE.Resources.GreyscaleJobs.WVR.png",
            14 => "ICE.Resources.GreyscaleJobs.ALC.png",
            15 => "ICE.Resources.GreyscaleJobs.CUL.png",
            16 => "ICE.Resources.GreyscaleJobs.MIN.png",
            17 => "ICE.Resources.GreyscaleJobs.BTN.png",
            18 => "ICE.Resources.GreyscaleJobs.FSH.png",
            _ => "ICE.Resources.GreyscaleJobs.Default.png",
        };

        return Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), greyJobIcon).GetWrapOrEmpty();
    }
    public static bool Sidebar_CollaspableHeader(string label, FontAwesomeIcon? icon = null, IDalamudTextureWrap? imageTexture = null)
    {
        float scale = ImGuiHelpers.GlobalScale;

        // Default Colors for Theming. This is really here to make sure it's formatted as I want it to be
        var headerColor = ImGui.GetColorU32(ImGuiCol.Header);
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);

        // This is here to make sure that
        // A: If it doesn't already exist, add it and just make it false (This makes it to where it's not expanded by default)
        //    - Could absolutely change that to true if I want to make it shown on inital creation
        // B: Returns the state in a form to where if that's true, then I could display the elements below it properly
        string categoryId = label;
        if (!C.MainUi_CustomHeader.ContainsKey(categoryId))
        {
            C.MainUi_CustomHeader[categoryId] = false;
            C.SaveDebounced();
        }

        bool isExpanded = C.MainUi_CustomHeader[categoryId];

        // Need these here for two reasons:
        // 1: drawList allows me to create un-conventional things that isn't included in the Imgui Library
        // 2: curserPos allows me to grab the absolute position, which is necessary to make sure things are lined up properly
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();

        // This is currently how I'm going to autosize it based on the contents of the window.
        // (as of typing this). It gets the width of the window and expands it on that (these are meant for the sidebar)
        // Height is scaled based on the global font scale
        float width = ImGui.GetContentRegionAvail().X;
        float height = 30 * scale;

        // Check for click, nice little rectangle area where it can be clicked at. This takes in account the above things to make sure it's only clicking within this area
        bool isHovered = ImGui.IsMouseHoveringRect(cursorPos, new Vector2(cursorPos.X + width, cursorPos.Y + height));
        bool isClicked = isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        if (isClicked)
        {
            C.MainUi_CustomHeader[categoryId] = !C.MainUi_CustomHeader[categoryId];
            isExpanded = C.MainUi_CustomHeader[categoryId];
            C.SaveDebounced();
        }

        // Change header color slightly on hover, just a nice QOL
        if (isHovered)
            headerColor = ImGui.GetColorU32(ImGuiCol.HeaderHovered);

        // Drawing the rectangle itself/ custom thingy. This is the container for all the fancy smancy stuff.
        // Border radius scaled
        drawList.AddRectFilled(cursorPos, new Vector2(cursorPos.X + width, cursorPos.Y + height), headerColor, 5.0f * scale);

        // Calculating the vertical spacing here, need to make sure it fits within our custom box nice and cozy
        float imageSize = 23 * scale; // Used for images specifically, since I like things being aligned with each other
        float textHeight = ImGui.CalcTextSize(label).Y;
        float verticalPadding = (height - textHeight) / 2;

        // Adding some padding to the left, don't need it feeling like it's right against the box. We're making somewhat bubbly things
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + verticalPadding);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 8 * scale);

        // Drawing either an image, icon, or nothing at all (should really only be the first 2, but on the off chance I decide to just use text)
        if (imageTexture != null)
        {
            // Calculating the offset here to center the image with the text (OCD here)
            float imageYOffset = (textHeight - imageSize) / 2;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + imageYOffset);

            // Adding a little bit of padding here for the image -> text (scaled)
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 4 * scale); // Current set to -4 to bring it closer, but can be changed to give it more space (lowering the number) or removing more space (increasing number)

            ImGui.Image(imageTexture.Handle, new Vector2(imageSize, imageSize));

            // Resetting the Y position for text, to make sure it lines up (scaled spacing)
            ImGui.SameLine(0, 2 * scale);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - imageYOffset);
        }
        else if (icon.HasValue)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, textColor); // Disabled text color here to match the rest of it
            ImGuiEx.Icon(icon.Value);
            ImGui.PopStyleColor();
            ImGui.SameLine();
        }
        else
        {
            // No icon, just adding some sameline spacing
            ImGui.SameLine();
        }

        // Actual label here
        ImGui.PushStyleColor(ImGuiCol.Text, textColor);
        ImGui.Text(label);
        ImGui.PopStyleColor();

        // Replace the badge count section with the caret icon (scaled padding)
        float iconSize = ImGui.CalcTextSize(FontAwesomeIcon.CaretDown.ToIconString()).X;
        float rightPadding = 10 * scale;

        float iconXPos = cursorPos.X + width - iconSize - rightPadding;
        float iconYPos = cursorPos.Y + verticalPadding;

        ImGui.SetCursorScreenPos(new Vector2(iconXPos, iconYPos));
        ImGui.PushStyleColor(ImGuiCol.Text, textColor);
        ImGuiEx.Icon(isExpanded ? FontAwesomeIcon.CaretSquareDown : FontAwesomeIcon.CaretSquareRight);
        ImGui.PopStyleColor();

        // Last but not least, resetting it for the next thing here:
        ImGui.SetCursorScreenPos(new Vector2(cursorPos.X, cursorPos.Y + height));
        ImGui.Spacing();

        return isExpanded;
    }
    public static void DrawSelectable_Icon(FontAwesomeIcon icon, string label, string id)
    {
        bool isSelected = C.MainUi_SelectedWindow == id;
        float scale = ImGuiHelpers.GlobalScale;

        // Change background color if selected
        if (isSelected)
        {
            ImGui.PushStyleColor(ImGuiCol.Header, ImGui.GetColorU32(ImGuiCol.HeaderActive));
        }

        // Indent for items under categories (scaled)
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 16 * scale);

        float width = ImGui.GetContentRegionAvail().X;

        // Invisible selectable as the clickable area (scaled height)
        if (ImGui.Selectable($"##{id}", isSelected, ImGuiSelectableFlags.None, new Vector2(width, 25 * scale)))
        {
            C.MainUi_SelectedWindow = id;
            C.SaveDebounced();
        }

        if (isSelected)
        {
            ImGui.PopStyleColor();

            // Draw colored bar on the left side
            var drawList = ImGui.GetWindowDrawList();
            var rectMin = ImGui.GetItemRectMin();
            var rectMax = ImGui.GetItemRectMax();

            // Draw a 3-4 pixel wide bar on the left (scaled)
            drawList.AddRectFilled(
                rectMin,
                new Vector2(rectMin.X + 3 * scale, rectMax.Y),
                ImGui.GetColorU32(new Vector4(0.4f, 0.7f, 1.0f, 1.0f)) // Your accent color here
            );
        }

        // Get the position of that selectable we just drew
        float itemY = ImGui.GetItemRectMin().Y;

        // Set cursor back to draw icon and text on top (scaled offsets)
        ImGui.SetCursorScreenPos(new Vector2(ImGui.GetItemRectMin().X + 8 * scale, itemY + 4 * scale));

        ImGuiEx.Icon(icon);
        ImGui.SameLine();
        ImGui.Text(label);

        // Add small spacing between items (scaled)
        ImGui.Dummy(new Vector2(0, 2 * scale));
    }
    public static void DrawSelectable_Image(uint iconId, string label, string id)
    {
        bool isSelected = C.MainUi_SelectedWindow == id;
        float scale = ImGuiHelpers.GlobalScale;

        // Change background color if selected
        if (isSelected)
        {
            ImGui.PushStyleColor(ImGuiCol.Header, ImGui.GetColorU32(ImGuiCol.HeaderActive));
        }

        // Indent for items under categories (scaled)
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 16 * scale);

        float width = ImGui.GetContentRegionAvail().X;

        // Invisible selectable as the clickable area (scaled height)
        if (ImGui.Selectable($"##{id}", isSelected, ImGuiSelectableFlags.None, new Vector2(width, 25 * scale)))
        {
            C.MainUi_SelectedWindow = id;
            C.SaveDebounced();
        }

        if (isSelected)
        {
            ImGui.PopStyleColor();

            // Draw colored bar on the left side
            var drawList = ImGui.GetWindowDrawList();
            var rectMin = ImGui.GetItemRectMin();
            var rectMax = ImGui.GetItemRectMax();

            // Draw a 3-4 pixel wide bar on the left (scaled)
            drawList.AddRectFilled(
                rectMin,
                new Vector2(rectMin.X + 3 * scale, rectMax.Y),
                ImGui.GetColorU32(new Vector4(0.4f, 0.7f, 1.0f, 1.0f)) // Your accent color here
            );
        }

        // Get the position of that selectable we just drew
        float itemY = ImGui.GetItemRectMin().Y;

        // Set cursor back to draw image and text on top (scaled offsets)
        // Match the Icon version's offset pattern
        ImGui.SetCursorScreenPos(new Vector2(ImGui.GetItemRectMin().X + 8 * scale, itemY + 4 * scale));

        Svc.Texture.TryGetFromGameIcon(iconId, out var iconImage);
        if (iconImage != null)
        {
            var image = iconImage.GetWrapOrEmpty();
            Vector2 imageSize = new Vector2(25 * scale, 25 * scale);
            // Remove the vertical centering - just draw at current position
            ImGui.Image(image.Handle, imageSize);
        }

        ImGui.SameLine();
        ImGui.Text(label);
    }
    public static bool DrawStyledImageButton(IDalamudTextureWrap? icon, Vector2 size, bool enabled = true)
    {
        Vector4 buttonColor, borderColor;
        float borderSize;

        if (enabled)
        {
            buttonColor = new Vector4(0.3f, 0.3f, 0.35f, 0.7f);
            borderColor = new Vector4(0.898f, 0.8f, 0.501f, 1f);
            borderSize = 1.0f;
        }
        else
        {
            buttonColor = new Vector4(0.2f, 0.2f, 0.2f, 0.1f);
            borderColor = new Vector4(0.4f, 0.4f, 0.4f, 0.5f);
            borderSize = 0.5f;
        }

        // Apply the custom styling
        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonColor * 1.1f); // Slightly brighter on hover
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, buttonColor * 0.9f);  // Slightly darker when pressed
        ImGui.PushStyleColor(ImGuiCol.Border, borderColor);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, borderSize);

        // Create the ImageButton
        bool clicked = ImGui.ImageButton(icon.Handle, size);

        // Restore original styling
        ImGui.PopStyleVar(); // FrameBorderSize
        ImGui.PopStyleColor(4); // Button, ButtonHovered, ButtonActive, Border

        return clicked;
    }
    public static bool DrawStyledImageButton(ISharedImmediateTexture? icon, Vector2 size, bool enabled = true)
    {
        Vector4 buttonColor, borderColor;
        float borderSize;

        if (enabled)
        {
            buttonColor = new Vector4(0.3f, 0.3f, 0.35f, 0.7f);
            borderColor = new Vector4(0.898f, 0.8f, 0.501f, 1f);
            borderSize = 1.0f;
        }
        else
        {
            buttonColor = new Vector4(0.2f, 0.2f, 0.2f, 0.1f);
            borderColor = new Vector4(0.4f, 0.4f, 0.4f, 0.5f);
            borderSize = 0.5f;
        }

        float zoomFactor = 0.25f; // 25% zoom-in
        float cropAmount = zoomFactor / 2; // Crop equally from all sides

        Vector2 uv0 = enabled ? new Vector2(0, 0) : new Vector2(cropAmount, cropAmount);
        Vector2 uv1 = enabled ? new Vector2(1, 1) : new Vector2(1 - cropAmount, 1 - cropAmount);

        // Applies the custom code
        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonColor * 1.1f); // Slightly brighter on hover
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, buttonColor * 0.9f);  // Slightly darker when pressed
        ImGui.PushStyleColor(ImGuiCol.Border, borderColor);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, borderSize);

        bool clicked = ImGui.ImageButton(icon.GetWrapOrEmpty().Handle, size, uv0, uv1);

        // Restore original styling
        ImGui.PopStyleVar(); // FrameBorderSize
        ImGui.PopStyleColor(4); // Button, ButtonHovered, ButtonActive, Border

        return clicked;
    }
    public static bool DrawCompactCategoryHeader(string label, FontAwesomeIcon? icon = null)
    {
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();

        // Get colors from current theme
        var headerColor = ImGui.GetColorU32(ImGuiCol.Header);
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);

        // Calculate content size
        float horizontalPadding = 8;
        float verticalPadding = 4;
        float iconTextSpacing = 4;

        var textSize = ImGui.CalcTextSize(label);
        float contentWidth = ImGui.GetContentRegionAvail().X;
        float contentHeight = verticalPadding * 2 + textSize.Y;

        // Check if this category is expanded (default to false)
        string categoryId = label;
        if (!categoryStates.ContainsKey(categoryId))
            categoryStates[categoryId] = false;

        bool isExpanded = categoryStates[categoryId];

        // Check for click
        bool isHovered = ImGui.IsMouseHoveringRect(cursorPos, new Vector2(cursorPos.X + contentWidth, cursorPos.Y + contentHeight))
                      && ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup | ImGuiHoveredFlags.ChildWindows);
        bool isClicked = isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        if (isClicked)
        {
            categoryStates[categoryId] = !categoryStates[categoryId];
            isExpanded = categoryStates[categoryId];
        }

        // Change header color slightly on hover
        if (isHovered)
            headerColor = ImGui.GetColorU32(ImGuiCol.HeaderHovered);

        // Draw background rectangle with rounded corners
        drawList.AddRectFilled(cursorPos,
            new Vector2(cursorPos.X + contentWidth, cursorPos.Y + contentHeight),
            headerColor,
            5.0f);

        // Position cursor with padding
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + horizontalPadding);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + verticalPadding);

        // Draw icon if provided
        if (icon.HasValue)
        {
            ImGuiEx.Icon(icon.Value);
            ImGui.SameLine(0, iconTextSpacing);
        }

        ImGui.Text(label);
        ImGui.SameLine(0, 8);

        // Draw caret icon based on expanded state
        ImGuiEx.Icon(isExpanded ? FontAwesomeIcon.CaretDown : FontAwesomeIcon.CaretRight);

        ImGui.SameLine(0, 8);
        ImGui.Dummy(Vector2.Zero);

        // Advance cursor past the header
        ImGui.SetCursorScreenPos(new Vector2(cursorPos.X, cursorPos.Y + contentHeight));
        ImGui.Spacing();

        return isExpanded;
    }
    public static void DrawJobButtons(uint jobId, string tooltip)
    {
        float scale = ImGuiHelpers.GlobalScale;

        uint selectedJob = C.SelectedJob;
        bool state = selectedJob == jobId;
        var iconEnabled = CosmicHelper.JobIconDict[jobId];
        var iconDisabled = GetGreyscaleJob(jobId);
        Vector2 size = new Vector2(26 * scale, 26 * scale);
        bool autoPickCurrentJob = C.AutoPickCurrentJob;

        if (state)
        {
            if (DrawStyledImageButton(iconEnabled, size, state))
            {
                if (autoPickCurrentJob)
                {
                    autoPickCurrentJob = false;
                    C.AutoPickCurrentJob = autoPickCurrentJob;
                }

                C.SelectedJob = jobId;
                C.Save();
            }
        }
        else if (!state)
        {
            if (DrawStyledImageButton(iconDisabled, size, state))
            {
                if (autoPickCurrentJob)
                {
                    autoPickCurrentJob = false;
                    C.AutoPickCurrentJob = autoPickCurrentJob;
                }

                C.SelectedJob = jobId;
                C.Save();
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltip);
            ImGui.EndTooltip();
        }
    }
    public static void Draw_XPBar(float current, float needed, float max = 0, string label = null, Vector2? size = null)
    {
        // If we want it to have a standard label above the bar. Not required but for small things it's nice to just have the option
        if (label != null)
        {
            ImGui.TextWrapped(label);
        }

        // Setting the dimensions of the custom bar/drawing it.
        // Usual stuff of drawlist being OP
        var pos = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();

        // Calculating the size of the bar and everything here.
        // If size is null, then it just defaults to the norm. Otherwise it uses whatever size we set (nice in case I want to use this for other things besides XP/Modify it a bit easier)
        var barStart = pos;
        var actualSize = size ?? new Vector2(ImGui.GetContentRegionAvail().X, 10);
        var barEnd = new Vector2(pos.X + actualSize.X, pos.Y + actualSize.Y);

        // Draw background (dark gray)
        drawList.AddRectFilled(barStart, barEnd, ImGui.GetColorU32(new Vector4(0.15f, 0.15f, 0.15f, 1f)));

        // Now comes the fun part, actually creating the filling (that sounds bad)

        // Defining the colors globaly here just cause they're used across the board
        var blueColor = new Vector4(0.2f, 0.6f, 1f, 1f);      // Blue #3399ff  - Fill Bar Part #1 (Left Side)
        var greenColor = new Vector4(0.6f, 1f, 0.8f, 1f);      // Green #99ffcc - Fill Bar Part #2 (Right Side)
        var goldColor = new Vector4(1f, 0.84f, 0f, 1f);      // Gold #ffd600  - Overcap color

        // Case 1: At or above cap when needed == max (show full gold) [Really only used when at max stage for that planet when a new one comes out)
        if (needed > 0 && needed == max && current >= needed)
        {
            drawList.AddRectFilled(barStart, barEnd, ImGui.GetColorU32(goldColor));
        }
        // Case 2: Normal progression (not overcapped)
        else if (current <= needed && needed > 0)
        {
            float fraction = Math.Clamp((float)current / needed, 0f, 1f);
            float filledWidth = actualSize.X * fraction;

            if (filledWidth > 0f)
            {
                var filledEnd = new Vector2(pos.X + filledWidth, pos.Y + actualSize.Y);
                drawList.AddRectFilledMultiColor(
                    barStart, filledEnd,
                    ImGui.GetColorU32(blueColor),  // top-left
                    ImGui.GetColorU32(greenColor), // top-right
                    ImGui.GetColorU32(greenColor), // bottom-right
                    ImGui.GetColorU32(blueColor)   // bottom-left
                );
            }
        }
        // Case 3: Overcapped (show gradient + gold overlay)
        else if (current > needed && max > 0 && needed > 0)
        {
            // Full blue-green gradient background
            drawList.AddRectFilledMultiColor(
                barStart, barEnd,
                ImGui.GetColorU32(blueColor),
                ImGui.GetColorU32(greenColor),
                ImGui.GetColorU32(greenColor),
                ImGui.GetColorU32(blueColor)
            );

            // Gold overlay for overcap amount
            float overcapAmount = current - needed;
            float overcapRange = max - needed;
            float overcapFraction = Math.Clamp((float)overcapAmount / overcapRange, 0f, 1f);
            float goldWidth = actualSize.X * overcapFraction;

            if (goldWidth > 0f)
            {
                var goldEnd = new Vector2(pos.X + goldWidth, pos.Y + actualSize.Y);
                drawList.AddRectFilled(barStart, goldEnd, ImGui.GetColorU32(goldColor));
            }
        }
        // Case 4: No needed XP (cosmic score scenario - just show progress to max) [Nice for just pure gold bar to fill]
        else if (needed == 0 && max > 0)
        {
            float fraction = Math.Clamp((float)current / max, 0f, 1f);
            float filledWidth = actualSize.X * fraction;

            if (filledWidth > 0f)
            {
                var filledEnd = new Vector2(pos.X + filledWidth, pos.Y + actualSize.Y);
                drawList.AddRectFilled(barStart, filledEnd, ImGui.GetColorU32(goldColor));
            }
        }

        // Reset to captured pos before Dummy so manual cursor shifts (e.g. vertical centering)
        // don't cause the Dummy to double-advance. No-op for normal usage.
        ImGui.SetCursorScreenPos(pos);
        ImGui.Dummy(actualSize);
    }
    public static bool DrawCategoryButton(string label, string categoryId, FontAwesomeIcon? icon = null, float spacingAfter = 5, bool disabled = false)
    {
        float scale = ImGuiHelpers.GlobalScale;

        // Setting the values of the content size (padding, spacing, etc) that way it's used across the board
        float horizontalPadding = 8 * scale;
        float verticalPadding = 4 * scale;
        float iconTextSpacing = 4 * scale;

        // These are to make sure that they're drawn in place
        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();

        // Calculate text size
        var textSize = ImGui.CalcTextSize(label);

        // Calculate icon width if present
        float iconWidth = icon.HasValue ? textSize.Y + iconTextSpacing : 0;

        // Calculate button dimensions based on content
        float contentWidth = horizontalPadding * 2 + iconWidth + textSize.X;
        float contentHeight = verticalPadding * 2 + textSize.Y;

        // Initialize category state if needed
        if (!C.Mission_Tabs.ContainsKey(categoryId))
        {
            C.Mission_Tabs[categoryId] = false;
            C.Save();
        }

        bool isExpanded = C.Mission_Tabs[categoryId];

        // Calculate interaction state
        var buttonRect = new Vector2(cursorPos.X + contentWidth, cursorPos.Y + contentHeight);
        bool isHovered = !disabled && ImGui.IsMouseHoveringRect(cursorPos, buttonRect)
                      && ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup | ImGuiHoveredFlags.ChildWindows);
        bool isClicked = isHovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        if (isClicked)
        {
            C.Mission_Tabs[categoryId] = !isExpanded;
            isExpanded = !isExpanded;
            C.Save();
        }

        // Determine colors based on state
        var headerColor = GetButtonColor(isExpanded, isHovered, disabled);
        var textColor = disabled
            ? ImGui.GetColorU32(ImGuiCol.TextDisabled)
            : ImGui.GetColorU32(ImGuiCol.Text);

        // Draw background rectangle with rounded corners (scaled)
        drawList.AddRectFilled(cursorPos, buttonRect, headerColor, 5.0f * scale);

        // Draw content with disabled color if needed
        ImGui.SetCursorScreenPos(new Vector2(cursorPos.X + horizontalPadding, cursorPos.Y + verticalPadding));

        ImGui.PushStyleColor(ImGuiCol.Text, textColor);

        // Draw icon if provided
        if (icon.HasValue)
        {
            ImGuiEx.Icon(icon.Value);
            ImGui.SameLine(0, iconTextSpacing);
        }

        ImGui.Text(label);
        ImGui.PopStyleColor();

        // Create an invisible button to properly reserve space and handle layout
        ImGui.SetCursorScreenPos(cursorPos);
        ImGui.InvisibleButton($"##{categoryId}_btn", new Vector2(contentWidth, contentHeight));

        // Add spacing after the button (scaled)
        ImGui.SameLine(0, spacingAfter * scale);

        return isExpanded;
    }
    public static uint GetButtonColor(bool isExpanded, bool isHovered, bool disabled)
    {
        if (disabled)
        {
            // Use a dimmed version of the button color for disabled state
            var baseColor = ImGui.GetColorU32(ImGuiCol.Button);
            var disabledAlpha = 0.5f; // 50% opacity
            return ImGui.ColorConvertFloat4ToU32(ImGui.ColorConvertU32ToFloat4(baseColor) with { W = disabledAlpha });
        }

        if (isHovered)
            return ImGui.GetColorU32(ImGuiCol.TabHovered);

        if (isExpanded)
            return ImGui.GetColorU32(ImGuiCol.TabActive);

        return ImGui.GetColorU32(ImGuiCol.Button);
    }
    public static void DrawImageBox(ISharedImmediateTexture texture, string? label = null, float imageZoom = 1.5f, float spacingAfter = 5)
    {
        float scale = ImGuiHelpers.GlobalScale;

        // Default coloring to match button style
        var headerColor = ImGui.GetColorU32(ImGuiCol.Button);

        // Setting the values of the content size (padding, spacing, ect)
        float horizontalPadding = 8 * scale;
        float verticalPadding = 4 * scale;
        float iconTextSpacing = 4 * scale;

        var drawList = ImGui.GetWindowDrawList();
        var cursorPos = ImGui.GetCursorScreenPos();

        // Calculate text size if label provided
        Vector2 textSize = Vector2.Zero;
        float textWidth = 0;
        if (!string.IsNullOrEmpty(label))
        {
            textSize = ImGui.CalcTextSize(label);
            textWidth = textSize.X + iconTextSpacing;
        }

        // Image size should match the text height from buttons to keep consistent height
        float imageSize = textSize.Y > 0 ? textSize.Y : ImGui.CalcTextSize("A").Y;

        // Calculating box dimensions - height should match button height
        float contentWidth = horizontalPadding * 2 + imageSize + textWidth;
        float contentHeight = verticalPadding * 2 + (textSize.Y > 0 ? textSize.Y : ImGui.CalcTextSize("A").Y);

        // Draw background rectangle with rounded corners
        drawList.AddRectFilled(cursorPos, new Vector2(cursorPos.X + contentWidth, cursorPos.Y + contentHeight), headerColor, 5.0f * scale);

        // Calculate vertical center
        float contentVerticalCenter = cursorPos.Y + (contentHeight / 2);

        // Draw image centered vertically with UV zoom
        float imageYOffset = contentVerticalCenter - (imageSize / 2);
        var imagePos = new Vector2(cursorPos.X + horizontalPadding, imageYOffset);

        // Calculate UV coordinates for zoom effect (zooms into center)
        float uvZoom = 1.0f / imageZoom;
        float uvOffset = (1.0f - uvZoom) / 2.0f;
        Vector2 uv0 = new Vector2(uvOffset, uvOffset);
        Vector2 uv1 = new Vector2(uvOffset + uvZoom, uvOffset + uvZoom);

        drawList.AddImage(texture.GetWrapOrEmpty().Handle, imagePos, new Vector2(imagePos.X + imageSize, imagePos.Y + imageSize), uv0, uv1);

        // Draw text if provided, centered vertically
        if (!string.IsNullOrEmpty(label))
        {
            float textYOffset = contentVerticalCenter - (textSize.Y / 2);
            float textXPos = cursorPos.X + horizontalPadding + imageSize + iconTextSpacing;
            ImGui.SetCursorScreenPos(new Vector2(textXPos, textYOffset));
            ImGui.Text(label);
        }

        // Create an invisible button to properly reserve space and handle layout (matches DrawCategoryButton)
        ImGui.SetCursorScreenPos(cursorPos);
        ImGui.InvisibleButton($"##imagebox_{label}", new Vector2(contentWidth, contentHeight));

        // Add spacing after the button (scaled) - matches DrawCategoryButton
        ImGui.SameLine(0, spacingAfter * scale);
    }
    public static void EndCategoryButtonRow()
    {
        ImGui.NewLine();
        ImGui.Separator();
    }
    public static void IconWithTooltip(Vector4 col, FontAwesomeIcon icon, string? tooltip = null)
    {
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, col);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();
        ImGui.PopStyleColor();

        if (tooltip != null)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(tooltip);
            }
        }
    }
    public static void IconWithTooltip(FontAwesomeIcon icon, string? tooltip = null, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();

        if (tooltip != null)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(tooltip);
            }
        }
    }
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
    public static bool SliderButton(string id, string label, ref bool v)
    {
        var pos = ImGui.GetCursorScreenPos();
        var dl = ImGui.GetWindowDrawList();

        var height = ImGui.GetFrameHeight() * 0.8f;
        var width = height * 1.8f;
        var radius = height * 0.5f;

        var storage = ImGui.GetStateStorage();
        var key = ImGui.GetID(id); // Use the fixed ID here

        var target = v ? 1f : 0f;
        var anim = Lerp(storage.GetFloat(key, target), target, ImGui.GetIO().DeltaTime * 9f);
        storage.SetFloat(key, anim);

        var pressed = ImGui.InvisibleButton($"##{id}_toggle", new Vector2(width, height));
        if (pressed)
            v = !v;

        var hov = ImGui.IsItemHovered();

        var col = v
                          ? ImGui.ColorConvertFloat4ToU32(new Vector4((hov ? 0.40f : 0.30f), (hov ? 0.90f : 0.80f), (hov ? 0.40f : 0.30f), 1f))
                          : ImGui.ColorConvertFloat4ToU32(new Vector4((hov ? 0.90f : 0.80f), (hov ? 0.40f : 0.30f), (hov ? 0.40f : 0.30f), 1f));

        dl.AddRectFilled(pos, new Vector2(pos.X + width, pos.Y + height), col, radius);

        var knobX = pos.X + radius + anim * (width - radius * 2f);
        dl.AddCircleFilled(new Vector2(knobX, pos.Y + radius), radius - 1.5f,
                           ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)));

        if (!string.IsNullOrEmpty(label))
        {
            ImGui.SameLine();
            ImGui.Text(label);
        }

        return pressed;
    }

    // Quick access functions that are used in multiple places
    public static void Draw_ExpTable(uint jobId, Vector2? Barsize = null)
    {
        var ExpInfo = CosmicHelper.Cosmic_ClassInfo();
        if (ExpInfo.TryGetValue(jobId, out var jobInfo))
        {
            foreach (var exp in jobInfo.CurrentExp.Values)
            {
                ImGui.Text($"Exp {exp.Name}: {exp.Current} / {exp.Needed}");
                if (Barsize == null)
                    Draw_XPBar(exp.Current, exp.Needed, exp.Max);
                else
                    Draw_XPBar(exp.Current, exp.Needed, exp.Max, size: Barsize);
                if (ImGui.IsItemHovered())
                {
                    using (var expTooltip = ImRaii.Tooltip())
                    {
                        ImGui.Text($"Type: {exp.Name}");
                        ImGui.Separator();

                        ImGui.Text($"Current: {exp.Current}");
                        ImGui.Text($"Need: {exp.Needed}");
                        ImGui.Text($"Max: {exp.Max}");
                    }
                }
            }
        }
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
    public static void Table_FontCenter(FontAwesomeIcon icon)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(icon.ToIconString());
        ImGui.PopFont();
    }
}
