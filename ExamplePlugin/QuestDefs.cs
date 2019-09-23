using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod
{
    public class QuestDefs
    {
        public System.Random random = new System.Random();
        public int questObjectiveLimit;

        // Builds the string for the quest description
        public string GetDescription(QuestMessage Quest, QuestServerData ServerData)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}", ServerData.type, 
                string.Format("{0} {1}{2}",ServerData.objective, Quest.questTarget, Quest.type == 0 ? "s" : ""),
                string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(ServerData.drop.GetPickupColor()), Language.GetString(ItemCatalog.GetItemDef(ServerData.drop.itemIndex).nameToken)),
                ServerData.progress, ServerData.objective, ItemCatalog.GetItemDef(ServerData.drop.itemIndex).pickupIconPath));
            return sb.ToString();
        }

        public QuestMessage GetQuest(int specificSeverDataIndex = -1) {
            questObjectiveLimit = (int)Math.Round(ModConfig.questObjectiveMin * Run.instance.compensatedDifficultyCoefficient);

            if (questObjectiveLimit >= ModConfig.questObjectiveMax)
            {
                questObjectiveLimit = ModConfig.questObjectiveMax;
            }

            QuestMessage questMessage = new QuestMessage();
            QuestServerData newServerData = new QuestServerData();
            int num;

            if (specificSeverDataIndex == -1) {
                num = random.Next(0, 2);
            }
            else {
                num = MainDefs.questsServerData[specificSeverDataIndex].type;
            }

            int monstersAlive = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;

            if (monstersAlive > 0)
            {
                switch (num)
                {
                    case 0:
                        (questMessage, newServerData) = QuestElim();
                        break;
                    case 1:
                        (questMessage, newServerData) = QuestGold();
                        break;
                }
            }
            else
            {
                questMessage.questDescription = "bad";
                return questMessage;
            }

            newServerData.type = questMessage.type;

            if (specificSeverDataIndex > -1) {
                newServerData = MainDefs.questsServerData[specificSeverDataIndex];
            }

            questMessage.questDescription = GetDescription(questMessage, newServerData);

            if (ModConfig.displayQuestsInChat && !MainDefs.stageChanging || ModConfig.restartQuestsOnStageChange)
            {
                Chat.SimpleChatMessage message = new Chat.SimpleChatMessage();

                message.baseToken = string.Format("{0} {1} {2} to receive: <color=#{3}>{4}</color>",
                    MainDefs.questTypes[newServerData.type],
                    newServerData.objective,
                    questMessage.questTarget,
                    ColorUtility.ToHtmlStringRGBA(newServerData.drop.GetPickupColor()),
                    Language.GetString(ItemCatalog.GetItemDef(newServerData.drop.itemIndex).nameToken));

                Chat.SendBroadcastChat(message);
            }

            if (specificSeverDataIndex == -1)
            {
                MainDefs.questsServerData.Add(newServerData);
            }

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

        public (QuestMessage, QuestServerData) QuestElim()
        {
            QuestMessage quest = new QuestMessage();
            QuestServerData newServerData = new QuestServerData();
            int monstersAlive = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;
            CharacterBody targetBody = TeamComponent.GetTeamMembers(TeamIndex.Monster)[random.Next(0, monstersAlive)].GetComponent<CharacterBody>();

            if (targetBody.isBoss || SurvivorCatalog.FindSurvivorDefFromBody(targetBody.master.bodyPrefab) != null)
            {
                return (quest, newServerData);
            }

            newServerData.objective = random.Next(ModConfig.questObjectiveMin, questObjectiveLimit);
            newServerData.progress = 0;
            newServerData.drop = GetQuestDrop();

            quest.type = 0;
            quest.questTarget = targetBody.GetUserName();
            quest.questTargetName = targetBody.name;
            quest.questInitialised = true;
            return (quest, newServerData);
        }

        public (QuestMessage, QuestServerData) QuestGold()
        {
            QuestMessage quest = new QuestMessage();
            QuestServerData newServerData = new QuestServerData();
            newServerData.objective = (int)Math.Floor(100 * Run.instance.difficultyCoefficient) * Run.instance.participatingPlayerCount;
            newServerData.progress = 0;
            newServerData.drop = GetQuestDrop();
            quest.type = 1;
            quest.questTargetName = "Textures/AchievementIcons/texPlaceholderAchievement";
            quest.questTarget = "Gold";
            quest.questInitialised = true;
            return (quest, newServerData);
        }
    }
}
