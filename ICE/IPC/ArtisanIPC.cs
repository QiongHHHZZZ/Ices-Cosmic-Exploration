using ECommons.EzIpcManager;
using ECommons.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using static ICE.ConfigFiles.Config.MissionSettings;

namespace ICE.IPC
{
    public class ArtisanIPC
    {
        public const string Name = "Artisan";
        public ArtisanIPC() => EzIPC.Init(this, Name, SafeWrapper.AnyException);

        [EzIPC] public Func<bool> IsBusy;
        [EzIPC] public Func<bool> GetEnduranceStatus;
        [EzIPC] public Action<bool> SetEnduranceStatus;
        [EzIPC] public Func<bool> IsListRunning;
        [EzIPC] public Func<bool> IsListPaused;
        [EzIPC] public Action<bool> SetListPause;
        [EzIPC] public Func<bool> GetStopRequest;
        [EzIPC] public Action<bool> SetStopRequest;
        [EzIPC] public Action<ushort, int> CraftItem;
        [EzIPC] public Action<ushort, uint, uint, uint, uint> AssignRecipie;

        [EzIPC] public Action<uint, string, bool> ChangeSolver;
        [EzIPC] public Action<uint> SetTempSolverBackToNormal;

        [EzIPC] public Action<uint, uint, bool, bool> ChangeFood;
        [EzIPC] public Action<uint> SetTempFoodBackToNormal;

        [EzIPC] public Action<uint, uint, bool, bool> ChangePotion;
        [EzIPC] public Action<uint> SetTempPotionBackToNormal;

        [EzIPC] public Action<uint, uint, bool> ChangeManual;
        [EzIPC] public Action<uint> SetTempManualBackToNormal;

        [EzIPC] public Action<uint, uint, bool> ChangeSquadronManual;
        [EzIPC] public Action<uint> SetTempSquadronManualBackToNormal;

        [EzIPC] public Action<uint, uint, bool> ChangeExpertProfileID;
        [EzIPC] public Action<uint> SetTempExpertProfileIDBackToNormal;

        [EzIPC] public Action<uint, uint, bool> ChangeExpertMaxSteadyUses;
        [EzIPC] public Action<uint> SetTempExpertMaxSteadyUsesBackToNormal;

        [EzIPC] public Action<uint, uint, bool> ChangeExpertMaxMaterialMiracleUses;
        [EzIPC] public Action<uint> SetTempExpertMaxMaterialMiracleUsesBackToNormal;

        [EzIPC] public Action<uint, uint, bool> ChangeExpertMinimumStepsBeforeMiracle;
        [EzIPC] public Action<uint> SetTempExpertMinimumStepsBeforeMiracleBackToNormal;

        [EzIPC] public Action<uint, bool> ChangeStandardMaxMaterialMiracleUses;
        [EzIPC] public Action SetTempStandardMaxMaterialMiracleUsesBackToNormal;

        [EzIPC] public Action<uint, bool> ChangeStandardMinimumStepsBeforeMiracle;
        [EzIPC] public Action SetTempStandardMinimumStepsBeforeMiracleBackToNormal;

        private void AssignArtisanRecipe(uint recipeId, ArtisanSettings recipeConfig, bool isExpert)
        {
            if (recipeConfig.ArtisanSolverType != ArtisanCraftType.Default)
            {
                string ArtisanType = recipeConfig.ArtisanSolverType switch
                {
                    ArtisanCraftType.Standard => "Standard Recipe Solver",
                    ArtisanCraftType.ProgressOnly => "Progress Only Solver",
                    ArtisanCraftType.Raphael => "Raphael Recipe Solver",
                    ArtisanCraftType.Macro => $"Macro: {recipeConfig.MacroName}",
                    ArtisanCraftType.Expert => "Expert Recipe Solver",
                    _ => "Standard Recipe Solver",
                };
                ChangeSolver(recipeId, ArtisanType, true);
            }
            else
            {
                SetTempSolverBackToNormal(recipeId);
            }

            if (recipeConfig.FoodId != 0)
                ChangeFood(recipeId, recipeConfig.FoodId, recipeConfig.FoodHQ, true);
            else
                SetTempFoodBackToNormal(recipeId);

            if (recipeConfig.PotionId != 0)
                ChangePotion(recipeId, recipeConfig.PotionId, recipeConfig.PotionHQ, true);
            else
                SetTempPotionBackToNormal(recipeId);

            if (recipeConfig.ManualId != 0)
                ChangeManual(recipeId, recipeConfig.ManualId, true);
            else
                SetTempManualBackToNormal(recipeId);

            if (recipeConfig.SquadronManualId != 0)
                ChangeSquadronManual(recipeId, recipeConfig.SquadronManualId, true);
            else
                SetTempSquadronManualBackToNormal(recipeId);

            if (isExpert)
            {
                if (recipeConfig.SkillUsageAmount != -1)
                {
                    ChangeExpertMaxSteadyUses(recipeId, (uint)recipeConfig.SkillUsageAmount, true);
                    ChangeExpertMaxMaterialMiracleUses(recipeId, (uint)recipeConfig.SkillUsageAmount, true);
                }
                else
                {
                    SetTempExpertMaxSteadyUsesBackToNormal(recipeId);
                    SetTempExpertMaxMaterialMiracleUsesBackToNormal(recipeId);
                }

                if (recipeConfig.MinStepsForMiracle != -1)
                    P.Artisan.ChangeExpertMinimumStepsBeforeMiracle(recipeId, (uint)recipeConfig.MinStepsForMiracle, true);
                else
                    P.Artisan.SetTempExpertMinimumStepsBeforeMiracleBackToNormal(recipeId);
            }
            else
            {
                if (recipeConfig.SkillUsageAmount != -1)
                {
                    ChangeStandardMaxMaterialMiracleUses((uint)recipeConfig.SkillUsageAmount, true);
                }
                else
                {
                    SetTempStandardMaxMaterialMiracleUsesBackToNormal();
                }

                if (recipeConfig.MinStepsForMiracle != -1)
                {
                    ChangeStandardMinimumStepsBeforeMiracle((uint)recipeConfig.MinStepsForMiracle, true);
                }
                else
                {
                    SetTempStandardMinimumStepsBeforeMiracleBackToNormal();
                }
            }
        }

