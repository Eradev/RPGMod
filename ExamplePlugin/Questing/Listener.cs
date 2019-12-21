using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMod
{
    namespace Questing
    {
        public static class Listener
        {
            // Updates the relevant quest according to the parameters.
            public static void UpdateQuest(int value, Type type, string target)
            {
                if (MainDefs.AdvancingStage) {
                    return;
                }

                Debug.Log("Quest Updating [VALUE = " + value + "] [TYPE = " + type + "] [TARGET = " + target + "]");

                for (var i = 0; i < MainDefs.QuestServerMessages.Count; i++)
                {
                    Debug.Log("Quest Checking [TYPE = " + MainDefs.QuestServerMessages[i].type + "] [TARGET = " + MainDefs.QuestClientMessages[i].target + "]");
                    Debug.Log(MainDefs.QuestServerMessages[i].type == type);
                    Debug.Log(MainDefs.QuestClientMessages[i].target == target);

                    if (MainDefs.QuestServerMessages[i].type == type && MainDefs.QuestClientMessages[i].target == target)
                    {
                        ServerMessage newServerData = MainDefs.QuestServerMessages[i];
                        newServerData.progress += value;
                        MainDefs.QuestServerMessages[i] = newServerData;
                        MainDefs.QuestClientMessages[i].description = Quest.GetDescription(MainDefs.QuestClientMessages[i], MainDefs.QuestServerMessages[i]);
                        CheckQuestStatus(i);
                        Debug.Log("UPDATED QUEST");
                        Quest.SendQuest(MainDefs.QuestClientMessages[i]);
                    }
                }
            }

            // Checks if the quest at the index has fulfilled the objective.
            private static void CheckQuestStatus(int index)
            {
                if (MainDefs.QuestServerMessages[index].progress >= MainDefs.QuestServerMessages[index].objective && MainDefs.QuestClientMessages[index].active)
                {
                    foreach (var player in PlayerCharacterMasterController.instances)
                    {
                        if (player.master.alive)
                        {
                            var transform = player.master.GetBody().coreTransform;
                            if (Config.dropItemsFromPlayers)
                            {
                                PickupDropletController.CreatePickupDroplet(MainDefs.QuestServerMessages[index].drop.pickupIndex, transform.position, transform.forward * 10f);
                            }
                            else
                            {
                                player.master.inventory.GiveItem(MainDefs.QuestServerMessages[index].drop.itemIndex);
                            }
                        }
                    }
                    MainDefs.QuestClientMessages[index].active = false;
                }
            }

            public static bool HookOnCharacterDeath(bool ignoreDeath, DamageReport damageReport) {
                if (!ignoreDeath)
                {
                    float chance;
                    CharacterBody enemyBody = damageReport.victimBody;
                    GameObject attackerMaster = damageReport.damageInfo.attacker.GetComponent<CharacterBody>().masterObject;
                    CharacterMaster attackerController = attackerMaster.GetComponent<CharacterMaster>();

                    Questing.Listener.UpdateQuest(1, 0, enemyBody.GetUserName());

                    if (Questing.Config.enemyItemDropsEnabled)
                    {
                        bool isElite = enemyBody.isElite || enemyBody.isChampion;
                        bool isBoss = enemyBody.isBoss;

                        if (isBoss)
                        {
                            chance = Questing.Config.bossDropChance;
                        }
                        else
                        {
                            if (isElite)
                            {
                                chance = Questing.Config.eliteDropChance;
                            }
                            else
                            {
                                chance = Questing.Config.normalDropChance;
                            }
                        }

                        if (enemyBody.isElite)
                        {
                            Questing.Listener.UpdateQuest(1, Type.KillElites, "Elites");
                        }

                        chance *= ((1 - Questing.Config.playerChanceScaling) + (Questing.Config.playerChanceScaling * Run.instance.participatingPlayerCount));
                        if (Questing.Config.earlyChanceScaling - Run.instance.difficultyCoefficient > 0)
                        {
                            chance *= (Questing.Config.earlyChanceScaling - (Run.instance.difficultyCoefficient - 1));
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
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, Questing.Config.eliteChanceCommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, Questing.Config.eliteChanceUncommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, Questing.Config.eliteChanceLegendary);
                                    weightedSelection.AddChoice(Run.instance.availableLunarDropList, Questing.Config.eliteChanceLunar);
                                }
                                else
                                {
                                    weightedSelection.AddChoice(Run.instance.availableTier1DropList, Questing.Config.normalChanceCommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier2DropList, Questing.Config.normalChanceUncommon);
                                    weightedSelection.AddChoice(Run.instance.availableTier3DropList, Questing.Config.normalChanceLegendary);
                                    weightedSelection.AddChoice(Run.instance.availableEquipmentDropList, Questing.Config.normalChanceEquip);
                                }
                                // Get a Tier
                                List<PickupIndex> list = weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);
                                // Pick random from tier
                                PickupIndex item = list[Run.instance.spawnRng.RangeInt(0, list.Count)];
                                // Spawn item
                                PickupDropletController.CreatePickupDroplet(item, enemyBody.transform.position, Vector3.up * 20f);
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