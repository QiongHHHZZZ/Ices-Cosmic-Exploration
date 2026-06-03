using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ICE.Scheduler.Handlers.PictoStuff;
using ICE.Utilities.Cosmic_Helper;
using ICE.Utilities.ImGuiTools;
using System.Collections.Generic;
using static ICE.Localization.L10n;

namespace ICE.Ui.DebugWindowTabs
{
    internal class Ui_PlayerInfo
    {
        private static uint best_LevelMission = 0;
        private static uint playerLevel = 90;

        private static int currentXp = 0;
        private static int neededXp = 100;
        private static int maxXp = 200;

        public static unsafe void Draw()
        {
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt(T("Current XP"), ref currentXp);
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt(T("Needed XP"), ref neededXp);
            ImGui.SetNextItemWidth(200);
            ImGui.InputInt(T("Max XP"), ref maxXp);
            ImGui_Ice.Draw_XPBar(currentXp, neededXp, maxXp, size: new Vector2(200, 10));

            ImGui.Separator();
            var currentProgress = WorldProgress();
            ImGui.Text(T("World Stage: {0}", currentProgress));

            ImGui.Text(T("Need to actually put the player info here. It got lost"));
            ImGui.Spacing();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(T("Player Position: X:{0:N2}, Y:{1:N2}, Z:{2:N2}", Player.Position.X, Player.Position.Y, Player.Position.Z));
            ImGui.SameLine();
            if (ImGui.Button(T("Copy Vector2")))
            {
                ImGui.SetClipboardText($"{Player.Position.X:N2}f, {Player.Position.Z:N2}f");
            }
            ImGui.SameLine();
            if (ImGui.Button(T("Copy Vector3")))
            {
                ImGui.SetClipboardText($"{Player.Position.X:N2}f, {Player.Position.Y:N2}f, {Player.Position.Z:N2}f");
            }
            ImGui.Text(T("Job: {0}", Player.Job));
            ImGui.Text(T("JobId: {0}", (uint)Player.Job));
            ImGui.Text(T("Current Territory/ZoneId: {0}", Player.Territory.RowId));
            if (PlayerHelper.IsInCosmicZone())
            {
                var manager = WKSManager.Instance();
                var currentMission = manager->State.CurrentMission.MissionUnitRowId;

                ImGui.Text(T("Current Mission: {0}", currentMission));
            }
            if (Svc.Targets.Target != null)
            {
                var currentTarget = Svc.Targets.Target;
                if (ImGui.Button(T("Name: {0}", currentTarget.Name)))
                {
                    ImGui.SetClipboardText(currentTarget.Name.ToString());
                }
                if (ImGui.Button(T("Id: {0}", currentTarget.BaseId)))
                {
                    ImGui.SetClipboardText(currentTarget.BaseId.ToString());
                }
                if (ImGui.Button(T("Position: X: {0:N2}, Y: {1:N2}, Z: {2:N2}", currentTarget.Position.X, currentTarget.Position.Y, currentTarget.Position.Z)))
                {
                    ImGui.SetClipboardText($"{currentTarget.Position.X:N2}f, {currentTarget.Position.Y:N2}f, {currentTarget.Position.Z:N2}f");
                }
                ImGui.Text(T("Distance: {0:N2}", Player.DistanceTo(currentTarget)));
            }

            ImGui.Text(T("Items on person: "));
            foreach (var item in ConsumableInfo.GatherFood)
            {
                if (PlayerHelper.GetItemCount(item.Id, out var count) && count > 0)
                    ImGui.Text(T("{0} | {1}", item.Name, item.Id));
            }
            if (ImGui.Button(T("Use Gathering Food")))
            {
                P.TaskManager.Enqueue(() => Task_Gather.UseFood());
            }
            if (ImGui.Button(T("Set all leveling missions")))
            {
                foreach (var mission in C.MissionConfig)
                {
                    if (!CosmicHelper.QuickLevelList.Contains(mission.Key))
                        mission.Value.Enabled = false;
                    else
                        mission.Value.Enabled = true;
                }
                C.SaveDebounced();
            }

            ImGui.SliderUInt(T("Player Level"), ref playerLevel, 10, 100);
            if (ImGui.Button(T("Update best mission")))
            {
                best_LevelMission = LevelTest();
            }
            ImGui.Text(T("Best Mission for leveling: [{0}]", best_LevelMission));


            ClassInfo();

            DroidCheck();

            if (ImGui.CollapsingHeader(T("Test Picto")))
            {
                PictoManager.DrawPicto();
            }

            ImGui.Text(T("Drone Ready: {0}", DroneReady()));

            if (ImGui.Button(T("Use Drone")))
            {
                UseDrone();
            }
            if (ImGui.Button(T("Test Pathing to position")))
            {
                P.TaskManager.Enqueue(() => MovetoFlag());
            }
            ImGui.SameLine();
            if (ImGui.Button(T("Set position: {0:N2}", customDestination)))
            {
                customDestination = Player.Position;
            }

            ImGui.Text(T("Any need repaired: {0}", PlayerHelper.AnyNeedsRepair(99)));
        }

