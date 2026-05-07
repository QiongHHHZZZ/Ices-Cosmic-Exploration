using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ICE.Utilities
{
    internal class CosmicHandler
    {
        internal unsafe static bool IsMissionTimedOut()
        {
            var c = UIState.Instance()->MassivePcContentTodo.Director;
            if (c != null)
            {
                var todo = c->MassivePcContentTodos[1];
                if (todo[1].Enabled)
                {
                    var t = todo[1];
                    var timeRemaining = t.EndTimestamp - Framework.GetServerTime();
                    if (timeRemaining > 0)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public enum WKSEvents
        {
            Mechops_Commenced = 0,
            RedAlert_Incoming = 1,
            RedAlert_Progressing = 2,
            MechOps_Issues = 5,
            MechOps_Deploying = 6,
            WaitingforDevStage = 8,
        }

        internal unsafe static (WKSEvents wksEvent, uint timer)? EventInfo()
        {
            var agent = AgentWKSAnnounce.Instance();
            if (agent == null || agent->Data == null)
                return null;

            var data = agent->Data;
            return ((WKSEvents)data->State, data->EndTime);
        }
    }
}
