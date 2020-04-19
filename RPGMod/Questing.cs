using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Events;

namespace RPGMod
{
namespace Questing
{
public enum QuestType {
    killCommon,
    killElite,
    killChampion,
    collectGold,
    openChests
}
public static class QuestEvents {
    [Serializable]
    public class QuestEvent : UnityEvent<int, short>{

    }

    public static QuestEvent commonKilled = new QuestEvent();
    public static QuestEvent eliteKilled = new QuestEvent();
    public static QuestEvent championKilled = new QuestEvent();
    public static QuestEvent goldCollected = new QuestEvent();
    public static QuestEvent chestOpened = new QuestEvent();
}
class QuestComponent {
    public int progress;
    public int objective;
    public int playerControllerId;
    public QuestType questType;
    public QuestComponent(QuestType questType, short playerControllerId) {
        progress = 0;
        this.questType = questType;
        this.playerControllerId = playerControllerId;
        objective = GenerateObjective(questType);
        switch (questType) {
            case QuestType.killCommon:
                QuestEvents.commonKilled.AddListener(this.Listener);
                break;
            case QuestType.killElite:
                QuestEvents.eliteKilled.AddListener(this.Listener);
                break;
            case QuestType.killChampion:
                QuestEvents.championKilled.AddListener(this.Listener);
                break;
            case QuestType.collectGold:
                QuestEvents.goldCollected.AddListener(this.Listener);
                break;
            case QuestType.openChests:
                QuestEvents.chestOpened.AddListener(this.Listener);
                break;
            default:
                Debug.LogError(questType);
                break;
        }
    }
    public int GenerateObjective(QuestType questType) {
        int objective;
        // TODO: Fill out values
        switch (questType) {
            case QuestType.killCommon:
                objective = 0;
                break;
            case QuestType.killElite:
                objective = 0;
                break;
            case QuestType.killChampion:
                objective = 0;
                break;
            case QuestType.collectGold:
                objective = 0;
                break;
            case QuestType.openChests:
                objective = 0;
                break;
            default:
                objective = 0;
                Debug.LogError(questType);
                break;
        }
        return objective;
    }
    void Listener(int value, short playerControllerId) {
        Debug.Log(value);
        Debug.Log(playerControllerId);
        Debug.Log(this.playerControllerId);
        if (this.playerControllerId == playerControllerId) {
            progress += value;
        }
        Debug.Log(String.Format("{0}/{1}",progress,objective));
    }
}
class PlayerData : MessageBase
{
    public bool complete;
    public float completionTime;
    public int connectionId;
    public List<QuestComponent> questComponents;
    public PickupIndex reward;
    public short playerControllerId;

    public PlayerData() {
        complete = false;
        completionTime = 0;
        playerControllerId = 0;
        reward = GenerateReward();
    }

    public PlayerData(int connectionId) : this()
    {
        this.connectionId = connectionId;
        Debug.Log(connectionId);
        Chat.SimpleChatMessage message = new Chat.SimpleChatMessage();

        NetworkUser currentUser = null;
        foreach (NetworkUser user in NetworkUser.readOnlyInstancesList) {
            if (user?.connectionToClient?.connectionId == this.connectionId) {
                currentUser = user;
            }
        }

        if (!currentUser) {
            Debug.LogError("User not found");
            return;
        }

        playerControllerId = currentUser.playerControllerId;

        ItemTier rewardTier = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(reward).itemIndex).tier;
        switch (rewardTier) {
            case ItemTier.Tier1:
                questComponents = GenerateQuestComponents(1, playerControllerId);
                break;
            case ItemTier.Tier2:
                questComponents = GenerateQuestComponents(2, playerControllerId);
                break;
            case ItemTier.Tier3:
                questComponents = GenerateQuestComponents(3, playerControllerId);
                break;
            default:
                Debug.LogError(rewardTier);
                break;
        }

        message.baseToken = string.Format(
            "Alright {0}, we'll be needing you to do these missions to receive <b><color=#{1}>{2}</color></b>",
            currentUser.GetNetworkPlayerName().GetResolvedName(),
            ColorUtility.ToHtmlStringRGBA(PickupCatalog.GetPickupDef(reward).baseColor),
            Language.GetString(PickupCatalog.GetPickupDef(reward).nameToken));

