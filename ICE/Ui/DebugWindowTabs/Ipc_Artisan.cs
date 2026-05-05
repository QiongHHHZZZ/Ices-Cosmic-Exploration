using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ipc_Artisan
    {
        internal static List<ItemInfo> PotentialCrafterFood = new();
        internal static List<ItemInfo> PotentialPots = new();
        internal static List<ItemInfo> PotentialManuals = new();
        internal static List<ItemInfo> PotentialSquadronManuals = new();

        internal static ItemInfo SelectedFood = new();
        internal static ItemInfo SelectedPot = new();
        internal static ItemInfo SelectedManual = new();
        internal static ItemInfo SelectedSquadronManual = new();

        public class ItemInfo
        {
            public uint Id { get; set; } = 0;
            public string Name { get; set; } = "";
        }

        internal static uint RecipeId = 0;
        private static uint MaxSkillUsage = 0;

        public static void Draw()
        {
            ImGui.Text(T("Selected Food: [{0}] {1}", SelectedFood.Id, SelectedFood.Name));
            ImGui.SameLine();
            if (ImGui.Button(T("Select Food")))
            {
                PotentialCrafterFood.Clear();
                foreach (var food in ConsumableInfo.CrafterFood)
                {
                    if (PlayerHelper.GetItemCount(food.Id, out var count) && count > 0)
                    {
                        PotentialCrafterFood.Add(new() { Id = food.Id, Name = food.Name });
                    }
                }
                ImGui.OpenPopup("Select Crafter Food");
            }

            if (ImGui.BeginPopup("Select Crafter Food"))
            {
                foreach (var item in PotentialCrafterFood)
                {
                    if (ImGui.Selectable($"{item.Name}"))
                        SelectedFood = item;
                }
                ImGui.EndPopup();
            }

            ImGui.Text(T("Selected Pot: [{0}] {1}", SelectedPot.Id, SelectedPot.Name));
            ImGui.SameLine();
            if (ImGui.Button(T("Select Pot")))
            {
                PotentialPots.Clear();
                foreach (var pot in ConsumableInfo.Pots)
                {
                    if (PlayerHelper.GetItemCount(pot.Id, out var count))
                    {
                        PotentialPots.Add(new() { Id = pot.Id, Name = pot.Name });
                    }
                }
                ImGui.OpenPopup("Select Pot");
            }

            if (ImGui.BeginPopup("Select Pot"))
            {
                foreach (var item in PotentialPots)
                {
                    if (ImGui.Selectable($"{item.Name}"))
                        SelectedPot = item;
                }
                ImGui.EndPopup();
            }

            ImGui.Text(T("Selected Manual: [{0}] {1}", SelectedManual.Id, SelectedManual.Name));
            ImGui.SameLine();
            if (ImGui.Button(T("Select Manual")))
            {
                PotentialManuals.Clear();
                foreach (var manual in ConsumableInfo.Manuals)
                {
                    if (PlayerHelper.GetItemCount(manual.Id, out var count))
                    {
                        PotentialManuals.Add(new() { Id = manual.Id, Name = manual.Name });
                    }
                }
                ImGui.OpenPopup("Select Manual");
            }

            if (ImGui.BeginPopup("Select Manual"))
            {
                foreach (var item in PotentialManuals)
                {
                    if (ImGui.Selectable($"{item.Name}"))
                        SelectedManual = item;
                }
                ImGui.EndPopup();
            }

            ImGui.Text(T("Selected Squadron Manual: [{0}] {1}", SelectedSquadronManual.Id, SelectedSquadronManual.Name));
            ImGui.SameLine();
            if (ImGui.Button(T("Select Squadron Manual")))
            {
                PotentialSquadronManuals.Clear();
                foreach (var manual in ConsumableInfo.SquadronManuals)
                {
                    if (PlayerHelper.GetItemCount(manual.Id, out var count))
                    {
                        PotentialSquadronManuals.Add(new() { Id = manual.Id, Name = manual.Name });
                    }
                }
                ImGui.OpenPopup("Select Squadron Manual");
            }

            if (ImGui.BeginPopup("Select Squadron Manual"))
            {
                foreach (var item in PotentialSquadronManuals)
                {
                    if (ImGui.Selectable($"{item.Name}"))
                        SelectedSquadronManual = item;
                }
                ImGui.EndPopup();
            }

            ImGui.Separator();
            ImGui.InputUInt(T("Recipe Id"), ref RecipeId);

            if (ImGui.Button(T("Reset Temp")))
            {
                P.Artisan.SetTempFoodBackToNormal(RecipeId);
                P.Artisan.SetTempPotionBackToNormal(RecipeId);
                P.Artisan.SetTempManualBackToNormal(RecipeId);
                P.Artisan.SetTempSquadronManualBackToNormal(RecipeId);
            }

            if (ImGui.Button(T("Food [HQ]")))
            {
                P.Artisan.ChangeFood(RecipeId, SelectedFood.Id, true, true);
            }
            ImGui.SameLine();
            if (ImGui.Button(T("Food [NQ]")))
            {
                P.Artisan.ChangeFood(RecipeId, SelectedFood.Id, false, true);
            }

            if (ImGui.Button(T("Potion [HQ]")))
            {
                P.Artisan.ChangePotion(RecipeId, SelectedPot.Id, true, true);
            }
            ImGui.SameLine();
            if (ImGui.Button(T("Potion [NQ]")))
            {
                P.Artisan.ChangePotion(RecipeId, SelectedPot.Id, false, true);
            }

            if (ImGui.Button(T("Manual")))
            {
                P.Artisan.ChangeManual(RecipeId, SelectedManual.Id, true);
            }
            ImGui.SameLine();
            if (ImGui.Button(T("Squad Manual")))
            {
                P.Artisan.ChangeSquadronManual(RecipeId, SelectedSquadronManual.Id, true);
            }

            var mission = CosmicHelper.SheetMissionDict.FirstOrNull(kvp => kvp.Value.Crafts_Main.ContainsKey((ushort)RecipeId)
                                                                    || kvp.Value.Crafts_Pre.ContainsKey((ushort)RecipeId));

            if (mission != null)
            {
                var sheetInfo = mission.Value.Value;

                if (sheetInfo.TemporaryActionCount != 0)
                {
                    var actionInfo = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(sheetInfo.TemporaryActionId);
                    var name = actionInfo.Name;
                    var icon = Svc.Texture.GetFromGameIcon((int)actionInfo.Icon).GetWrapOrEmpty();
                    ImGui.Image(icon.Handle, new(24, 24));
                    ImGui.AlignTextToFramePadding();
                    ImGui.SameLine();
                    ImGui.Text($"{name}");

                    ImGui.SliderUInt(T("Max Usage"), ref MaxSkillUsage, 0, 2);
                    if (ImGui.Button(T("Apply Temp")))
                    {
                        if (sheetInfo.TemporaryActionId == 41269)
                        {
                            P.Artisan.ChangeExpertMaxMaterialMiracleUses(RecipeId, MaxSkillUsage, false);
                        }
                    }
                }
            }

        }
    }
}