        private static unsafe void ClassInfo()
        {
            ImGui.Text(T("Manipulation Check"));
            Dictionary<uint, uint> ManipClassInfo = new()
            {
                [8] = 4574,
                [9] = 4575,
                [10] = 4576,
                [11] = 4577,
                [12] = 4578,
                [13] = 4579,
                [14] = 4580,
                [15] = 4581,
            };

            foreach (var job in ManipClassInfo)
            {
                var isUnlocked = ActionManager.Instance()->GetActionStatus(ActionType.Action, job.Value, checkRecastActive: false, checkCastingActive: false) is 574 or 586;
                ImGui.Text(T("JobId: {0} | Unlocked: {1}", job.Key, isUnlocked));
                // 573 | Not unlocked??? 
                // 574 | Is unlocked
                // 586 | Is unlocked for current class/ready to use
            }

            var canUseSkill = ActionManager.Instance()->GetActionStatus(ActionType.Action, 272, checkRecastActive: false, checkCastingActive: false);
            ImGui.Text(T("Skill Status [272]: {0}", canUseSkill));

            ImGui.Separator();
            PlayerHelper.UpdateHasManip();

            ImGui.Text(T("Custom Is Busy: {0}", PlayerHelper.CustomIsBusy));
            foreach (var job in PlayerHelper.ManipClassInfo)
            {
                ImGui.Text(T("JobID: {0} | HasUnlocked: {1}", job.Key, job.Value.HasUnlocked));
            }
        }

        private static uint LevelTest()
        {
            uint bestMission = 0;

            foreach (var mission in C.MissionConfig)
            {
                var id = mission.Key;

                if (!CosmicHelper.QuickLevelList.Contains(id))
                    continue;

                if (CosmicHelper.SheetMissionDict.TryGetValue(id, out var missionInfo))
                {
                    var attribute = missionInfo.Attributes;
                    var missionLevel = missionInfo.Level;

                    // if (!missionInfo.Jobs.Contains((uint)Player.Job))
                        // continue;

                    int playerTier = playerLevel >= 90 ? 90 : playerLevel >= 50 ? 50 : 10;

                    if (missionLevel != playerTier)
                        continue;

                    bestMission = id;
                }
            }

            return bestMission;
        }

        private static void DroidCheck()
        {
            if (ImGui.CollapsingHeader(T("Object info")))
            {
                foreach (var obect in Svc.Objects.OrderBy(x => Player.DistanceTo(x.Position)))
                {
                    ImGui.Text(T("Name: {0} : {1}", obect.Name, obect.BaseId));
                }
            }
        }

        private static unsafe bool DroneReady()
        {
            var actionManager = ActionManager.Instance();

            // For regular items
            uint itemId = 50414; // your item ID
            var actionStatus = actionManager->GetActionStatus(
                ActionType.Item,
                itemId
            );

            // actionStatus == 0 means the item is ready to use
            // any other value indicates it's not ready (on cooldown, requirements not met, etc.)
            if (actionStatus == 0)
            {
                return true;
            }
            else if (Task_ArtifactSearch.IsTreasureDetected())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static unsafe void UseDrone()
        {
            uint itemId = 50414;
            var inventoryManager = InventoryManager.Instance();

            // Array of inventory types to check
            var inventoryTypes = new[]
            {
                InventoryType.Inventory1,
                InventoryType.Inventory2,
                InventoryType.Inventory3,
                InventoryType.Inventory4
            };

            foreach (var invType in inventoryTypes)
            {
                var container = inventoryManager->GetInventoryContainer(invType);
                if (container == null) continue;

                for (int i = 0; i < container->Size; i++)
                {
                    var item = container->GetInventorySlot(i);
                    if (item != null && item->ItemId == itemId)
                    {
                        // Use the item from inventory
                        AgentInventoryContext.Instance()->UseItem(item->ItemId, invType, (uint)i, 0);
                        return;
                    }
                }
            }

            // If we get here, item wasn't found
            PluginLog.Warning($"Item {itemId} not found in any inventory container");
        }

        private static Vector3 customDestination = Vector3.Zero;

        private static bool? MovetoFlag()
        {
            if (Task_NavmeshMove.Task_NavTo(customDestination, stayMounted: true).Value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static unsafe uint WorldProgress()
        {
            var wks = WKSManager.Instance();
            if (wks == null)
                return 0;

            return wks->State.DevGrade;
        }
    }
}
