using ECommons.GameHelpers;
using ICE.ConfigFiles;
using System;
using System.Collections.Generic;
using System.Text;
using static ICE.ConfigFiles.Config;

namespace ICE.Utilities
{
    public static class Char_Info
    {
        private static Config G => C; // just an alias for readability
        private static ulong CID => Player.CID != 0 ? (ulong)Player.CID : 0UL;

        private static CharacterOverride? Ov => C.CharacterOverrides.GetValueOrDefault(CID);

        // Repair
        public static bool SelfRepairGather => Ov?.SelfRepairGather ?? G.SelfRepairGather;
        public static bool SelfRepairCrafter => Ov?.SelfRepairCrafter ?? G.SelfRepairCrafter;
        public static bool RepairAtVendor => Ov?.RepairAtVendor ?? G.RepairAtVendor;
        public static int RepairPercent => Ov?.RepairPercent ?? G.RepairPercent;
        public static bool RepairAllGear => Ov?.RepairAllGear ?? G.RepairAllGear;
        public static bool Stop_DarkMatter => G.Stop_DarkMatter;
        public static int Minimum_DarkMatter => Ov?.Minimum_DarkMatter ?? G.Minimum_DarkMatter;

        // Artisan
        public static Global_Artisan Artisan_GlobalStandard => Ov?.Artisan_GlobalStandard ?? G.Artisan_GlobalStandard;
        public static Global_Artisan Artisan_GlobalExpert => Ov?.Artisan_GlobalExpert ?? G.Artisan_GlobalExpert;

        // Mount — the four radius/usage ones are global-only
        public static uint MountId => Ov?.MountId ?? G.MountId;
        public static string MountName => Ov?.MountName ?? G.MountName;
        public static bool UseMountOutsideMission => G.UseMountOutsideMission;
        public static bool UseMountInMission => G.UseMountInMission;
        public static float MountRadius => G.MountRadius;
        public static float DismountRadius => G.DismountRadius;

        // Relic
        public static bool Relic_SwapJob => Ov?.Relic_SwapJob ?? G.Relic_SwapJob;
        public static uint Relic_BattleJob => Ov?.Relic_BattleJob ?? G.Relic_BattleJob;
    }
}
