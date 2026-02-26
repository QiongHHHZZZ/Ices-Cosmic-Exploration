
using System.Collections.Generic;

namespace ICE.ConfigFiles;

public partial class Config
{
    public Dictionary<string, bool> MainUi_CustomHeader { get; set; } = new();
    public string MainUi_SelectedWindow { get; set; } = "";
    public Dictionary<string, bool> Mission_Tabs { get; set; } = new();
    public bool UseChineseUi { get; set; } = true;
}
