using Dalamud.Interface;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Ui.MainUi.Settings
{
    internal class Shop_Tokens
    {
        public static void Draw()
        {
            var enableBooklet = C.BookletBuy_Enable;
            var bookletAmount = C.BookletBuy_Amount;

            var enableMountBuy = C.PlanetMount_Enable;
            var mountBuyAmount = C.PlanetMount_Amount;

            if (ImGui.Checkbox(T("Buy Booklets"), ref enableBooklet))
            {
                C.BookletBuy_Enable = enableBooklet;
                C.SaveDebounced();
            }
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt(T("Buy Booklets @"), ref bookletAmount))
            {
                if (bookletAmount > 99)
                {
                    C.BookletBuy_Amount = bookletAmount;
                    C.SaveDebounced();
                }
            }

            if (ImGui.Checkbox(T("Buy Mounts"), ref enableMountBuy))
            {
                C.PlanetMount_Enable = enableMountBuy;
                C.SaveDebounced();
            }
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt(T("Buy Mount @"), ref mountBuyAmount))
            {
                if (mountBuyAmount > 59)
                {
                    C.PlanetMount_Amount = mountBuyAmount;
                    C.SaveDebounced();
                }
            }

            if (ImGui.BeginTable("Mount Token Info", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn(T("Planet"));
                ImGui.TableSetupColumn(T("Tokens"));
                ImGui.TableSetupColumn(T("Booklets"));
                ImGui.TableSetupColumn(T("Mount"));
                ImGui.TableSetupColumn(T("Unlocked"));

                ImGui.TableHeadersRow();

                foreach (var entry in CosmicMoonRegistry.TokenIds)
                {
                    var planet = CosmicMoonRegistry.ByTerritoryId[entry.Key];
                    var planetIcon = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), planet.IconResource).GetWrapOrEmpty();

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui_Ice.ImageButtonWithText(planetIcon, T(planet.DisplayName), planet.DisplayName, new(24, 24));

                    ImGui.TableNextColumn();
                    if (ExcelHelper.ItemSheet.TryGetRow(entry.Value.tokenId, out var tokenSheet))
                    {
                        if (Svc.Texture.TryGetFromGameIcon((int)tokenSheet.Icon, out var icon) && PlayerHelper.GetItemCount(entry.Value.tokenId, out var count))
                        {
                            ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), $"{count:N0}", $"{tokenSheet.Name}", new(24));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Item ID: {0}", entry.Value.tokenId));
                                ImGui.Text(T("Name: {0}", tokenSheet.Name));
                                ImGui.EndTooltip();
                            }
                        }
                    }

                    ImGui.TableNextColumn();
                    if (ExcelHelper.ItemSheet.TryGetRow(entry.Value.bookletId, out var bookletSheet))
                    {
                        if (Svc.Texture.TryGetFromGameIcon((int)bookletSheet.Icon, out var icon) && PlayerHelper.GetItemCount(entry.Value.bookletId, out var count))
                        {
                            ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), $"{count:N0}", $"{bookletSheet.Name}", new(24));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Item ID: {0}", entry.Value.bookletId));
                                ImGui.Text(T("Name: {0}", bookletSheet.Name));
                                ImGui.EndTooltip();
                            }
                        }
                    }

                    ImGui.TableNextColumn();
                    if (ExcelHelper.ItemSheet.TryGetRow(entry.Value.mountId, out var mountSheet))
                    {
                        if (Svc.Texture.TryGetFromGameIcon((int)mountSheet.Icon, out var icon) && PlayerHelper.GetItemCount(entry.Value.mountId, out var count))
                        {
                            ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), $"{count:N0}", $"{mountSheet.Name}", new(24));
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text(T("Item ID: {0}", entry.Value.mountId));
                                ImGui.Text(T("Name: {0}", mountSheet.Name));
                                ImGui.EndTooltip();
                            }
                        }
                    }

                    ImGui.TableNextColumn();
                    bool unlocked = UnlockState.IsItemUnlocked(mountSheet);
                    var fontIcon = unlocked ? FontAwesomeIcon.Check : FontAwesomeIcon.Times;
                    var color = unlocked ? EColor.Green : EColor.Red;
                    ImGuiEx.Icon(color, fontIcon);
                }

                ImGui.EndTable();
            }

        }
    }
}
