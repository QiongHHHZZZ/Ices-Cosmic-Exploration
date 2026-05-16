using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICE.Scheduler
{
    internal static class Task_HubActivities
    {
        public static bool RepairNpc = false;
        public static bool RelicTurnin = false;
        public static bool CosmoBuy = false;
        public static bool CanGamba = false;
        public static bool CanBuyDrones = false;
        private static Vector3 craftingSpot = Vector3.Zero;
        private static bool blockNextHubReentry = false;
        private static long blockNextHubReentryUntil = 0;

        public static void Enqueue()
        {
            P.TaskManager.Enqueue(RegisterCraftingPosition, "Registering crafting position for later");
            P.TaskManager.Enqueue(Task_Repair.HubCheck, "Checking to see if we're in hub area");
            if (RepairNpc)
            {
                P.TaskManager.EnqueueMulti
                (
                    new(() => IceLogging.Info("Starting repair task at the npc", "Task_HubActivities")),
                    new(Task_Repair.Repair_PathTo, "Pathing to the repair NPC"),
                    new(Task_Repair.RepairAtNpc, "Repairing at the NPC Vendor"),
                    new(Task_Repair.CloseRepair, "Closing the repair window")
                );
            }
            if (RelicTurnin)
            {
                P.TaskManager.Enqueue(() => IceLogging.Info("Starting Relic Turnin task at the npc", "Task_HubActivities"));
                Task_RelicTurnin.Enqueue();
                P.TaskManager.Enqueue(() => IceLogging.Info("Task_Relic turnin is Complete"));
            }
            if (CosmoBuy)
            {
                P.TaskManager.Enqueue(() => IceLogging.Info("Starting Relic Turnin task at the npc", "Task_HubActivities"));;
                Task_BuyCosmoItems.Enqueue();
            }
            if (CanGamba)
            {
                P.TaskManager.Enqueue(() => IceLogging.Info("Starting Gamba task at the npc", "Task_HubActivities"));
                Task_Gamba.Enqueue();
            }
            if (CanBuyDrones)
            {
                P.TaskManager.Enqueue(() => IceLogging.Info("Starting the drone buying", "Task_HubActivities"));
                Task_ArtifactSearch.EnqueueBuy();
            }
            P.TaskManager.EnqueueMulti
            (
                new(ArmPostHubReentryGuard, "Arming hub reentry guard"),
                new(() => ResetAll(), "Setting all task to false"),
                new(() => IceLogging.Info("Checking to see if we need to path back to the spot")),
                new(PathBackToCraftingSpot, "Pathing back to our crafting spot", Utils.TaskConfig),
                new(() => SchedulerMain.State = IceState.Start, "Swapping back to start")
            );
        }

        public static bool? RegisterCraftingPosition()
        {
            if (CosmicHelper.CrafterJobList.Contains((uint)Player.Job))
                craftingSpot = Player.Position;

            return true;
        }

        public static bool? PathBackToCraftingSpot()
        {
            if (CosmicHelper.CrafterJobList.Contains((uint)Player.Job))
            {
                if (TryDailyRoutinesTeleportToCraftingSpot(craftingSpot))
                    return false;

                if (!Task_NavmeshMove.Task_NavTo(craftingSpot, true, 1, false).Value)
                {
                    return false;
                }
                else
                {
                    IceLogging.Debug("We're back at our spot, so continuing on");
                    return true;
                }
            }
            else
            {
                IceLogging.Info($"We're not on a crafting job. (Allegedly) which means that we don't need to path back | Player Job: {(uint)Player.Job}");
                return true;
            }
        }

        private static bool TryDailyRoutinesTeleportToCraftingSpot(Vector3 destination)
        {
            const string tag = "[Task_HubActivities: ReturnSpot TP]";

            if (!C.HubReturnUseDailyRoutinesTP)
                return false;

            if (Player.DistanceTo(destination) < 3f)
                return false;

            if (!Utils.HasPlugin("DailyRoutines"))
            {
                if (EzThrottler.Throttle("HubReturnMissingDailyRoutines", 8000))
                    IceLogging.Warning("未检测到 Daily Routines，已回退原有寻路。", tag);
                return false;
            }

            if (EzThrottler.Throttle("HubReturnDailyRoutinesTeleport", 2500))
            {
                var command = string.Format(
                    CultureInfo.InvariantCulture,
                    "/pdrtp pos {0:F2} {1:F2} {2:F2}",
                    destination.X,
                    destination.Y,
                    destination.Z);

                Svc.Commands.ProcessCommand(command);
                IceLogging.Debug($"已尝试 Daily Routines 传送：{command}", tag);
                return true;
            }

            return false;
        }

        public static void ApplyPostHubReentryGuard(ref bool canBuyDrones, ref bool canGamba)
        {
            if (!blockNextHubReentry)
                return;

            if (Environment.TickCount64 > blockNextHubReentryUntil)
            {
                blockNextHubReentry = false;
                return;
            }

            if (!(canBuyDrones || canGamba))
                return;

            canBuyDrones = false;
            canGamba = false;
            blockNextHubReentry = false;

            IceLogging.Info("已拦截一次 Hub 立即重入（抽奖/无人机），避免 TP 回点后立刻再返回基地。", "[Task_HubActivities: Reentry Guard]");
        }

        private static bool? ResetAll()
        {
            IceLogging.Info("Resetting all hub task to false");

            RepairNpc = false;
            RelicTurnin = false;
            CosmoBuy = false;
            CanGamba = false;
            CanBuyDrones = false;
            CosmicHelper.Task_UpdateRelicMissionInfo();

            return true;
        }

        private static bool? ArmPostHubReentryGuard()
        {
            if (CanGamba || CanBuyDrones)
            {
                blockNextHubReentry = true;
                blockNextHubReentryUntil = Environment.TickCount64 + 8000;
            }

            return true;
        }
    }
}
