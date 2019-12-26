using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMod
{
    namespace Questing
    {
        // Handles quest updates
        public static class Listener
        {
            // Methods

            // Updates the relevant quest according to the parameters.
            public static void UpdateQuest(int value, Type type, string target)
            {
                //Debug.Log("Quest Updating [VALUE = " + value + "] [TYPE = " + type + "] [TARGET = " + target + "]");

                for (var i = 0; i < ClientMessage.Instances.Count; i++)
                {
                    //Debug.Log("Quest Checking [TYPE = " + ServerMessage.Instances[i].type + "] [TARGET = " + ClientMessage.Instances[i].target + "]");
                    //Debug.Log(ServerMessage.Instances[i].type == type);
                    //Debug.Log(ClientMessage.Instances[i].target == target);

                    if (ServerMessage.Instances[i].type == type && ClientMessage.Instances[i].target == target && ClientMessage.Instances[i].active)
                    {
                        ServerMessage newServerData = ServerMessage.Instances[i];
                        newServerData.progress += value;
                        ServerMessage.Instances[i] = newServerData;
                        ClientMessage.Instances[i].description = Quest.GetDescription(ClientMessage.Instances[i], ServerMessage.Instances[i]);
                        CheckQuestStatus(i);
                        if (Core.debugMode)
                        {
                            Debug.Log("UPDATED QUEST");
                        }
                        ClientMessage.Instances[i].SendToAll();
                    }
                }
            }

            // Checks if the quest at the index has fulfilled the objective.
            private static void CheckQuestStatus(int index)
            {
                if (ServerMessage.Instances[index].progress >= ServerMessage.Instances[index].objective && ClientMessage.Instances[index].active)
                {
                    foreach (var player in PlayerCharacterMasterController.instances)
                    {
                        if (player.master.alive)
                        {
                            var transform = player.master.GetBody().coreTransform;
                            if (Config.dropItemsFromPlayers)
                            {
                                PickupDropletController.CreatePickupDroplet(ServerMessage.Instances[index].drop.pickupIndex, transform.position, transform.forward * 10f);
                            }
                            else
                            {
                                player.master.inventory.GiveItem(ServerMessage.Instances[index].drop.itemIndex);
                            }
                        }
                    }
                    ClientMessage.Instances[index].active = false;
                }
            }

            public static bool HookOnCharacterDeath(bool ignoreDeath, DamageReport damageReport) {
                if (!ignoreDeath)
                {
                    float chance;
                    CharacterBody enemyBody = damageReport.victimBody;
                    GameObject attackerMaster = damageReport.damageInfo.attacker.GetComponent<CharacterBody>().masterObject;
                    CharacterMaster attackerController = attackerMaster.GetComponent<CharacterMaster>();

                    Listener.UpdateQuest(1, 0, enemyBody.GetUserName());

                    if (Config.enemyItemDropsEnabled)
                    {
                        bool isElite = enemyBody.isElite || enemyBody.isChampion;
                        bool isBoss = enemyBody.isBoss;

                        if (isBoss)
                        {
                            chance = Config.bossDropChance;
                        }
                        else
                        {
                            if (isElite)
                            {
                                chance = Config.eliteDropChance;
                            }
                            else
                            {
                                chance = Config.normalDropChance;
                            }
                        }

                        if (enemyBody.isElite)
                        {
                            Listener.UpdateQuest(1, Type.KillElites, "Elites");
                        }

                        chance *= ((1 - Config.playerChanceScaling) + (Config.playerChanceScaling * Run.instance.participatingPlayerCount));
                        if (Config.earlyChanceScaling - Run.instance.difficultyCoefficient > 0)
                        {
                            chance *= (Config.earlyChanceScaling - (Run.instance.difficultyCoefficient - 1));
                        }

                        // rng check
                        bool didDrop = Util.CheckRoll(chance, attackerController ? attackerController.luck : 0f, null);

                        // Gets Item and drops in world
                        if (didDrop)
                        {
                            if (!isBoss)
                            {
                                // Create a weighted selection for rng
                                WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
                                // Check if enemy is boss, elite or normal
                                if (isElite)
                                {
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.eliteChanceCommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.eliteChanceUncommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.eliteChanceLegendary);
                                    weightedSelection.AddChoice(Run.instance.availableLunarDropList, Config.eliteChanceLunar);
                                }
                                else
                                {
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.normalChanceCommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.normalChanceUncommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.normalChanceLegendary);
                                    weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, Config.normalChanceEquip);
                                }
                                // Get a Tier
                                List<PickupIndex> list = weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);
                                // Pick random from tier
                                PickupDef item = PickupCatalog.GetPickupDef(list[Run.instance.spawnRng.RangeInt(0, list.Count)]);
                                // Spawn item
                                if (Config.dropItemsFromEnemies || list == Run.instance.availableEquipmentDropList)
                                {
                                    PickupDropletController.CreatePickupDroplet(item.pickupIndex, enemyBody.transform.position, Vector3.up * 20f);
                                }
                                else {
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
                            }
                        }
                    }
                }
                else
                {
                    ignoreDeath = false;
                }
                return ignoreDeath;
            }
        }
    } // namespace Questing
} // namespace RPGMod