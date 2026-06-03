using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class CS_TimerInfo
    {
        public static void Draw()
        {
            ImGui.Text(T("All world timers:"));
            TimerUpdate();

            if (CosmicHandler.EventInfo() is { } info)
            {
                ImGui.Text(T("State: {0}", info.wksEvent));
                ImGui.Text(T("Timer: {0:N0}", info.timer));
            }
        }

        private static unsafe void TimerUpdate()
        {
            var c = UIState.Instance()->MassivePcContentTodo.Director;
            if (c != null)
            {
                for (int i = 0; i < c->MassivePcContentTodos.Length; i++)
                {
                    var todo = c->MassivePcContentTodos[i];
                    for (int i1 = 0; i1 < todo.Count; i1++)
                    {
                        var t = todo[i1];
                        if (t.Enabled)
                        {
                            ImGuiEx.Text($"{i} - {i1} - {t.EndTimestamp - Framework.GetServerTime()}");
                        }
                    }
                }
            }
        }
    }
}
