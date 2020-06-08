using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace RPGMod.Questing
{
    public static class QuestUpdateHandler
    {
        // Updates the relevant quest according to the parameters.
        public static void UpdateQuest(int value, QuestType questType, string target)
        {
            if (Core.DebugMode)
            {
                Debug.Log("Quest Updating [VALUE = " + value + "] [TYPE = " + questType + "] [TARGET = " + target + "]");
            }

            for (var i = 0; i < ClientMessage.Instances.Count; i++)
            {
                if (Core.DebugMode)
                {
                    Debug.Log("Quest Checking [TYPE = " + ServerMessage.Instances[i].QuestType + "] [TARGET = " + ClientMessage.Instances[i].Target + "]");
                    Debug.Log(ServerMessage.Instances[i].QuestType == questType);
                    Debug.Log(ClientMessage.Instances[i].Target == target);
                }

                if (ServerMessage.Instances[i].QuestType != questType || 
                    ClientMessage.Instances[i].Target != target ||
                    !ClientMessage.Instances[i].Active)
                {
                    continue;
                }

                var newServerData = ServerMessage.Instances[i];
                newServerData.Progress += value;
                ServerMessage.Instances[i] = newServerData;
                ClientMessage.Instances[i].Description = Quest.GetDescription(ClientMessage.Instances[i], ServerMessage.Instances[i]);
                CheckQuestStatus(i);
                if (Core.DebugMode)
                {
                    Debug.Log("UPDATED QUEST");
                }
                ClientMessage.Instances[i].SendToAll();
            }
        }

        // Checks if the quest at the index has fulfilled the objective.
        private static void CheckQuestStatus(int index)
        {
            if (ServerMessage.Instances[index].Progress < ServerMessage.Instances[index].Objective ||
                !ClientMessage.Instances[index].Active)
            {
                return;
            }

            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player.master.IsDeadAndOutOfLivesServer())
                {
                    continue;
                }

                var transform = player.master.GetBody().coreTransform;
                if (Config.DropItemsFromPlayers)
                {
                    PickupDropletController.CreatePickupDroplet(ServerMessage.Instances[index].Drop.pickupIndex, transform.position, transform.forward * 10f);
                }
                else
                {
                    player.master.inventory.GiveItem(ServerMessage.Instances[index].Drop.itemIndex);
                }
            }
            ClientMessage.Instances[index].Active = false;
        }

        public static bool HookOnCharacterDeath(bool ignoreDeath, DamageReport damageReport)
        {
            if (ignoreDeath)
            {
                return false;
            }

            var enemyBody = damageReport.victimBody;
            var attackerMaster = damageReport.damageInfo.attacker.GetComponent<CharacterBody>().masterObject;
            var attackerController = attackerMaster.GetComponent<CharacterMaster>();

            UpdateQuest(1, 0, enemyBody.GetUserName());

            if (!Config.EnemyItemDropsEnabled)
            {
                return false;
            }

            var isElite = enemyBody.isElite || enemyBody.isChampion;
            var isBoss = enemyBody.isBoss;
            float chance;

            if (isBoss)
            {
                chance = Config.BossDropChance;
            }
            else
            {
                chance = isElite ? Config.EliteDropChance : Config.NormalDropChance;
            }

            if (enemyBody.isElite)
            {
                UpdateQuest(1, QuestType.KillElites, "Elites");
            }

            chance *= 1 - Config.PlayerChanceScaling + Config.PlayerChanceScaling * Run.instance.participatingPlayerCount;

            if (Config.EarlyChanceScaling - Run.instance.difficultyCoefficient > 0)
            {
                chance *= Config.EarlyChanceScaling - (Run.instance.difficultyCoefficient - 1);
            }

            // rng check
            var didDrop = Util.CheckRoll(chance, attackerController ? attackerController.luck : 0f);

            // Gets Item and drops in world
            if (!didDrop || isBoss)
            {
                return false;
            }

            // Create a weighted selection for rng
            var weightedSelection = new WeightedSelection<List<PickupIndex>>();

            // Check if enemy elite or normal
            if (isElite)
            {
                weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.EliteChanceCommon);
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.EliteChanceUncommon);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.EliteChanceLegendary);
                weightedSelection.AddChoice(Run.instance.availableLunarDropList, Config.EliteChanceLunar);
            }
            else
            {
                weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.NormalChanceCommon);
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.NormalChanceUncommon);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.NormalChanceLegendary);
                weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, Config.NormalChanceEquip);
            }

            // Get a Tier
            var list = weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);

            // Pick random from tier
            var item = PickupCatalog.GetPickupDef(list[Run.instance.spawnRng.RangeInt(0, list.Count)]);

            // Spawn item
            if (Config.DropItemsFromEnemies || list == Run.instance.availableEquipmentDropList)
            {
                PickupDropletController.CreatePickupDroplet(item.pickupIndex, enemyBody.transform.position, Vector3.up * 20f);
            }
            else
            {
                if (attackerController.minionOwnership.ownerMaster != null)
                {
                    attackerController.minionOwnership.ownerMaster.inventory.GiveItem(item.itemIndex);
                }
                else
                {
                    attackerController.inventory.GiveItem(item.itemIndex);
                }

                Chat.AddMessage("Enemy Drop: 1 " + Language.GetString(item.nameToken));
            }

            return false;
        }
    }
}