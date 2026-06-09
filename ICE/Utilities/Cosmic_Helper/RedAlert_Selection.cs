using ICE.Utilities.GatheringHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Utilities.Cosmic_Helper;

public static partial class CosmicHelper
{
    public class CriticalInfo
    {
        public Vector3 RawLocation { get; set; }
        public uint NpcSelection { get; set; }
    }

    #region Sinus

    private static CriticalInfo AstromagneticStorm1α = new()
    {
        RawLocation = new Vector3(176.24f, 9.40f, 560.07f),
        NpcSelection = 0,
    };
    private static CriticalInfo AstromagneticStorm1β = new()
    {
        RawLocation = new Vector3(-91.58f, 19.32f, -241.99f),
        NpcSelection = 1,
    };
    private static CriticalInfo AstromagneticStorm2α = new()
    {
        RawLocation = new Vector3(-72.76f, 51.00f, 768.64f),
        NpcSelection = 0,
    };
    private static CriticalInfo AstromagneticStorm2β = new()
    {
        RawLocation = new Vector3(-464.50f, 37.89f, -69.89f),
        NpcSelection = 1,
    };

    private static CriticalInfo MeteorShower1α = new()
    {
        RawLocation = new Vector3(-219.93f, 24.16f, 209.98f),
        NpcSelection = 0,
    };
    private static CriticalInfo MeteorShower1β = new()
    {
        RawLocation = new Vector3(34.86f, 34.38f, -349.75f),
        NpcSelection = 1,
    };
    private static CriticalInfo MeteorShower2α = new()
    {
        RawLocation = new Vector3(845.90f, -58.44f, -390.45f),
        NpcSelection = 0,
    };
    private static CriticalInfo MeteorShower2β = new()
    {
        RawLocation = new Vector3(497.36f, -115.32f, -845.65f),
        NpcSelection = 1,
    };

    private static CriticalInfo SporingMist1α = new()
    {
        RawLocation = new Vector3(539.43f, 36.38f, 49.89f),
        NpcSelection = 0,
    };
    private static CriticalInfo SporingMist1β = new()
    {
        RawLocation = new Vector3(654.32f, 52.00f, 100.13f),
        NpcSelection = 1,
    };
    private static CriticalInfo SporingMist2α = new()
    {
        RawLocation = new Vector3(379.62f, 51.39f, 704.14f),
        NpcSelection = 0,
    };
    private static CriticalInfo SporingMist2β = new()
    {
        RawLocation = new Vector3(99.66f, 18.11f, -209.80f),
        NpcSelection = 1,
    };

    #endregion

    #region Phaenna

    // Phaenna
    private static CriticalInfo Thunderstorms1α = new()
    {
        RawLocation = new Vector3(417.57f, 52.00f, -445.41f),
        NpcSelection = 0,
    };
    private static CriticalInfo Thunderstorms1β = new()
    {
        RawLocation = new Vector3(432.76f, 54.13f, -169.80f),
        NpcSelection = 1,
    };
    private static CriticalInfo Thunderstorms2α = new()
    {
        RawLocation = new Vector3(169.79f, 41.00f, -210.79f),
        NpcSelection = 0,
    };
    private static CriticalInfo Thunderstorms2β = new()
    {
        RawLocation = new Vector3(-615.30f, 8.26f, -515.45f),
        NpcSelection = 1,
    };

    private static CriticalInfo AnnealingWinds1α = new()
    {
        RawLocation = new Vector3(239.77f, 133.83f, -704.44f),
        NpcSelection = 0,
    };
    private static CriticalInfo AnnealingWinds1β = new()
    {
        RawLocation = new Vector3(-506.27f, -8.42f, -751.29f),
        NpcSelection = 1,
    };
    private static CriticalInfo AnnealingWinds2α = new()
    {
        RawLocation = new Vector3(410.29f, 18.90f, 25.14f),
        NpcSelection = 1,
    };
    private static CriticalInfo AnnealingWinds2β = new()
    {
        RawLocation = new Vector3(10.10f, 7.98f, 339.70f),
        NpcSelection = 0,
    };

    private static CriticalInfo GlassRain1α = new()
    {
        RawLocation = new Vector3(407.15f, -229.45f, 224.76f),
        NpcSelection = 0,
    };
    private static CriticalInfo GlassRain1β = new()
    {
        RawLocation = new Vector3(544.42f, -251.07f, 634.55f),
        NpcSelection = 1,
    };
    private static CriticalInfo GlassRain2α = new()
    {
        RawLocation = new Vector3(148.96f, -9.99f, 487.46f),
        NpcSelection = 0,
    };
    private static CriticalInfo GlassRain2β = new()
    {
        RawLocation = new Vector3(-488.32f, 25.05f, 35.65f),
        NpcSelection = 1,
    };

    #endregion

    #region Oizys

    private static CriticalInfo GravitationAnom1α = new()
    {
        RawLocation = new Vector3(77.05f, -58.69f, -475.25f),
        NpcSelection = 0,
    };
    private static CriticalInfo GravitationAnom1β = new()
    {
        RawLocation = new Vector3(-189.71f, -0.07f, -61.36f),
        NpcSelection = 1,
    };

