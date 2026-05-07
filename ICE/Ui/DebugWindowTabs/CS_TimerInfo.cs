using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.DebugWindowTabs
{
    internal class CS_TimerInfo
    {
        public static void Draw()
        {
            ImGui.Text("All world timers:");
            TimerUpdate();

            if (CosmicHandler.EventInfo() is { } info)
            {
                ImGui.Text($"State: {info.wksEvent}");
                ImGui.Text($"Timer: {info.timer:N0}");
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
