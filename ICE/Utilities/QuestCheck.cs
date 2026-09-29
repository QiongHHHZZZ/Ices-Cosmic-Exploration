using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Utilities;

public class QuestCheck
{
    public static bool CollectablesUnlocked()
    {
        ushort collectableMission = 2097;
        return QuestCompleted(collectableMission);
    }

    public static bool ReductionUnlocked()
    {
        ushort reduceMission = 2095;
        return QuestCompleted(reduceMission);
    }

    private static unsafe bool QuestCompleted(ushort questId)
    {
        var questManager = QuestManager.Instance();
        if (questManager == null)
            return false;

        return QuestManager.IsQuestComplete(questId);
    }


}