        Debug.Log(questComponents.Count);

        foreach (var questComponent in questComponents) {
            Debug.Log("NEW COMPONENT");
            Debug.Log(questComponent.questType);
        }

        Chat.SendBroadcastChat(message);
    }
    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(complete);
        writer.Write(completionTime);
        writer.Write(reward);
        writer.Write(questComponents.Count);
        for (int i = 0; i<questComponents.Count; i++) {
            writer.Write(JsonUtility.ToJson(questComponents[i]));
        }
    }
    public override void Deserialize(NetworkReader reader)
    {
        complete = reader.ReadBoolean();
        completionTime = reader.ReadSingle();
        reward = reader.ReadPickupIndex();
        questComponents = new List<QuestComponent>();
        int questCount = reader.ReadInt32();
        for (int i = 0; i<questCount; i++) {
            questComponents.Add(JsonUtility.FromJson<QuestComponent>(reader.ReadString()));
        }
    }
    public static void Handler(NetworkMessage networkMessage) {
        Questing.Client.PlayerData = networkMessage.ReadMessage<Questing.PlayerData>();
    }
    public static PickupIndex GenerateReward() {
        WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>();

        weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.Questing.chanceCommon);
        weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.Questing.chanceUncommon);
        weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.Questing.chanceLegendary);

        List<PickupIndex> pickupList = weightedSelection.Evaluate(UnityEngine.Random.value);
        PickupIndex pickupIndex = pickupList[UnityEngine.Random.Range(0, pickupList.Count)];

        return pickupIndex;
    }
    public static List<QuestComponent> GenerateQuestComponents(int amount, short playerControllerId) {
        List<QuestComponent> questComponents = new List<QuestComponent>();
        List<QuestType> usedTypes = new List<QuestType>();
        for (int i = 0; i < amount; i++) {
            QuestType questType;
            do {
                questType = (QuestType)Run.instance.runRNG.RangeInt(0,5);
            } while (usedTypes.Contains(questType));
            usedTypes.Add(questType);
            questComponents.Add(new QuestComponent(questType, playerControllerId));
        }
        return questComponents;
    }
}
static class Client
{
    private static PlayerData playerData;
    public static PlayerData PlayerData
    {
        get {
            return playerData;
        }
        set {
            playerData = value;
        }
    }
}
static class Server
{
    public static List<PlayerData> PlayerDatas { get; set; } = new List<PlayerData>();
}
static class Handler
{
    public static void Update()
    {
        for (int i = 0; i<NetworkServer.connections.Count; i++) {
            if (NetworkServer.connections[i].isReady) {
                int dataIndex = -1;
                for (int j = 0; j < Server.PlayerDatas.Count; j++)
                {
                    if (Server.PlayerDatas[j].connectionId == NetworkServer.connections[i].connectionId) {
                        dataIndex = j;
                        break;
                    }
                }
                if (dataIndex != -1) {
                    if (Server.PlayerDatas[dataIndex].complete && (Run.instance.GetRunStopwatch() - Server.PlayerDatas[dataIndex].completionTime) > Config.Questing.cooldown)
                    {
                        Server.PlayerDatas[dataIndex] = new PlayerData(NetworkServer.connections[i].connectionId);
                    }
                }
                else {
                    Server.PlayerDatas.Add(new PlayerData(NetworkServer.connections[i].connectionId));
                }
            }
        }
    }
    public static void Setup() {
        Client.PlayerData = null;
    }
    public static void CleanUp() {
        Server.PlayerDatas.Clear();
        Client.PlayerData = null;
        QuestEvents.commonKilled.RemoveAllListeners();
        QuestEvents.eliteKilled.RemoveAllListeners();
        QuestEvents.championKilled.RemoveAllListeners();
        QuestEvents.goldCollected.RemoveAllListeners();
        QuestEvents.chestOpened.RemoveAllListeners();
    }
}
} // namespace Questing
} // namespace RPGMod
