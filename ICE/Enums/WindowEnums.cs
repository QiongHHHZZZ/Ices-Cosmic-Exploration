using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Enums
{
    public enum WindowSelection
    {
        MissionSetup,
        CosmicAgenda,
        ExpeditionLogs,

        StopWhen,
        GatheringProfiles,
        MissionPriority,
        CharacterSettings,
        MiscSettings,
        TravelSettings,

        CreditShopping,
        GambaShopping,
        DroneShopping,

        Plugin_Install,
        Plugin_Logs,
        Plugin_Tips,
    }

    [Flags]
    public enum SidebarTabs
    {
        None = 0,

        CosmicHelper = 1 << 0,
        PlanetSelection = 1 << 1,
        HubActivites = 1 << 2,
        Settings = 1 << 3,
        ClassSelection = 1 << 4,
        ExpInfo = 1 << 5,
        HelpInfo = 1 << 6,

        All = CosmicHelper + PlanetSelection + HubActivites
            + Settings + ClassSelection + ExpInfo
            + HelpInfo
    }
}
