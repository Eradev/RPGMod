using System;
using System.Collections.Generic;
using RoR2;
using RPGMod.Utils;
using UnityEngine;

namespace RPGMod.Questing
{
    internal static class Quest
    {
        // Builds the quest description used for messaging data across clients.
        public static string GetDescription(ClientMessage clientMessage, ServerMessage serverMessage) // TODO: Maybe move some of this data to individual message components
        {
            // ReSharper disable UseStringInterpolation
            return string.Format("{0},{1},{2},{3},{4},{5}", (int)serverMessage.QuestType,
                string.Format("{0} {1}{2}", serverMessage.Objective, clientMessage.Target, serverMessage.QuestType == QuestType.KillEnemies ? "s" : ""),
                string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(serverMessage.Drop.baseColor), Language.GetString(ItemCatalog.GetItemDef(serverMessage.Drop.itemIndex).nameToken)),
                serverMessage.Progress, serverMessage.Objective, ItemCatalog.GetItemDef(serverMessage.Drop.itemIndex).pickupIconPath);
            // ReSharper enable UseStringInterpolation
        }

        // Creates the dynamic quest limit
        public static int GetObjectiveLimit(int min, int max, float scale)
        {
            var questObjectiveLimit = (int)Math.Round(min * Run.instance.compensatedDifficultyCoefficient * scale);

            if (questObjectiveLimit >= max)
            {
                questObjectiveLimit = max;
            }
            else if (questObjectiveLimit < min)
            {
                questObjectiveLimit = min;
            }

            return questObjectiveLimit;
        }

        public new static QuestType GetType()
        {
            QuestType questType;

            do
            {
                questType = (QuestType)Core.Random.Next(0, Core.QuestDefinitions.Items);
            }
            while (Core.UsedTypes[questType] >= Config.QuestPerTypeMax || questType == QuestType.KillElites && Run.instance.loopClearCount < 1);

            return questType;
        }

