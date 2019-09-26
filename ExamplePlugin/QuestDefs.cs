using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod
{
    public class QuestDefs
    {
        public System.Random random = new System.Random();
        public int questObjectiveLimit;

        // Builds the quest description used for messaging data accross clients.
        public static string GetDescription(QuestMessage quest, QuestServerData serverData)
        {
            return string.Format("{0},{1},{2},{3},{4},{5}", serverData.type, 
                string.Format("{0} {1}{2}",serverData.objective, quest.questTarget, serverData.type == 0 ? "s" : ""),
                string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(serverData.drop.GetPickupColor()), Language.GetString(ItemCatalog.GetItemDef(serverData.drop.itemIndex).nameToken)),
                serverData.progress, serverData.objective, ItemCatalog.GetItemDef(serverData.drop.itemIndex).pickupIconPath);
        }

        // Sends the quest to all clients via the quest port.
        public static void SendQuest(QuestMessage Quest)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            NetworkServer.SendToAll(MainDefs.questPort, Quest);
        }

        // Handles creating a new quest.
        public QuestMessage GetQuest(int specificServerDataIndex = -1) {
            questObjectiveLimit = (int)Math.Round(ModConfig.questObjectiveMin * Run.instance.compensatedDifficultyCoefficient);

            if (questObjectiveLimit >= ModConfig.questObjectiveMax)
            {
                questObjectiveLimit = ModConfig.questObjectiveMax;
            }

            QuestMessage questMessage = new QuestMessage();
            questMessage.questDescription = "bad";
            QuestServerData newServerData;
            int num;

            if (specificServerDataIndex == -1) {
                num = random.Next(0, MainDefs.questTargets.Count);
                List<int> usedTypes = new List<int>();
                for (int i = 0; i < MainDefs.questsServerData.Count; i++) {
                    usedTypes.Add(MainDefs.questsServerData[i].type);
                }
                while (usedTypes.Contains(num)) {
                    num = random.Next(0, MainDefs.questTargets.Count);
                }
            }
            else {
                num = MainDefs.questsServerData[specificServerDataIndex].type;
            }

            int monstersAlive = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;

            if (monstersAlive > 0)
            {
                (questMessage, newServerData) = GenericQuest(MainDefs.questTargets[num], num);
                switch (num)
                {
                    // Monster Elimination Quest - [Gets random enemy and sets it as the objective]
                    case 0:
                        CharacterBody targetBody = TeamComponent.GetTeamMembers(TeamIndex.Monster)[random.Next(0, monstersAlive)].GetComponent<CharacterBody>();

                        if (targetBody.isBoss || SurvivorCatalog.FindSurvivorDefFromBody(targetBody.master.bodyPrefab) != null)
                        {
                            questMessage.questDescription = "bad";
                            return questMessage;
                        }

                        questMessage.questTarget = targetBody.GetUserName();
                        questMessage.questIconPath = targetBody.name;
                        break;
                    // Collect Gold Quest - [Gets quest objective according to game difficulty]
                    case 1:
                        newServerData.objective = (int)Math.Floor(100 * Run.instance.difficultyCoefficient);
                        break;
                    case 3:
                        newServerData.objective *= 3;
                        break;
                    case 4:
                        int max = 0;
                        foreach (var player in PlayerCharacterMasterController.instances) {
                            if (max < player.master.GetBody().healthComponent.fullHealth) {
                                max = (int)player.master.GetBody().healthComponent.fullHealth;
                            }
                        }
                        newServerData.objective = (int)Math.Floor(max * 1.6f);
                        break;
                }
            }
            else
            {
                return questMessage;
            }

            if (questMessage.questDescription == "bad") {
                return questMessage;
            }

            if (specificServerDataIndex > -1) {
                newServerData = MainDefs.questsServerData[specificServerDataIndex];
            }

            questMessage.questDescription = GetDescription(questMessage, newServerData);

            if (ModConfig.displayQuestsInChat && !MainDefs.stageChanging || ModConfig.restartQuestsOnStageChange)
            {
                Chat.SimpleChatMessage message = new Chat.SimpleChatMessage();

                message.baseToken = string.Format("{0} {1} {2}{3} to receive: <color=#{4}>{5}</color>",
                    MainDefs.questTypes[newServerData.type],
                    newServerData.objective,
                    questMessage.questTarget,
                    newServerData.type == 0 ? "s" : "",
                    ColorUtility.ToHtmlStringRGBA(newServerData.drop.GetPickupColor()),
                    Language.GetString(ItemCatalog.GetItemDef(newServerData.drop.itemIndex).nameToken));

                Chat.SendBroadcastChat(message);
            }

            if (specificServerDataIndex == -1)
            {
                MainDefs.questsServerData.Add(newServerData);
            }

            MainDefs.usedTypes.Add(newServerData.type);
            questMessage.questID = GetUniqueID();

            return questMessage;
        }

        public int GetUniqueID() {
            int newID = random.Next();
            while (MainDefs.usedIDs.Contains(newID))
            {
                newID = random.Next();
            }
            MainDefs.usedIDs.Add(newID);
            return newID;
        }

        // Gets the drop for the quest
        public PickupIndex GetQuestDrop()
        {
            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);

            weightedSelection.AddChoice(Run.instance.availableTier1DropList, ModConfig.questChanceCommon);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, ModConfig.questChanceUncommon);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, ModConfig.questChanceLegendary);

            List<PickupIndex> list = weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);
            PickupIndex item = list[Run.instance.spawnRng.RangeInt(0, list.Count)];

            return item;
        }

        // A generic quest creator for non unique quest types.
        public (QuestMessage, QuestServerData) GenericQuest(string target, int type)
        {
            QuestMessage quest = new QuestMessage
            {
                questIconPath = "custom",
                questTarget = target,
                questInitialised = true
            };
            QuestServerData newServerData = new QuestServerData
            {
                objective = random.Next(ModConfig.questObjectiveMin, questObjectiveLimit),
                progress = 0,
                drop = GetQuestDrop(),
                type = type
            };
            return (quest, newServerData);
        }
    }
}
