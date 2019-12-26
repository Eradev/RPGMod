using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod
{
    namespace Questing
    {
        public enum Type
        {
            KillEnemies,
            CollectGold,
            OpenChests,
            Heal,
            KillElites
        }

        internal static class Quest
        {
            // Builds the quest description used for messaging data across clients.
            public static string GetDescription(ClientMessage clientMessage, ServerMessage serverMessage)
            {
                return string.Format("{0},{1},{2},{3},{4},{5}", (int)serverMessage.type,
                    string.Format("{0} {1}{2}", serverMessage.objective, clientMessage.target, serverMessage.type == Type.KillEnemies ? "s" : ""),
                    string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(serverMessage.drop.baseColor), Language.GetString(ItemCatalog.GetItemDef(serverMessage.drop.itemIndex).nameToken)),
                    serverMessage.progress, serverMessage.objective, ItemCatalog.GetItemDef(serverMessage.drop.itemIndex).pickupIconPath);
            }

            public static int GetObjectiveLimit()
            {
                int questObjectiveLimit = (int)Math.Round(Config.questObjectiveMin * Run.instance.compensatedDifficultyCoefficient);
                if (questObjectiveLimit >= Config.questObjectiveMax)
                {
                    questObjectiveLimit = Config.questObjectiveMax;
                }
                return questObjectiveLimit;
            }

            public static Type GetQuestType()
            {
                Type type;

                do
                {
                    type = (Type)Core.random.Next(0, Core.questDefinitions.items);
                } while ((Core.usedTypes[type] >= Config.questPerTypeMax) || (type == Type.KillElites && Run.instance.loopClearCount > 0));

                return type;
            }

            // Handles creating a new quest.
            public static ClientMessage GetQuest(int serverMessageIndex = -1)
            {

                Type type;

                if (serverMessageIndex == -1)
                {
                    type = GetQuestType();
                }
                else
                {
                    type = Questing.ServerMessage.Instances[serverMessageIndex].type;
                }

                ClientMessage clientMessage = new ClientMessage(Core.questDefinitions.targets[(int)type]);
                ServerMessage serverMessage = new ServerMessage(type);

                //Debug.Log(type);

                switch (type)
                {
                    // Monster Elimination Quest - [Gets random enemy from scene and sets it as the objective]
                    case Type.KillEnemies:

                        var choices = ClassicStageInfo.instance.monsterSelection.choices;
                        List<WeightedSelection<DirectorCard>.ChoiceInfo> newChoices = new List<WeightedSelection<DirectorCard>.ChoiceInfo>();
                        for (int i = 0; i < choices.Length; i++) {
                            if (!(choices[i].value == null || choices[i].value.spawnCard == null || choices[i].value.spawnCard.name == null))
                            {
                                if (!(choices[i].value.spawnCard.directorCreditCost > 30 && Run.instance.GetRunStopwatch() < (15 * 60)) && !(choices[i].value.spawnCard.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>().isChampion && Run.instance.GetRunStopwatch() < (35 * 60)))
                                {
                                    newChoices.Add(choices[i]);
                                }
                            }
                        } 

                        var targetCard = newChoices[Core.random.Next(0, newChoices.Count)].value.spawnCard;
                        var targetMaster = targetCard.prefab.GetComponent<CharacterMaster>();
                        var targetObject = targetMaster.bodyPrefab;
                        var targetBody = targetObject.GetComponent<CharacterBody>();

                        clientMessage.target = targetBody.GetUserName();
                        clientMessage.iconPath = targetBody.name;
                        break;
                    // Collect Gold Quest - [Gets quest objective according to game difficulty]
                    case Type.CollectGold:
                        serverMessage.objective = (int)Math.Floor(100 * Run.instance.difficultyCoefficient);
                        break;
                    // Heal Quest
                    case Type.Heal:
                        if (serverMessageIndex == -1)
                        {
                            int max = 0;
                            foreach (var player in PlayerCharacterMasterController.instances)
                            {
                                if (max < player.master.GetBody().healthComponent.fullHealth)
                                {
                                    max = (int)player.master.GetBody().healthComponent.fullHealth;
                                }
                            }
                            serverMessage.objective = (int)Math.Floor(max * 1.6f);
                        }
                        break;

                    default:
                        break;
                }

                if (serverMessageIndex != -1)
                {
                    Debug.Log(serverMessageIndex);
                    serverMessage = Questing.ServerMessage.Instances[serverMessageIndex];
                }

                clientMessage.description = GetDescription(clientMessage, serverMessage);

                if (Config.displayQuestsInChat)
                {
                    DisplayQuestInChat(clientMessage, serverMessage);
                }

                Core.usedTypes[serverMessage.type] += 1;
                if (serverMessageIndex == -1)
                {
                    serverMessage.RegisterInstance();
                }
                else {
                    Questing.ServerMessage.Instances[serverMessageIndex].awaitingClientMessage = false;
                }
                clientMessage.id = GetUniqueID();

                return clientMessage;
            }

            public static int GetUniqueID()
            {
                int newID;
                do
                {
                    newID = Core.random.Next();
                } while (Core.usedIDs.Contains(newID));
                Core.usedIDs.Add(newID);
                return newID;
            }

            // Gets the drop for the quest
            public static PickupDef GetQuestDrop()
            {
                WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);

                weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.questChanceCommon);
                weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.questChanceUncommon);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.questChanceLegendary);

                List<PickupIndex> list = weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);
                PickupIndex item = list[Run.instance.spawnRng.RangeInt(0, list.Count)];

                return PickupCatalog.GetPickupDef(item);
            }

            public static void DisplayQuestInChat(ClientMessage clientMessage, ServerMessage serverMessage)
            {
                Chat.SimpleChatMessage message = new Chat.SimpleChatMessage();

                message.baseToken = string.Format("{0} {1} {2}{3} to receive: <color=#{4}>{5}</color>",
                    Core.questDefinitions.types[(int)serverMessage.type],
                    serverMessage.objective,
                    clientMessage.target,
                    serverMessage.type == 0 ? "s" : "",
                    ColorUtility.ToHtmlStringRGBA(serverMessage.drop.baseColor),
                    Language.GetString(ItemCatalog.GetItemDef(serverMessage.drop.itemIndex).nameToken));

                Chat.SendBroadcastChat(message);
            }
        }
    } // namespace Questing
} // namespace RPGMod