    private static CriticalInfo GaleForce1α = new()
    {
        RawLocation = new(584.29f, -60.42f, -429.47f),
        NpcSelection = 0,
    };
    private static CriticalInfo GaleForce1β = new()
    {
        RawLocation = new(125.37f, 0.64f, -68.72f),
        NpcSelection = 1,
    };
    private static CriticalInfo GaleForce2α = new()
    {
        RawLocation = new(-669.31f, -88.50f, -453.18f),
        NpcSelection = 0,
    };
    private static CriticalInfo GaleForce2β = new()
    {
        RawLocation = new(-127.96f, 0.27f, -50.00f),
        NpcSelection = 1,
    };

    private static CriticalInfo BubbleBloom1α = new()
    {
        RawLocation = new(-572.10f, 22.70f, 156.11f),
        NpcSelection = 0,
    };
    private static CriticalInfo BubbleBloom1β = new()
    {
        RawLocation = new(-369.71f, 104.98f, 876.68f),
        NpcSelection = 1,
    };

    #endregion

    #region Auxesia

    private static CriticalInfo AuroralFlare1α = new()
    {
        RawLocation = new(-37.54f, 185.10f, 352.42f),
        NpcSelection = 0,
    };
    private static CriticalInfo AuroralFlare1β = new()
    {
        RawLocation = new(-660.01f, 184.99f, 292.33f),
        NpcSelection = 1,
    };
    private static CriticalInfo AuroralFlare2α = new()
    {
        RawLocation = new(13.88f, 165.01f, 124.97f),
        NpcSelection = 0,
    };
    private static CriticalInfo AuroralFlare2β = new()
    {
        RawLocation = new(300.00f, 165.10f, 13.52f),
        NpcSelection = 1,
    };

    private static CriticalInfo Floracane1α = new()
    {
        RawLocation = new(-367.00f, 146.96f, 426.96f),
        NpcSelection = 0,
    };
    private static CriticalInfo Floracane1β = new()
    {
        RawLocation = new(-654.14f, 184.99f, 290.08f),
        NpcSelection = 1,
    };
    private static CriticalInfo Floracane2α = new()
    {
        RawLocation = new(739.87f, 184.28f, 514.36f),
        NpcSelection = 0,
    };
    private static CriticalInfo Floracane2β = new()
    {
        RawLocation = new(-353.96f, 165.10f, 226.09f),
        NpcSelection = 1,
    };

    #endregion

    public static void UpdateCriticalWeather()
    {
        CriticalUpdate(AstromagneticStorm1α, 40);
        CriticalUpdate(AstromagneticStorm1β, 41);
        CriticalUpdate(AstromagneticStorm2α, 38);
        CriticalUpdate(AstromagneticStorm2β, 39);

        CriticalUpdate(MeteorShower1α, 42);
        CriticalUpdate(MeteorShower1β, 43);
        CriticalUpdate(MeteorShower2α, 44);
        CriticalUpdate(MeteorShower2β, 45);

        CriticalUpdate(SporingMist1α, 48);
        CriticalUpdate(SporingMist1β, 49);
        CriticalUpdate(SporingMist2α, 46);
        CriticalUpdate(SporingMist2β, 47);

        // Phaenna
        CriticalUpdate(Thunderstorms1α, 83);
        CriticalUpdate(Thunderstorms1β, 84);
        CriticalUpdate(Thunderstorms2α, 85);
        CriticalUpdate(Thunderstorms2β, 86);

        CriticalUpdate(AnnealingWinds1α, 87);
        CriticalUpdate(AnnealingWinds1β, 88);
        CriticalUpdate(AnnealingWinds2α, 90);
        CriticalUpdate(AnnealingWinds2β, 89);

        CriticalUpdate(GlassRain1α, 91);
        CriticalUpdate(GlassRain1β, 92);
        CriticalUpdate(GlassRain2α, 93);
        CriticalUpdate(GlassRain2β, 94);

        // Oizys
        CriticalUpdate(GravitationAnom1α, 101);
        CriticalUpdate(GravitationAnom1β, 102);

        CriticalUpdate(GaleForce1α, 105);
        CriticalUpdate(GaleForce1β, 106);
        CriticalUpdate(GaleForce2α, 107);
        CriticalUpdate(GaleForce2β, 108);

        CriticalUpdate(BubbleBloom1α, 103);
        CriticalUpdate(BubbleBloom1β, 104);

        // Auxesia
        CriticalUpdate(AuroralFlare1α, 185);
        CriticalUpdate(AuroralFlare1β, 186);
        CriticalUpdate(AuroralFlare2α, 187);
        CriticalUpdate(AuroralFlare2β, 188);

        CriticalUpdate(Floracane1α, 189);
        CriticalUpdate(Floracane1β, 190);
        CriticalUpdate(Floracane2α, 191);
        CriticalUpdate(Floracane2β, 192);

        // Auxesia criticals — AddKeys(...) once the CriticalInfo blocks above exist.

    }
    private static void CriticalUpdate(CriticalInfo details, uint key)
    {
        GatheringUtil.CriticalSpots[key].WorldCords = details.RawLocation;
        GatheringUtil.CriticalSpots[key].NpcSelector = details.NpcSelection;
    }
}
