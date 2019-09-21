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
        public List<string> questTypes = new List<string>() { "<b>Kill</b>" };
        public int questObjectiveLimit;

        // Builds the string for the quest description
        public string GetDescription(QuestMessage Quest, QuestServerData ServerData)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0} {1} {2}s.", questTypes[ServerData.Type], ServerData.Objective, Quest.questTarget));
            sb.AppendLine(string.Format("<b>Progress:</b> {0}/{1}", ServerData.Progress, ServerData.Objective));
            sb.AppendLine(string.Format("<b>Reward:</b> <color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(ServerData.Drop.GetPickupColor()), Language.GetString(ItemCatalog.GetItemDef(ServerData.Drop.itemIndex).nameToken)));
            return sb.ToString();
        }

        public QuestMessage GetQuest(int index) {
            questObjectiveLimit = (int)Math.Round(ModConfig.questObjectiveMin * Run.instance.compensatedDifficultyCoefficient);

            if (questObjectiveLimit >= ModConfig.questObjectiveMax)
            {
                questObjectiveLimit = ModConfig.questObjectiveMax;
            }

            QuestMessage questMessage = new QuestMessage();
            QuestServerData newServerData = new QuestServerData();
            int num = 0;

            if (index == -1) {
                num = random.Next(0, 0);
            }
            else {
                num = MainDefs.questsServerData[index].Type;
            }

            switch (num)
            {
                case 0:
                    int monstersAlive = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;
                    if (monstersAlive > 0)
                    {
                        questMessage = QuestElim();
                    }
                    else {
                        questMessage.questDescription = "bad";
                        return questMessage; }
                    break;
            }

            if (!MainDefs.awaitingSetQuests)
            {
                newServerData.Objective = random.Next(ModConfig.questObjectiveMin, questObjectiveLimit);
                newServerData.Progress = 0;
                newServerData.Drop = GetQuestDrop();
                newServerData.Type = 0;
            }
            else
            {
                newServerData = MainDefs.questsServerData[index];
            }

            questMessage.questDescription = GetDescription(questMessage, newServerData);

            if (ModConfig.displayQuestsInChat && !MainDefs.stageChanging || ModConfig.restartQuestsOnStageChange)
            {
                Chat.SimpleChatMessage message = new Chat.SimpleChatMessage();

                message.baseToken = string.Format("{0} {1} {2}s to receive: <color=#{3}>{4}</color>",
                    questTypes[newServerData.Type],
                    newServerData.Objective,
                    questMessage.questTarget,
                    ColorUtility.ToHtmlStringRGBA(newServerData.Drop.GetPickupColor()),
                    Language.GetString(ItemCatalog.GetItemDef(newServerData.Drop.itemIndex).nameToken));

                Chat.SendBroadcastChat(message);
            }

            if (!MainDefs.awaitingSetQuests)
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

        public QuestMessage QuestElim()
        {
            QuestMessage quest = new QuestMessage();
            quest.questDescription = "bad";
            int monstersAlive = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;

            CharacterBody targetBody = TeamComponent.GetTeamMembers(TeamIndex.Monster)[random.Next(0, monstersAlive)].GetComponent<CharacterBody>();

            if (targetBody.isBoss || SurvivorCatalog.FindSurvivorDefFromBody(targetBody.master.bodyPrefab) != null)
            {
                return quest;
            }

            quest.questTarget = targetBody.GetUserName();
            quest.questTargetName = targetBody.name;
            quest.questInitialised = true;
            return quest;
        }
    }
}
