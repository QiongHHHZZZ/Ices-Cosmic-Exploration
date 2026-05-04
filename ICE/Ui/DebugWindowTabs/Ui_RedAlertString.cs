using System;
using System.Collections.Generic;
using System.Text;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_RedAlertString
    {
        public static Dictionary<Job, string> classJobs = new();

        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectString>(out var selectString))
            {
                foreach (var entry in selectString.Entries)
                {
                    if (ImGui.Button($"{entry.Text}"))
                    {
                        entry.Select();
                    }
                }
            }
            else
            {
                ImGui.Text("Select string not visible");
            }
        }
    }
}
