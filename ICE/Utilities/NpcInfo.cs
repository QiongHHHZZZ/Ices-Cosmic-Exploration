using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Numerics; // Add this for Vector2 and Vector3

namespace ICE.Utilities;

internal static class NpcData // Renamed the class to avoid conflict
{
    public enum NpcType
    {
        Repair,
        Credit,
        Relic,
        Gamba,
        Drone,
        RedAlert
    }

    public class NPCInfo // Keep this class for the dictionary
    {
        public uint NpcId { get; set; }
        public string Name { get; set; }
        public Vector3 Location_Npc { get; set; }
        public Vector3 Location_Circle { get; set; }
    }

    public static Dictionary<uint, Dictionary<NpcType, NPCInfo>> MoonNpcs = new()
    {
        [1237] = new Dictionary<NpcType, NPCInfo> // sinus
        {
            [NpcType.Repair] = new NPCInfo // Repair | Gil Gear Vendor
            {
                NpcId = 1052610,
                Name = "Godgyth",
                Location_Npc = new Vector3(19.46f, 1.69f, 18.11f),
                Location_Circle = new Vector3(16.97f, 1.69f, 15.16f),
            },
            [NpcType.Credit] = new NPCInfo // Credit Exchange Vendor
            {
                NpcId = 1052608,
                Name = "Mesouaidonque",
                Location_Npc = new Vector3(18.23f, 1.69f, 19.42f),
                Location_Circle = new Vector3(15.08f, 1.69f, 16.89f),
            },
            [NpcType.Relic] = new NPCInfo // Relic NPC
            {
                NpcId = 1052605,
                Name = "Researchingway",
                Location_Npc = new Vector3(-18.91f, 2.15f, 18.84f),
                Location_Circle = new Vector3(-16.03f, 1.69f, 16.10f),
            },
            [NpcType.Gamba] = new NPCInfo // Cosmic Fortune aka Gamba Wheel
            {
                NpcId = 1052612,
                Name = "Orbitingway",
                Location_Npc = new Vector3(18.84f, 2.24f, -18.91f),
                Location_Circle = new Vector3(15.92f, 1.69f, -16.16f),
            },
            [NpcType.RedAlert] = new()
            {
                NpcId = 1052663,
                Name = "Lefleda",
                Location_Npc = new(16.74f, 1.71f, -3.86f),
                Location_Circle = new(15.28f, 1.64f, -3.83f),

            }
        },
        [1291] = new Dictionary<NpcType, NPCInfo> // phaenna
        {
            [NpcType.Repair] = new NPCInfo
            {
                NpcId = 1052641,
                Name = "Godgyth",
                Location_Npc = new Vector3(359.52f, 52.75f, -401.72f),
                Location_Circle = new(357.25f, 52.69f, -405.16f),
            },
            [NpcType.Credit] = new NPCInfo // Credit Exchange Vendor
            {
                NpcId = 1052640,
                Name = "Mesouaidonque",
                Location_Npc = new Vector3(358.33f, 52.75f, -400.44f),
                Location_Circle = new(354.94f, 52.69f, -402.99f),
            },
            [NpcType.Relic] = new NPCInfo // Relic NPC
            {
                NpcId = 1052629,
                Name = "Researchingway",
                Location_Npc = new Vector3(321.22f, 53.19f, -401.24f),
                Location_Circle = new(323.92f, 52.69f, -404.05f),
            },
            [NpcType.Gamba] = new NPCInfo // Cosmic Fortune aka Gamba Wheel
            {
                NpcId = 1052642,
                Name = "Orbitingway",
                Location_Npc = new Vector3(358.82f, 53.19f, -438.86f),
                Location_Circle = new Vector3(355.97f, 52.69f, -436.08f),
            },
            [NpcType.RedAlert] = new NPCInfo()
            {
                NpcId = 1052626,
                Name = "Lefleda",
                Location_Npc = new(343.89f, 52.64f, -443.47f),
                Location_Circle = new(343.46f, 52.64f, -441.80f),
            }
        },
        [1310] = new Dictionary<NpcType, NPCInfo> // Oizys
        {
            [NpcType.Repair] = new NPCInfo // Repair | Gil Gear Vendor
            {
                NpcId = 1052651,
                Name = "Godgyth",
                Location_Npc = new Vector3(-202.44f, 0.65f, 154.31f),
                Location_Circle = new Vector3(-198.98f, 0.50f, 153.69f),
            },
            [NpcType.Credit] = new NPCInfo // Credit Exchange Vendor
            {
                NpcId = 1052650,
                Name = "Mesouaidonque",
                Location_Npc = new Vector3(-202.20f, 0.65f, 152.54f),
                Location_Circle = new Vector3(-198.98f, 0.50f, 153.69f),
            },
            [NpcType.Relic] = new NPCInfo // Relic NPC
            {
                NpcId = 1052647,
                Name = "Researchingway",
                Location_Npc = new Vector3(-202.26f, 1.19f, 122.00f),
                Location_Circle = new Vector3(-199.07f, 0.50f, 121.97f),
            },
            [NpcType.Gamba] = new NPCInfo // Cosmic Fortune aka Gamba Wheel
            {
                NpcId = 1052652,
                Name = "Orbitingway",
                Location_Npc = new Vector3(-157.73f, 1.19f, 153.98f),
                Location_Circle = new Vector3(-161.04f, 0.50f, 153.84f),
            },
            [NpcType.Drone] = new NPCInfo
            {
                NpcId = 1052654,
                Name = "Kaede",
                Location_Npc = new Vector3(-206.38f, 0.50f, 131.09f),
                Location_Circle = new(-205.85f, 0.65f, 134.19f),
            },
            [NpcType.RedAlert] = new NPCInfo()
            {
                NpcId = 1052645,
                Name = "Lefleda",
                Location_Npc = new(-155.02f, 0.50f, 144.58f),
                Location_Circle = new(-156.91f, 0.50f, 143.07f),
            }
        },
        [1319] = new Dictionary<NpcType, NPCInfo> // Auxesia
        {
            [NpcType.Repair] = new NPCInfo // Repair | Gil Gear Vendor
            {
                NpcId = 1056825,
                Name = "Godgyth",
                Location_Npc = new Vector3(317.60f, 205.75f, 374.77f),
                Location_Circle = new Vector3(315.01f, 205.64f, 375.59f),
            },
            [NpcType.Credit] = new NPCInfo // Credit Exchange Vendor
            {
                NpcId = 1056824,
                Name = "Mesouaidonque",
                Location_Npc = new Vector3(317.73f, 205.75f, 376.69f),
                Location_Circle = new Vector3(315.01f, 205.64f, 375.59f),
            },
            [NpcType.Relic] = new NPCInfo // Relic NPC
            {
                NpcId = 1056821,
                Name = "Researchingway",
                Location_Npc = new Vector3(291.00f, 206.21f, 402.57f),
                Location_Circle = new Vector3(291.08f, 205.64f, 399.60f),
            },
            [NpcType.Gamba] = new NPCInfo // Cosmic Fortune aka Gamba Wheel
            {
                NpcId = 1056826,
                Name = "Orbitingway",
                Location_Npc = new Vector3(290.94f, 206.21f, 349.35f),
                Location_Circle = new Vector3(290.90f, 205.64f, 352.39f),
            },
            [NpcType.Drone] = new NPCInfo
            {
                NpcId = 1056828,
                Name = "Kaede",
                Location_Npc = new Vector3(302.20f, 205.64f, 398.5f),
                Location_Circle = new Vector3(301.25f, 205.64f, 396.5f),
            },
            [NpcType.RedAlert] = new NPCInfo()
            {
                NpcId = 1056819,
                Name = "Lefleda",
                Location_Npc = new(280.41f, 205.64f, 352.49f),
                Location_Circle = new(280.47f, 205.64f, 354.54f),
            }
        },
    };

    public static Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        // Generate random angle (0 to 2π)
        float angle = Random.Shared.NextSingle() * MathF.PI * 2f;

        // Generate random distance from center
        // Use square root for uniform distribution
        float distance = MathF.Sqrt(Random.Shared.NextSingle()) * radius;

        // Convert polar to cartesian coordinates
        float x = center.X + distance * MathF.Cos(angle);
        float z = center.Z + distance * MathF.Sin(angle);

        return new Vector3(x, center.Y, z);
    }
}