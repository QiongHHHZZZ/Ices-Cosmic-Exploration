using ICE.Utilities.Cosmic_Helper;

namespace ICE.Scheduler.Tasks.OldTask
{
    internal class TaskManualMode
    {
        public static void ZenMode()
        {
            if (CosmicHelper.CurrentLunarMission == 0)
            {
                SchedulerMain.State = IceState.GrabMission;
            }
            if (!C.OnlyGrabMission_Debug)
            {
                SchedulerMain.State = IceState.ManualMode;
            }
        }
    }
}
