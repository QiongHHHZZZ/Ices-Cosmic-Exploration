
using System.Collections.Generic;

namespace ICE.ConfigFiles;

public partial class Config
{
    public WindowSelection SelectedTab { get; set; } = WindowSelection.MissionSetup;
    public SidebarTabs ExpandedTabs { get; set; } = SidebarTabs.None;
    public Dictionary<string, bool> Mission_Tabs { get; set; } = new();
    public bool UseChineseUi { get; set; } = true;
}
