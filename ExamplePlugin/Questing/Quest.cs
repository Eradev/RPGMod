using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod {
namespace Questing {


static class Quest
{
    
    private static System.Random random = new System.Random();
    private static int QuestObjectiveLimit { get; set; }

    // Builds the quest description used for messaging data accross clients.
    public static string GetDescription(ClientMessage quest, ServerMessage serverData)
    {
        return string.Format("{0},{1},{2},{3},{4},{5}", serverData.type,
            string.Format("{0} {1}{2}", serverData.objective, quest.target, serverData.type == 0 ? "s" : ""),
            string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(serverData.drop.baseColor), Language.GetString(ItemCatalog.GetItemDef(serverData.drop.itemIndex).nameToken)),
            serverData.progress, serverData.objective, ItemCatalog.GetItemDef(serverData.drop.itemIndex).pickupIconPath);
    }

    // Sends the quest to all clients via the quest port.
    public static void SendQuest(ClientMessage Quest)
    {
        if (!NetworkServer.active)
        {
            return;
        }
        NetworkServer.SendToAll(Config.questPort, Quest);
    }

    // Handles creating a new quest.
    public static ClientMessage GetQuest(int specificServerDataIndex = -1)
    {
        QuestObjectiveLimit = (int)Math.Round(Config.questObjectiveMin * Run.instance.compensatedDifficultyCoefficient);

        if (QuestObjectiveLimit >= Config.questObjectiveMax)
        {
            QuestObjectiveLimit = Config.questObjectiveMax;
        }

        ClientMessage questMessage = new ClientMessage();
        questMessage.description = "bad";
        ServerMessage newServerData;
        int num;

        if (specificServerDataIndex == -1)
        {
            num = random.Next(0, MainDefs.questTargets.Count);
            List<int> usedTypes = new List<int>();
            for (int i = 0; i < MainDefs.QuestServerMessages.Count; i++)
            {
                usedTypes.Add(MainDefs.QuestServerMessages[i].type);
            }
            while (usedTypes.Contains(num))
            {
                num = random.Next(0, MainDefs.questTargets.Count);
            }
        }
        else
        {
            num = MainDefs.QuestServerMessages[specificServerDataIndex].type;
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
                        questMessage.description = "bad";
                        return questMessage;
                    }

                    questMessage.target = targetBody.GetUserName();
                    questMessage.iconPath = targetBody.name;
                    break;
                // Collect Gold Quest - [Gets quest objective according to game difficulty]
                case 1:
                    newServerData.objective = (int)Math.Floor(100 * Run.instance.difficultyCoefficient);
                    break;
                // Heal Quest
                case 3:
                    int max = 0;
                    foreach (var player in PlayerCharacterMasterController.instances)
                    {
                        if (max < player.master.GetBody().healthComponent.fullHealth)
                        {
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

        if (questMessage.description == "bad")
        {
            return questMessage;
        }

        if (specificServerDataIndex > -1)
        {
            newServerData = MainDefs.QuestServerMessages[specificServerDataIndex];
        }

        questMessage.description = GetDescription(questMessage, newServerData);

        if (Config.displayQuestsInChat && !MainDefs.stageChanging || Config.restartQuestsOnStageChange)
        {
            Chat.SimpleChatMessage message = new Chat.SimpleChatMessage();

            message.baseToken = string.Format("{0} {1} {2}{3} to receive: <color=#{4}>{5}</color>",
                MainDefs.questTypes[newServerData.type],
                newServerData.objective,
                questMessage.target,
                newServerData.type == 0 ? "s" : "",
                ColorUtility.ToHtmlStringRGBA(newServerData.drop.baseColor),
                Language.GetString(ItemCatalog.GetItemDef(newServerData.drop.itemIndex).nameToken));

            Chat.SendBroadcastChat(message);
        }

        if (specificServerDataIndex == -1)
        {
            MainDefs.QuestServerMessages.Add(newServerData);
        }

        MainDefs.usedTypes.Add(newServerData.type);
        questMessage.id = GetUniqueID();

        return questMessage;
    }

    public static int GetUniqueID()
    {
        int newID = random.Next();
        while (MainDefs.usedIDs.Contains(newID))
        {
            newID = random.Next();
        }
        MainDefs.usedIDs.Add(newID);
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

    // A generic quest creator for non unique quest types.
    public static (ClientMessage, ServerMessage) GenericQuest(string target, int type)
    {
        ClientMessage quest = new ClientMessage
        {
            iconPath = "custom",
            target = target,
            initialised = true
        };

        ServerMessage newServerData = new ServerMessage
        {
            objective = random.Next(Config.questObjectiveMin, QuestObjectiveLimit),
            progress = 0,
            drop = GetQuestDrop(),
            type = type
        };

        return (quest, newServerData);
    }


}
}
}