using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Scheduler.Tasks
{
    internal class Task_RelicTurnin
    {
        public static uint TurninJob = 0;

        public static void Enqueue()
        {
            P.TaskManager.EnqueueMulti
            (
                new(RegisterJob, "Register Job Swap Class"),
                new(CheckJobSwap, "Checking to see if we need to swap jobs", Utils.TaskConfig),
                new(Relic_PathTo, "Heading to the relic NPC for turnin"),
                new(TalkToResearchWay, "Talk to researchway"),
                new(SelectReport, "Selecting Report", Utils.TaskConfig),
                new(SelectRelicClass, "Selecting the class to turnin on", Utils.TaskConfig)
            );
        }
        public static bool? RegisterJob()
        {
            IceLogging.Verbose("Registering what job to turn in on");
            TurninJob = (uint)Player.Job;

            return true;
        }

        public static bool? CheckJobSwap()
        {
            if (C.Relic_SwapJob && C.Relic_BattleJob != 0)
            {
                if (Player.Job != (Job)C.Relic_BattleJob)
                {
                    if (EzThrottler.Throttle("Swapping jobs", 1000))
                    {
                        IceLogging.Verbose($"Telling the game to swap you to jobID: {C.Relic_BattleJob}");
                        GearsetHandler.TaskClassChange((Job)C.Relic_BattleJob);
                    }

                    return false;
                }
                else
                {
                    IceLogging.Verbose("Job swap is complete! Turning in the relic now");

                    return true;
                }
            }
            else if ((uint)Player.Job != Mission_Settings.SelectedJob)
            {
                if (EzThrottler.Throttle("Swapping jobs", 1000))
                {
                    IceLogging.Verbose($"Telling the game to swap you to jobID: {Mission_Settings.SelectedJob}");
                    GearsetHandler.TaskClassChange((Job)Mission_Settings.SelectedJob);
                }
                return false;
            }
            else
            {
                IceLogging.Debug("No swap is necessary/not configured properly. Continuing on");
                return true;
            }
        }

        public static bool? Relic_PathTo()
        {
            string handle = "[Task_Relic: PathTo]";
            var zoneId = Player.Territory;
            var npcEntry = NpcData.MoonNpcs[zoneId.RowId].Where(x => x.type == NpcData.NpcType.Relic).FirstOrDefault();

            if (npcEntry != null)
            {
                Vector3 randomPos = NpcData.GetRandomPointInCircle(npcEntry.Location_Circle, 0.5f);
                if (!Task_NavmeshMove.Task_NavTo(randomPos, distance: 5, npcLoc: npcEntry.Location_Npc).Value)
                {
                    if (EzThrottler.Throttle("Repair move message", 1000))
                        IceLogging.Verbose($"Pathing to repair NPC. Current distance: {Player.DistanceTo(npcEntry.Location_Npc)}", handle);
                }
                else
                {
                    IceLogging.Debug("We're close enough to the repair npc! Continuing on", handle);
                    return true;
                }
            }
            else
            {
                if (EzThrottler.Throttle("Error message: NPC", 5000))
                    IceLogging.Error("Hey! We don't have this npc coded yet, which means I forgot bout it, could you let me know\n" +
                                     $"Planet Territory ID: {Player.Territory.RowId}", handle);
            }

            return false;
        }

        public static bool? TalkToResearchWay()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var selectString) && selectString.IsAddonReady)
            {
                IceLogging.Info("Talk to researchway complete");
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Clicking the talk dialog", 100))
                {
                    talk.Click();
                }
            }

            var researchId = NpcData.MoonNpcs[Player.Territory.RowId].Where(x => x.type == NpcData.NpcType.Relic).FirstOrDefault().NpcId;

            Utils.TryGetObjectByDataId(researchId, out var researchNpc);
            if (EzThrottler.Throttle("Interacting with researchingway"))
            {
                Utils.TargetgameObject(researchNpc);
                Utils.InteractWithObject(researchNpc);
            }

            return false;
        }

        public static bool? SelectReport()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectIconString>("SelectIconString", out var selectIconString) && selectIconString.IsAddonReady)
            {
                IceLogging.Info("We're onto selecting the class to turnin, woo!");
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var selectString) && selectString.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting the research one"))
                    selectString.Entries[0].Select();
            }

            return false;
        }

        public static bool? SelectRelicClass()
        {
            Dictionary<uint, bool> jobUnlocked = new()
            {
                [8] = true,
                [9] = true,
                [10] = true,
                [11] = true,
                [12] = true,
                [13] = true,
                [14] = true,
                [15] = true,
                [16] = true,
                [17] = true,
                [18] = true,
            };
            foreach (var jobId in jobUnlocked)
            {
                if (Player.GetLevel((Job)jobId.Key) == 0)
                    jobUnlocked[jobId.Key] = false;
            }

            if (EzThrottler.Throttle("Throttle job unlock message", 1000))
                IceLogging.Debug($"Amount of jobs unlocked: {jobUnlocked.Where(x => x.Value).Count()}");
            uint selectedEntry = 0;
            foreach (var jobId in jobUnlocked)
            {
                if (TurninJob == jobId.Key)
                    break;
                else
                {
                    if (jobId.Value)
                        selectedEntry += 1;
                }
            }


            if (GenericHelpers.TryGetAddonMaster<SelectIconString>("SelectIconString", out var selectIconString) && selectIconString.IsAddonReady)
            {
                if (EzThrottler.Throttle($"Selecting jobId: {TurninJob}"))
                {
                    IceLogging.Debug($"Selecting Entry: {selectedEntry} for job: {TurninJob} to turnin relic");
                    selectIconString.Entries[selectedEntry].Select();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var selectYesno) && selectYesno.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting yes for turnin"))
                {
                    IceLogging.Verbose("Selecting yes for the turnin");
                    selectYesno.Yes();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Clicking the talk dialog", 50))
                {
                    IceLogging.Verbose("Clicking the talk dialog");
                    talk.Click();
                }
            }
            else if (!Player.IsBusy)
            {
                IceLogging.Info("No longer busy talking to researchingway, to we're done");
                if (C.Relic_SwapJob)
                {
                    if (C.Relic_Stylist)
                    {
                        P.TaskManager.Enqueue(() => StylistCheck(), "Doing a stylist check", Utils.TaskConfig);
                    }
                    else
                    {
                        P.TaskManager.Enqueue(() => ReturnBackToJob(), "Returning back to the original job", Utils.TaskConfig);
                    }
                }
                return true;
            }

            return false;

        }
        public static bool StylistCheck()
        {
            var jobId = TurninJob;

            if (CosmicHelper.CrafterJobList.Contains(jobId))
            {
                Task_TurninMission.ExecuteCommand("/stylist crafter");
            }
            else if (CosmicHelper.GatheringJobList.Contains(jobId))
            {
                Task_TurninMission.ExecuteCommand("/stylist gatherer");
            }
            P.TaskManager.EnqueueDelay(1000);
            P.TaskManager.Enqueue(() => ReturnBackToJob(), "Returning back to original job", Utils.TaskConfig);

            return true;
        }

        private static int postRelicCounter = 0;

        public static bool? ReturnBackToJob()
        {
            if ((uint)Player.Job == TurninJob)
            {
                IceLogging.Debug("We're back on the proper job, continuing on");

                var delayAmount = C.DelayPostRelic == 0 ? 25 : C.DelayPostRelic;

                if (EzThrottler.Throttle("Add to counter", delayAmount))
                {
                    postRelicCounter += 1;
                }

                if (postRelicCounter >= 2)
                {
                    postRelicCounter = 0;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (EzThrottler.Throttle("Swapping jobs", 1000))
                {
                    IceLogging.Verbose($"Telling the game to swap you to jobID: {TurninJob}");
                    GearsetHandler.TaskClassChange((Job)TurninJob);
                }
                return false;
            }
        }
    }
}