        private static bool ArtisanSettingsEqual(ArtisanSettings a, ArtisanSettings b) =>
            a.UseGlobal == b.UseGlobal &&
            a.FoodId == b.FoodId &&
            a.FoodHQ == b.FoodHQ &&
            a.PotionId == b.PotionId &&
            a.PotionHQ == b.PotionHQ &&
            a.ManualId == b.ManualId &&
            a.SquadronManualId == b.SquadronManualId &&
            a.ArtisanSolverType == b.ArtisanSolverType &&
            a.MacroName == b.MacroName &&
            a.SkillUsageAmount == b.SkillUsageAmount &&
            a.MinStepsForMiracle == b.MinStepsForMiracle &&
            a.ExpertProfileId == b.ExpertProfileId;

        private Dictionary<ushort, ArtisanSettings> CraftSettings = new();

        public void CheckArtisanSettings(ushort recipeId, uint missionId, bool isExpert, bool isLeveling = false)
        {
            if (!C.MissionConfig.TryGetValue(missionId, out var configInfo))
                return;
            if (!configInfo.CraftSettings.TryGetValue(recipeId, out var recipeSettings))
                return;

            // Resolve the effective settings — global profile or per-recipe config
            ArtisanSettings effectiveSettings;
            if (recipeSettings.UseGlobal)
            {
                var global = isExpert ? C.Artisan_GlobalExpert : C.Artisan_GlobalStandard;
                effectiveSettings = new ArtisanSettings
                {
                    UseGlobal = true,
                    FoodId = global.FoodId,
                    FoodHQ = global.FoodHQ,
                    PotionId = global.PotionId,
                    PotionHQ = global.PotionHQ,
                    ManualId = global.ManualId,
                    SquadronManualId = global.SquadronManual,
                    ArtisanSolverType = global.SolverType,
                    // MacroName, SkillUsageAmount, MinStepsForMiracle, ExpertProfileId
                    // I'm not using these in the global, so they're just getting defaulted
                };
            }
            else
            {
                effectiveSettings = recipeSettings;
            }

            if (isLeveling)
                effectiveSettings.ArtisanSolverType = ArtisanCraftType.ProgressOnly;

            if (CraftSettings.TryGetValue(recipeId, out var cached) && ArtisanSettingsEqual(cached, effectiveSettings))
                return;

            AssignArtisanRecipe(recipeId, effectiveSettings, isExpert);
            CraftSettings[recipeId] = new ArtisanSettings
            {
                UseGlobal = effectiveSettings.UseGlobal,
                FoodId = effectiveSettings.FoodId,
                FoodHQ = effectiveSettings.FoodHQ,
                PotionId = effectiveSettings.PotionId,
                PotionHQ = effectiveSettings.PotionHQ,
                ManualId = effectiveSettings.ManualId,
                SquadronManualId = effectiveSettings.SquadronManualId,
                ArtisanSolverType = effectiveSettings.ArtisanSolverType,
                MacroName = effectiveSettings.MacroName,
                SkillUsageAmount = effectiveSettings.SkillUsageAmount,
                MinStepsForMiracle = effectiveSettings.MinStepsForMiracle,
                ExpertProfileId = effectiveSettings.ExpertProfileId,
            };
        }

        public bool UpdatedArtisan()
        {
            if (DalamudReflector.TryGetDalamudPlugin($"Artisan", out var plogon, false, true))
            {
                if (plogon.GetType().Assembly.GetName().Version < new Version(4, 0, 4, 45))
                    return false;

                return true;
            }

            return false;
        }
    }
}
