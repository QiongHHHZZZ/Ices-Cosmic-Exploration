using ICE.Utilities.Cosmic_Helper;

namespace ICE.Scheduler.Tasks
{
    internal class Task_Manual
    {
        public static void Enqueue()
        {
            P.TaskManager.Enqueue(() => WaitForNextMission(), "Waiting for mission to not be 0", Utils.TaskConfig);
        }

        private static bool? WaitForNextMission()
        {
            if (CosmicHelper.CurrentLunarMission == 0)
            {
                if (Mission_Settings.StopAfterCurrent)
                {
                    SchedulerMain.State = IceState.Idle;
                    Mission_Settings.StopAfterCurrent = false;
                    P.TaskManager.Tasks.Clear();
                }
                else
                {
                    SchedulerMain.State = IceState.Start;
                    P.TaskManager.Tasks.Clear();
                }
                return true;
            }

            return false;
        }
    }
}
