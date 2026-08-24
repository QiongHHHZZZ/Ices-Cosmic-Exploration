using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Utilities;

public class QuestCheck
{
    public static unsafe bool CollectablesUnlocked()
    {
        var questManager = QuestManager.Instance();
        if (questManager == null)
            return false;

        ushort collectableMission = 2095;
        var missionId = questManager->GetQuestById(collectableMission);

        return QuestManager.IsQuestComplete(collectableMission);
    }
}
