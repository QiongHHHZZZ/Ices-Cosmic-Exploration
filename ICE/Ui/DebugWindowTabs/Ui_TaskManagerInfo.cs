using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_TaskManagerInfo
    {
        private static uint mission = 0;
        private static List<Vector3> pathTo = new List<Vector3>();
        private static Vector3 pathToArea = new Vector3();

        public static void Draw()
        {
            ImGui.Text(T("Running task: {0} | Amount of queue'd task: {1}", P.TaskManager.NumQueuedTasks != 0, P.TaskManager.NumQueuedTasks));
            string currentTask = P.TaskManager.CurrentTask?.Name ?? "";
            ImGui.Text(T("Current task running: {0}", currentTask));
            ImGui.Text(T("Current State: {0}", T(SchedulerMain.State.ToString())));
            ImGui.Text(T("Task Count: {0}", P.TaskManager.Tasks.Count));
            if (ImGui.Button(T("Set State to Idle")))
            {
                SchedulerMain.State = IceState.Idle; 
            }

            if (ImGui.Button(T("Stop Task")))
            {
                P.TaskManager.Tasks.Clear();
                P.TaskManager.Abort();
            }

            ImGui.SetNextItemWidth(100);
            ImGui.InputUInt(T("Mission"), ref mission);

            if (ImGui.Button(T("Abandon Mission")))
            {
                Task_AbandonMission.Enqueue();
            }
            if (ImGui.Button(T("Path to repair NPC")))
            {
                P.TaskManager.Enqueue(() => Task_Repair.Repair_PathTo(), "Pathing to repair NPC");
            }
            if (ImGui.Button(T("Test Repair Function")))
            {
                Task_Repair.Enqueue();
            }
            ImGui.Text(T("Current waypoint list count: {0}", pathTo.Count));

            ImGui.SetNextItemWidth(250);
            ImGui.InputFloat3(T("Destination"), ref pathToArea);
            if (ImGui.Button(T("Set Area")))
            {
                pathToArea = ECommons.GameHelpers.Player.Position;
            }
            if (ImGui.Button(T("Create waypoint list")))
            {
                Vector3 currentPos = ECommons.GameHelpers.Player.Position;

                // Fire and forget - this will update pathTo when complete
                _ = Task.Run(async () =>
                {
                    pathTo = await FindTask(currentPos);
                });
            }
            if (ImGui.Button(T("Test Crafting")))
            {
                Task_Craft.Enqueue();
            }
            if (ImGui.Button(T("Test Gather Targeting")))
            {
                Task_Gather.Enqueue();
            }
            if (ImGui.Button(T("Buy Items from shop")))
            {
                Task_BuyCosmoItems.Enqueue();
            }

            if (ImGui.Button(T("Test Drone Buy Item")))
            {
                Task_ArtifactSearch.EnqueueBuy();
            }
            if (ImGui.Button(T("Test Drone Pathing")))
            {
                P.TaskManager.Enqueue(() => Task_ArtifactSearch.CheckBoxStatus());
            }
        }

        private static async Task<List<Vector3>> FindTask(Vector3 currentPos)
        {
            IceLogging.DestinationLogs.Log(pathToArea);
            return await P.Navmesh.Pathfind(currentPos, pathToArea, false);
        }
    }
}