        // Handles quest creation
        public static ClientMessage GetQuest(int serverMessageIndex = -1)
        {
            var questType = serverMessageIndex == -1 ? GetType() : ServerMessage.Instances[serverMessageIndex].QuestType;
            var clientMessage = new ClientMessage(Core.QuestDefinitions.Targets[(int)questType]);
            var serverMessage = new ServerMessage(questType);

            switch (questType)
            {
                // Monster Elimination Quest - [Gets random enemy from scene and sets it as the objective]
                case QuestType.KillEnemies:
                    var choices = ClassicStageInfo.instance.monsterSelection.choices;
                    var newChoices = new List<WeightedSelection<DirectorCard>.ChoiceInfo>();
                    for (var i = 0; i < choices.Length; i++)
                    {
                        if (choices[i].value == null ||
                            choices[i].value.spawnCard == null ||
                            choices[i].value.spawnCard.name == null)
                        {
                            continue;
                        }

                        if (!(choices[i].value.spawnCard.directorCreditCost > 30 && Run.instance.GetRunStopwatch() < 15 * 60) &&
                            !(choices[i].value.spawnCard.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>().isChampion && Run.instance.GetRunStopwatch() < 35 * 60))
                        {
                            newChoices.Add(choices[i]);
                        }
                    }

                    serverMessage.Objective = Core.Random.Next(Config.QuestKillObjectiveMin, GetObjectiveLimit(Config.QuestKillObjectiveMin, Config.QuestKillObjectiveMax, 1));

                    var targetCard = newChoices[Core.Random.Next(0, newChoices.Count)].value.spawnCard;
                    var targetMaster = targetCard.prefab.GetComponent<CharacterMaster>();
                    var targetObject = targetMaster.bodyPrefab;
                    var targetBody = targetObject.GetComponent<CharacterBody>();

                    clientMessage.Target = targetBody.GetUserName();
                    clientMessage.IconPath = targetBody.name;
                    break;
                case QuestType.KillElites:
                    serverMessage.Objective = Core.Random.Next(Config.QuestKillObjectiveMin, GetObjectiveLimit(Config.QuestKillObjectiveMin, Config.QuestKillObjectiveMax, 0.85f));
                    break;
                case QuestType.OpenChests:
                    serverMessage.Objective = Core.Random.Next(Config.QuestUtilityObjectiveMin, GetObjectiveLimit(Config.QuestUtilityObjectiveMin, Config.QuestUtilityObjectiveMax, 0.8f));
                    break;
                // Collect Gold Quest - [Gets quest objective according to game difficulty]
                case QuestType.CollectGold:
                    serverMessage.Objective = (int)Math.Floor(100 * Run.instance.difficultyCoefficient);
                    break;
                // Heal Quest
                case QuestType.Heal:
                    if (serverMessageIndex == -1)
                    {
                        var max = 0;
                        foreach (var player in PlayerCharacterMasterController.instances)
                        {
                            if (max < player.master.GetBody().healthComponent.fullHealth)
                            {
                                max = (int)player.master.GetBody().healthComponent.fullHealth;
                            }
                        }
                        serverMessage.Objective = (int)Math.Floor(max * 1.6f);
                    }
                    break;
            }

            switch (ItemCatalog.GetItemDef(serverMessage.Drop.itemIndex).tier)
            {
                case ItemTier.Tier1:
                    serverMessage.Objective = (int)Math.Floor(serverMessage.Objective * Config.QuestObjectiveCommonMultiplier);
                    break;
                case ItemTier.Tier2:
                    serverMessage.Objective = (int)Math.Floor(serverMessage.Objective * Config.QuestObjectiveUncommonMultiplier);
                    break;
                case ItemTier.Tier3:
                    serverMessage.Objective = (int)Math.Floor(serverMessage.Objective * Config.QuestObjectiveLegendaryMultiplier);
                    break;
            }

            if (serverMessageIndex != -1)
            {
                serverMessage = ServerMessage.Instances[serverMessageIndex];
            }

            clientMessage.Description = GetDescription(clientMessage, serverMessage);

            if (Config.DisplayQuestsInChat)
            {
                DisplayQuestInChat(clientMessage, serverMessage);
            }

            Core.UsedTypes[serverMessage.QuestType] += 1;

            if (serverMessageIndex == -1)
            {
                serverMessage.RegisterInstance();
            }
            else
            {
                ServerMessage.Instances[serverMessageIndex].AwaitingClientMessage = false;
            }

            clientMessage.Id = GetUniqueId();

            return clientMessage;
        }

        public static int GetUniqueId()
        {
            int id;

            do
            {
                id = Core.Random.Next();
            }
            while (Core.UsedIDs.Contains(id));

            Core.UsedIDs.Add(id);

            return id;
        }

        // Gets the drop for the quest
        public static PickupDef GetQuestDrop()
        {
            var weightedSelection = new WeightedSelection<List<PickupIndex>>();

            weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.QuestChanceCommon);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.QuestChanceUncommon);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.QuestChanceLegendary);

            var list = weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);
            var item = list[Run.instance.spawnRng.RangeInt(0, list.Count)];

            return PickupCatalog.GetPickupDef(item);
        }

        public static void DisplayQuestInChat(ClientMessage clientMessage, ServerMessage serverMessage)
        {
            if (serverMessage.Objective - serverMessage.Progress > 0)
            {
                RPGModChat.SendMessage(string.Format("{0} {1} {2}{3} to receive: <color=#{4}>{5}</color> (<color=#58F73B>{6}</color> remaining)",
                    Core.QuestDefinitions.Types[(int)serverMessage.QuestType],
                    serverMessage.Objective,
                    clientMessage.Target,
                    serverMessage.QuestType == 0 ? "s" : "",
                    ColorUtility.ToHtmlStringRGBA(serverMessage.Drop.baseColor),
                    Language.GetString(ItemCatalog.GetItemDef(serverMessage.Drop.itemIndex).nameToken),
                    serverMessage.Objective - serverMessage.Progress));
            }
        }
    }
}