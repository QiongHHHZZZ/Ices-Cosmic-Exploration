using ECommons.GameHelpers;
using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static ICE.ConfigFiles.Config;

namespace ICE.Scheduler.Tasks
{
    internal static class Task_Gamba
    {
        public static readonly List<Gamba> DefaultGambaItems = new()
        {
            // Mounts
            new() { ItemId = 44505, Weight = 200, Type = GambaType.Mount },   // Vacuum Suit Identification Key
            new() { ItemId = 47973, Weight = 200, Type = GambaType.Mount },   // Warp Loader Identification Key
            new() { ItemId = 50441, Weight = 200, Type = GambaType.Mount },   // Volatile Gravity Vacuum Suit Identification Key

            // Emotes
            new() { ItemId = 44509, Weight = 25, Type = GambaType.Emote },    // Ballroom Etiquette - Personal Perfection
            new() { ItemId = 46795, Weight = 25, Type = GambaType.Emote },    // Ballroom Etiquette - Anticipating Exertion

            // Outfits
            new() { ItemId = 47937, Weight = 50, Type = GambaType.Outfit },   // Cosmosuit Coffer
            new() { ItemId = 47095, Weight = 50, Type = GambaType.Outfit },   // Star Pilot Attire Coffer
            new() { ItemId = 50828, Weight = 50, Type = GambaType.Outfit },   // Powersuit Coffer

            // Minions
            new() { ItemId = 47966, Weight = 25, Type = GambaType.Minion },   // Micro Rover
            new() { ItemId = 46782, Weight = 25, Type = GambaType.Minion },   // Model Suit
            new() { ItemId = 50323, Weight = 25, Type = GambaType.Minion },   // Droningway

            // Accessories
            new() { ItemId = 48154, Weight = 5, Type = GambaType.Accessory }, // The Faces We Wear - Tinted Sunglasses
            new() { ItemId = 48160, Weight = 5, Type = GambaType.Accessory }, // Loparasol
            new() { ItemId = 46840, Weight = 5, Type = GambaType.Accessory }, // The Faces We Wear - Scaevan Headgear
            new() { ItemId = 50458, Weight = 5, Type = GambaType.Accessory }, // The Faces We Wear - Holovisor
            new() { ItemId = 50455, Weight = 5, Type = GambaType.Accessory }, // The Faces We Wear - Holospecs

            // Orchestration
            new() { ItemId = 48210, Weight = 0, Type = GambaType.Orchestrion }, // Stargazers Orchestrion Roll
            new() { ItemId = 48220, Weight = 0, Type = GambaType.Orchestrion }, // Echoes in the Distance Orchestrion Roll
            new() { ItemId = 48221, Weight = 0, Type = GambaType.Orchestrion }, // Close in the Distance (Instrumental) Orchestrion Roll
            new() { ItemId = 46155, Weight = 0, Type = GambaType.Orchestrion }, // Kaleidoscope Orchestrion Roll
            new() { ItemId = 50803, Weight = 0, Type = GambaType.Orchestrion }, // The Uncharted Way Orchestrion Roll

            // Housing Items
            new() { ItemId = 23892, Weight = 0, Type = GambaType.Housing }, // Verdant Partition
            new() { ItemId = 48733, Weight = 0, Type = GambaType.Housing }, // Cosmotable
            new() { ItemId = 48734, Weight = 0, Type = GambaType.Housing }, // Cosmolamp
            new() { ItemId = 48136, Weight = 0, Type = GambaType.Housing }, // Drafting Table
            new() { ItemId = 32215, Weight = 0, Type = GambaType.Housing }, // Spring Meadow Partition
            new() { ItemId = 46175, Weight = 0, Type = GambaType.Housing }, // Portable Exoterminal
            new() { ItemId = 46174, Weight = 0, Type = GambaType.Housing }, // Cosmokitchen Partition
            new() { ItemId = 46173, Weight = 0, Type = GambaType.Housing }, // Cosmoseat
            new() { ItemId = 49849, Weight = 0, Type = GambaType.Housing }, // Imitation Cosmoportal
            new() { ItemId = 49850, Weight = 0, Type = GambaType.Housing }, // Cosmomodule Floor Panel

            // Dyes
            new() { ItemId = 52255, Weight = 0, Type = GambaType.Dye }, // Wide Spectrum #1 Dye
            new() { ItemId = 52256, Weight = 0, Type = GambaType.Dye }, // Wide Spectrum #2 Dye

            // Materia
            new() { ItemId = 41762, Weight = 0, Type = GambaType.Materia }, // Gatherer's Guerdon Materia XI
            new() { ItemId = 41763, Weight = 0, Type = GambaType.Materia }, // Gatherer's Guile Materia XI
            new() { ItemId = 41764, Weight = 0, Type = GambaType.Materia }, // Gatherer's Grasp Materia XI
            new() { ItemId = 41765, Weight = 0, Type = GambaType.Materia }, // Craftsman's Competence Materia XI
            new() { ItemId = 41766, Weight = 0, Type = GambaType.Materia }, // Craftsman's Cunning Materia XI
            new() { ItemId = 41767, Weight = 0, Type = GambaType.Materia }, // Craftsman's Command Materia XI
            new() { ItemId = 41775, Weight = 0, Type = GambaType.Materia }, // Gatherer's Guerdon Materia XII
            new() { ItemId = 41776, Weight = 0, Type = GambaType.Materia }, // Gatherer's Guile Materia XII
            new() { ItemId = 41777, Weight = 0, Type = GambaType.Materia }, // Gatherer's Grasp Materia XII
            new() { ItemId = 41778, Weight = 0, Type = GambaType.Materia }, // Craftsman's Competence Materia XII
            new() { ItemId = 41779, Weight = 0, Type = GambaType.Materia }, // Craftsman's Cunning Materia XII
            new() { ItemId = 41780, Weight = 0, Type = GambaType.Materia }, // Craftsman's Command Materia XII

            // Other
            new() { ItemId = 43943, Weight = 0, Type = GambaType.Other }, // Cracked Prismaticrystal
            new() { ItemId = 43944, Weight = 0, Type = GambaType.Other }, // Cracked Novacrystal
            new() { ItemId = 28724, Weight = 0, Type = GambaType.Other }, // Crafter's Delineation
            new() { ItemId = 6141,  Weight = 0, Type = GambaType.Other }, // Cordial HQ
            new() { ItemId = 48158, Weight = 0, Type = GambaType.Other }, // Magicked Prism (Cosmic Exploration)
            new() { ItemId = 50450, Weight = 0, Type = GambaType.Other }, // Cosmic Barding
        };
        public static void EnsureGambaWeightsInitialized(bool force = false)
        {
            bool changed = false;
            if (force)
                C.GambaItemWeights.Clear();
            foreach (var item in DefaultGambaItems)
            {
                if (C.GambaItemWeights.Any(x => x.ItemId == item.ItemId))
                    continue;
                C.GambaItemWeights.Add(new Gamba { ItemId = item.ItemId, Weight = item.Weight, Type = item.Type });
                changed = true;
            }
            if (changed)
                C.Save();
        }
        public static void Enqueue()
        {
            EnsureGambaWeightsInitialized();
            if (GenericHelpers.TryGetAddonMaster<WKSLottery>("WKSLottery", out var gamba) && gamba.IsAddonReady)
            {
                P.TaskManager.EnqueueMulti
                    (
                        new(GamblingTime, "Time to go gambling!", Utils.TaskConfig),
                        new(CloseTalk, "Closing the talk window"),
                        new(() => SchedulerMain.State = IceState.Idle)
                    );
            }
            else
            {
                // If this is the case, then we're here to initalize the gamba
                P.TaskManager.EnqueueMulti
                    (
                        new(Gamba_PathTo, "Pathing to the gamba NPC"),
                        new(TalkToGambaNpc, "Talk to the Gamba NPC"),
                        new(SelectGamba, "Selecting the options to go to gamba"),
                        new(GamblingTime, "Time to go gambling!", Utils.TaskConfig),
                        new(CloseTalk, "Closing the talk window")
                    );
            }
        }
        private static bool? Gamba_PathTo()
        {
            string handle = "Task_Gamba: PathTo";
            var zoneId = Player.Territory;
            var npcEntry = NpcData.MoonNpcs[zoneId.RowId].Where(x => x.type == NpcData.NpcType.Gamba).FirstOrDefault();

            if (npcEntry != null)
            {
                Vector3 randomPos = NpcData.GetRandomPointInCircle(npcEntry.Location_Circle, 0.5f);
                if (!Task_NavmeshMove.Task_NavTo(randomPos, distance: 5, npcLoc: npcEntry.Location_Npc).Value)
                {
                    if (EzThrottler.Throttle("Repair move message", 1000))
                        IceLogging.Verbose($"Pathing to repair NPC. Current distance: {Player.DistanceTo(npcEntry.Location_Npc)}", handle);
                }
                else
                {
                    IceLogging.Debug("We're close enough to the repair npc! Continuing on", handle);
                    return true;
                }
            }
            else
            {
                if (EzThrottler.Throttle("Error message: NPC", 5000))
                    IceLogging.Error("Hey! We don't have this npc coded yet, which means I forgot bout it, could you let me know\n" +
                                     $"Planet Territory ID: {Player.Territory.RowId}", handle);
            }

            return false;
        }
        private static bool? TalkToGambaNpc()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var selectString) && selectString.IsAddonReady)
            {
                IceLogging.Info("We've gotten to selecting the npc dialog (woo!). Selecting gamba");
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Closing Talk Window", 250))
                    talk.Click();
            }
            else
            {
                var researchId = NpcData.MoonNpcs[Player.Territory.RowId].Where(x => x.type == NpcData.NpcType.Gamba).FirstOrDefault().NpcId;

                Utils.TryGetObjectByDataId(researchId, out var researchNpc);
                if (EzThrottler.Throttle("Interacting with gambaNpc!"))
                {
                    Utils.TargetgameObject(researchNpc);
                    Utils.InteractWithObject(researchNpc);
                }
            }

            return false;
        }
        private static bool? SelectGamba()
        {
            if (GenericHelpers.TryGetAddonMaster<SelectIconString>("SelectIconString", out var iconString) && iconString.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting Materia Selection"))
                {
                    var select = iconString.Entries[0];
                    IceLogging.Debug($"Selecting: {select.Text}");
                    select.Select();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectString>("SelectString", out var selectString) && selectString.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting yes to gamba"))
                {
                    selectString.Entries[0].Select();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<WKSLottery>("WKSLottery", out var gamba) && gamba.IsAddonReady)
            {
                return true;
            }

            return false;
        }
        private static unsafe bool? GamblingTime()
        {
            string tag = "Gambling Time Task";

            if (GenericHelpers.TryGetAddonMaster<WKSLottery>("WKSLottery", out var gamba) && gamba.IsAddonReady)
            {
                var territory = Player.Territory.RowId;
                var itemId = CosmicHelper.PlanetCreditInfo[territory];
                PlayerHelper.GetItemCount(itemId, out var credits);

                bool confirmEnabled, leftWheelEnabled, rightWheelEnabled;
                unsafe
                {
                    confirmEnabled = gamba.SpinWheelButton->IsEnabled;
                    leftWheelEnabled = gamba.WheelLeftButton->IsEnabled;
                    rightWheelEnabled = gamba.WheelRightButton->IsEnabled;
                }

                if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var select) && select.IsAddonReady)
                {
                    if (credits >= 1000 + C.GambaCreditsMinimum)
                        select.Yes();
                    else
                        select.No();
                }
                else if (confirmEnabled)
                    gamba.ConfirmButton();
                else if (leftWheelEnabled || rightWheelEnabled)
                {
                    float leftWeight = gamba.LeftWheelItems.Sum(item => C.GambaItemWeights.FirstOrDefault(x => x.ItemId == item.itemId)?.Weight ?? 0);
                    float rightWeight = gamba.RightWheelItems.Sum(item => C.GambaItemWeights.FirstOrDefault(x => x.ItemId == item.itemId)?.Weight ?? 0);

                    if (C.GambaPreferSmallerWheel)
                    {
                        leftWeight = gamba.LeftWheelItems.Length > 0 ? leftWeight / gamba.LeftWheelItems.Length : 0;
                        rightWeight = gamba.RightWheelItems.Length > 0 ? rightWeight / gamba.RightWheelItems.Length : 0;

                        if (leftWeight == rightWeight && leftWeight > 0)
                        {
                            leftWeight += 1.0f / Math.Max(1, gamba.LeftWheelItems.Length);
                            rightWeight += 1.0f / Math.Max(1, gamba.RightWheelItems.Length);
                        }
                    }

                    if (gamba.LeftWheelItems.Length == 0)
                    {
                        IceLogging.Info($"Found a pure stellar mission gamba. Choosing left wheel", tag);
                        SelectWheelLeft(gamba);
                    }
                    else if (gamba.RightWheelItems.Length == 0)
                    {
                        IceLogging.Info($"Found a pure stellar mission gamba. Choosing right wheel", tag);
                        SelectWheelRight(gamba);
                    }
                    else if (leftWeight > rightWeight)
                    {
                        IceLogging.Info($"[Gamba] First wheel is better with total weight: {leftWeight}");
                        SelectWheelLeft(gamba);
                    }
                    else if (rightWeight > leftWeight)
                    {
                        IceLogging.Info($"[Gamba] Second wheel is better with total weight: {rightWeight}");
                        SelectWheelRight(gamba);
                    }
                    else
                    {
                        IceLogging.Info("[Gamba] Both wheels are equal in weight. Randomly selecting one.");
                        if (new Random().Next(2) == 0)
                            SelectWheelLeft(gamba);
                        else
                            SelectWheelRight(gamba);
                    }
                }

                return false;
            }
            else
            {
                return true;
            }

        }
        private static unsafe bool HasEnoughCredits()
        {
            var territory = Player.Territory.RowId;
            var itemId = CosmicHelper.PlanetCreditInfo[territory];

            PlayerHelper.GetItemCount(itemId, out var credits);
            return credits >= 1000;
        }
        private static bool? CloseTalk()
        {
            if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Closing Talk Window", 250))
                    talk.Click();
                return false;
            }
            else
            {
                return true;
            }
        }
        public static unsafe void SelectWheelLeft(WKSLottery gamba)
        {
            gamba.WheelLeftButton->Flags = 327936U; // Checked, Enabled, Selected
            gamba.WheelRightButton->Flags = 65792U; // Not Checked, Enabled, Not Selected
            IceLogging.Debug($"[Gamba] Selecting Left Wheel");
        }
        public static unsafe void SelectWheelRight(WKSLottery gamba)
        {
            gamba.WheelLeftButton->Flags = 65792U; // Not Checked, Enabled, Not Selected
            gamba.WheelRightButton->Flags = 327936U; // Checked, Enabled, Selected
            IceLogging.Debug($"[Gamba] Selecting Right Wheel");
        }
        public static bool BigBangGamba()
        {
            // Big Bang Tickets are earned from doing the fates... and this kind fucks with things? 
            // Name of the item is "Bing Bang Fortune (Planet Name)

            return false;
        }
    }
}
