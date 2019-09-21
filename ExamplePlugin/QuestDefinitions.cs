using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod
{
    public class QuestDefinitions
    {
        public System.Random random = new System.Random();
        public List<string> questTypes = new List<string>() { "<b>Kill</b>" };
        

        // Builds the string for the quest description
        public string GetDescription(QuestMessage Quest, QuestServerData ServerData)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("{0} {1} {2}s.", questTypes[ServerData.Type], ServerData.Objective, Quest.questTarget));
            sb.AppendLine(string.Format("<b>Progress:</b> {0}/{1}", ServerData.Progress, ServerData.Objective));
            sb.AppendLine(string.Format("<b>Reward:</b> <color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(ServerData.Drop.GetPickupColor()), Language.GetString(ItemCatalog.GetItemDef(ServerData.Drop.itemIndex).nameToken)));
            return sb.ToString();
        }

        public QuestMessage GetQuest() {
            switch (random.Next(0, 0))
            {
                case 0: return QuestElim();
            }
            return new QuestMessage();
        }

        // Gets the drop for the quest
        public PickupIndex GetQuestDrop()
        {
            WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);

            weightedSelection.AddChoice(Run.instance.availableTier1DropList, ModConfig.chanceQuestingCommon);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, ModConfig.chanceQuestingUnCommon);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, ModConfig.chanceQuestingLegendary);

            List<PickupIndex> list = weightedSelection.Evaluate(Run.instance.spawnRng.nextNormalizedFloat);
            PickupIndex item = list[Run.instance.spawnRng.RangeInt(0, list.Count)];

            return item;
        }

        public QuestMessage QuestElim()
        {
            QuestMessage Quest = new QuestMessage();
            Quest.questDescription = "bad";
            QuestServerData newServerData = new QuestServerData();
            int monstersAlive = TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;

            if (monstersAlive > 0)
            {
                CharacterBody targetBody = TeamComponent.GetTeamMembers(TeamIndex.Monster)[random.Next(0, monstersAlive)].GetComponent<CharacterBody>();

                if (targetBody.isBoss || SurvivorCatalog.FindSurvivorDefFromBody(targetBody.master.bodyPrefab) != null)
                {
                    return Quest;
                }

                Quest.questTarget = targetBody.GetUserName();
                Quest.questTargetName = targetBody.name;
                int upperObjectiveLimit = (int)Math.Round(ModConfig.questObjectiveFactor * Run.instance.compensatedDifficultyCoefficient);

                if (upperObjectiveLimit >= ModConfig.questObjectiveLimit)
                {
                    upperObjectiveLimit = ModConfig.questObjectiveLimit;
                }

                if (!GlobalDefs.stageChange || GlobalDefs.questFirst || ModConfig.isQuestResetting)
                {
                    newServerData.Objective = random.Next(ModConfig.questObjectiveFactor, upperObjectiveLimit);
                    newServerData.Progress = 0;
                    newServerData.Drop = GetQuestDrop();
                }

                Quest.questInitialised = true;
                newServerData.Type = 0;
                Quest.questDescription = GetDescription(Quest, newServerData);

                if (ModConfig.questInChat)
                {
                    Chat.SimpleChatMessage message = new Chat.SimpleChatMessage();
                    message.baseToken = string.Format("Eliminate {0} {1}s to receive: <color=#{2}>{3}</color>",
                        newServerData.Objective,
                        Quest.questTarget,
                        ColorUtility.ToHtmlStringRGBA(newServerData.Drop.GetPickupColor()),
                        Language.GetString(ItemCatalog.GetItemDef(newServerData.Drop.itemIndex).nameToken));
                    Chat.SendBroadcastChat(message);
                }

                GlobalDefs.questFirst = false;
                GlobalDefs.stageChange = false;
                GlobalDefs.QuestsServerData.Add(newServerData);
            }
            return Quest;
        }
    }
}
