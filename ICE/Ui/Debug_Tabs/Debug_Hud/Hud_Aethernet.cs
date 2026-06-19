using Dalamud.Interface;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ICE.Utilities.Cosmic_Helper;
using InteropGenerator.Runtime.Attributes;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.Debug_Tabs.Debug_Hud
{
    internal class Hud_Aethernet
    {
        public static void Draw()
        {
            UpdateDict();
            foreach (var entry in MoonAethernet)
            {
                bool isUnlocked = AgentWKSMissionEx.IsWKSAetheryteUnlocked((byte)entry.Key);

                FontAwesomeIcon mark = isUnlocked ? FontAwesomeIcon.Check : FontAwesomeIcon.XmarksLines;

                string ids = string.Join(",", entry.Value.BaseId);

                ImGuiEx.IconWithText(mark, $"以太之光ID：{ids} | 名称：{entry.Value.Name}");
            }
        }

        public class WKSAetherInfo
        {
            public string Name { get; set; } = string.Empty;
            public List<uint> BaseId { get; set; } = new();
        }

        public static Dictionary<uint, WKSAetherInfo> MoonAethernet = new();
        private static void UpdateDict()
        {
            if (MoonAethernet.Count == 0)
            {
                var wksAetheryteSheet = Svc.Data.GetExcelSheet<WKSAetheryte>();
                foreach (var aetherSheet in wksAetheryteSheet)
                {
                    var rowId = aetherSheet.RowId;
                    if (rowId == 0)
                        continue;

                    string name = aetherSheet.Name.Value.Name.ToString();

                    var ids = aetherSheet.ObjectGroup.Value.Select(s => s.Unknown0).ToList();

                    uint baseId = aetherSheet.ObjectGroup.Value.First().Unknown0;
                    MoonAethernet[rowId] = new()
                    {
                        Name = name,
                        BaseId = ids
                    };
                }
            }
        }
    }
}
