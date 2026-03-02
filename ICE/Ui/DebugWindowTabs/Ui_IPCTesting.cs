using ECommons.ExcelServices.TerritoryEnumeration;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_IPCTesting
    {
        private static int Radius = 10;
        private static int XLoc = 0;
        private static int YLoc = 0;
        private static string PandoraFeature = "";
        private static int amount = 1000;

        private static string importString = new string('\0', 2048); // Pre-allocate buffer
        private static string SwapToPreset = string.Empty;
        private static uint missionId = 0;
        private static uint baitId = 0;
        private static bool baitSwapped = false;

        private static string SettingChange = "";
        private static bool SettingState = false;

        public static unsafe void Draw()
        {
            ImGui.Text($"Artisan Is Busy? {P.Artisan.IsBusy()}");
            ImGui.Text($"{EzThrottler.GetRemainingTime("[Main Item(s)] Starting Main Craft")}");
            if (ImGui.Button(T("Artisan, craft this")))
            {
                P.Artisan.CraftItem(36026, 1);
            }

            ImGui.SetNextItemWidth(125);
            ImGui.InputInt(T("Radius"), ref Radius);
            ImGui.SetNextItemWidth(125);
            ImGui.InputInt(T("X Location"), ref XLoc);
            ImGui.SetNextItemWidth(125);
            ImGui.InputInt(T("Y Location"), ref YLoc);

            if (ImGui.Button($"Test Radius"))
            {
                var agent = AgentMap.Instance();

                Utils.SetGatheringRing(agent->CurrentTerritoryId, XLoc, YLoc, Radius);
            }

            ImGui.Separator();
            ImGui.InputText(T("Pandora Feature"), ref PandoraFeature);
            if (ImGui.Button(T("Pause Feature")))
            {
                P.Pandora.PauseFeature(PandoraFeature, amount);
            }

            ImGui.Separator();
            ImGui.Text(T("AutoHook"));
            ImGui.SetNextItemWidth(150);
            ImGui.InputText(T("Preset String"), ref importString, 2048);
            if (ImGui.Button(T("Import")))
            {
                P.AutoHook.ImportAndSelectPreset(importString);
                importString = string.Empty;
            }
            ImGui.SetNextItemWidth(150);
            ImGui.InputText(T("Swap to preset"), ref SwapToPreset);
            if (ImGui.Button(T("Swap")))
            {
                P.AutoHook.SetPreset(SwapToPreset);
            }
            if (ImGui.Button(T("Apply Temp")))
            {
                P.AutoHook.CreateAndSelectAnonymousPreset(importString);
            }
            ImGui.SetNextItemWidth(200);
            ImGui.InputUInt("Select mission to import", ref missionId);
            ImGui.InputUInt("Bait ID", ref baitId);
            if (ImGui.Button(T("Swap to bait")))
            {
                 SwapBait(baitId);
            }
            if (ImGui.Button(T("Swap Bait... simple")))
            {
                if (CosmicHelper.CurrentBait == 0)
                {
                    IceLogging.Debug("Bait is not currently equipped");
                }

                P.AutoHook.SwapBaitById(baitId);
            }
            if (ImGui.Button(T("Stupid Test")))
            {
                if (CosmicHelper.CurrentBait == 0)
                {
                    IceLogging.Debug($"No bait is equipped");
                }
                else if (CosmicHelper.CurrentBait == null)
                {
                    IceLogging.Debug("Bait is null... aka not in the middle of a mission");
                }
                else
                {
                    IceLogging.Debug($"Current bait: {CosmicHelper.CurrentBait}");
                }
            }

            if (ImGui.Button(T("Enable AutoHook")))
            {
                P.AutoHook.SetPluginState(true);
            }
            if (ImGui.Button(T("Disable Autohook")))
            {
                P.AutoHook.SetPluginState(false);
            }

            ImGui.Separator();
            ImGui.Text($"Is ICE Running? | {P.IceIpc.IsRunning()}");
            if (ImGui.Button(T("Only Missions Via IPC")))
            {
                HashSet<uint> missionListIds = new() { 1, 3, 4, 7, 9, 11 };
                P.IceIpc.OnlyMissions(missionListIds);
            }
            if (ImGui.Button(T("Change to gamba")))
            {
                SchedulerMain.State = IceState.Gambling;
            }

            ImGui.Separator();

            ImGui.SetNextItemWidth(150);
            ImGui.InputText(T("Setting Name"), ref SettingChange);
            ImGui.Checkbox(T("Setting Bool"), ref SettingState);

            if (ImGui.Button(T("Toggle Setting")))
            {
                P.IceIpc.ChangeSetting(SettingChange, SettingState);
            }

            if (ImGui.Button(T("Assign Artisan Food Test")))
            {
                P.Artisan.AssignArtisanRecipe(48797, 46253);
            }
            if (ImGui.Button(T("Set temp setting")))
            {
                P.Artisan.ChangeSolver(37084, "Progress Only Solver", true);
            }
            if (ImGui.Button(T("Set raphael solver")))
            {
                P.Artisan.ChangeSolver(37084, "Raphael Recipe Solver", true);
            }
            if (ImGui.Button(T("Set current mission to Raphael")))
            {
                if (CosmicHelper.CurrentLunarMission != 0)
                {
                    foreach (var craftItem in CosmicHelper.CurrentMissionInfo.Crafts_Main)
                    {
                        P.Artisan.ChangeSolver(craftItem.Value.RecipeId, "Raphael Recipe Solver", true);
                    }
                    foreach (var preCraft in CosmicHelper.CurrentMissionInfo.Crafts_Pre)
                    {
                        P.Artisan.ChangeSolver(preCraft.Value.RecipeId, "Raphael Recipe Solver", true);
                    }
                }
            }
            if (ImGui.Button(T("Set current mission to Progress")))
            {
                if (CosmicHelper.CurrentLunarMission != 0)
                {
                    foreach (var craftItem in CosmicHelper.CurrentMissionInfo.Crafts_Main)
                    {
                        P.Artisan.ChangeSolver(craftItem.Value.RecipeId, "Progress Only Solver", true);
                    }
                    foreach (var preCraft in CosmicHelper.CurrentMissionInfo.Crafts_Pre)
                    {
                        P.Artisan.ChangeSolver(preCraft.Value.RecipeId, "Progress Only Solver", true);
                    }
                }
            }
            if (ImGui.Button(T("Return back to normal")))
            {
                if (CosmicHelper.CurrentLunarMission != 0)
                {
                    foreach (var craftItem in CosmicHelper.CurrentMissionInfo.Crafts_Main)
                    {
                        P.Artisan.SetTempSolverBackToNormal(craftItem.Value.RecipeId);
                    }
                    foreach (var preCraft in CosmicHelper.CurrentMissionInfo.Crafts_Pre)
                    {
                        P.Artisan.SetTempSolverBackToNormal(preCraft.Value.RecipeId);
                    }
                }
            }
        }

        private static void SwapBait(uint baitId)
        {
            _ = Task.Run(async () =>
            {
                baitSwapped = await TaskSwapBait(baitId);
            });

            _ = Task.Run(async () =>
            {
                await P.AutoHook.SwapBaitById(baitId);
            });
        }

        private static async Task<bool> TaskSwapBait(uint bait)
        {
            return await P.AutoHook.SwapBaitById(bait);
        }
    }
}
