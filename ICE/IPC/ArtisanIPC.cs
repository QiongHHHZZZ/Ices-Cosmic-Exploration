using ECommons.EzIpcManager;
using ECommons.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

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

        public void AssignArtisanRecipe(ushort recipeId, uint reqFood, uint reqPotion = 0, uint reqManual = 0, uint reqSquadronManual = 0)
        {
            P.Artisan.AssignRecipie(recipeId, reqFood, reqPotion, reqManual, reqSquadronManual);
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
