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
    collectGold
}
public static class Events {
    public class QuestEvent : UnityEvent<int, NetworkInstanceId>{}

    public static QuestEvent commonKilled = new QuestEvent();
    public static QuestEvent eliteKilled = new QuestEvent();
    public static QuestEvent goldCollected = new QuestEvent();
}
class QuestComponent {
    private int progress;
    private bool complete;
    private int Progress { get { return progress; } set { if (!complete) { progress = value; if (progress >= objective) { Complete = true; } } } }
    private int objective;
    private NetworkInstanceId netId;
    public QuestType questType { get; private set; }
    public bool Complete { get { return complete; } private set { complete = value; if (complete) { RemoveListener(); Handler.CheckPlayerData(netId); } } }
    public QuestComponent(QuestType questType, NetworkInstanceId netId) {
        progress = 0;
        complete = false;
        this.questType = questType;
        this.netId = netId;
        objective = GenerateObjective(questType);
        switch (questType) {
            case QuestType.killCommon:
                Events.commonKilled.AddListener(Listener);
                break;
            case QuestType.killElite:
                Events.eliteKilled.AddListener(Listener);
                break;
            case QuestType.collectGold:
                Events.goldCollected.AddListener(Listener);
                break;
            default:
                Debug.LogError(questType);
                break;
        }
    }
    private int GenerateObjective(QuestType questType) {
        int objective;
        // TODO: Fill out values
        switch (questType) {
            case QuestType.killCommon:
                objective = 1;
                break;
            case QuestType.killElite:
                objective = 1;
                break;
            case QuestType.collectGold:
                objective = 10;
                break;
            default:
                objective = 1;
                Debug.LogError(questType);
                break;
        }
        return objective;
    }
    void Listener(int value, NetworkInstanceId netId) {
        if (this.netId == netId) {
            Progress += value;
        }
        Debug.Log(String.Format("{0}/{1}",progress,objective));
    }
    private void RemoveListener() {
        switch (questType) {
            case QuestType.killCommon:
                Events.commonKilled.RemoveListener(Listener);
                break;
            case QuestType.killElite:
                Events.eliteKilled.RemoveListener(Listener);
                break;
            case QuestType.collectGold:
                Events.goldCollected.RemoveListener(Listener);
                break;
        }
    }
    public static bool isComplete(QuestComponent questComponent)
    {
        return questComponent.Complete;
    }
}
class PlayerData : MessageBase
{
    public bool complete {get; private set;}
    public float completionTime {get; private set;}
    public int connectionId {get; private set;}
    private List<QuestComponent> questComponents;
    private PickupIndex reward;
    public NetworkInstanceId netId {get; private set;}

    public PlayerData() {
        complete = false;
        completionTime = 0;
        reward = GenerateReward();
    }

    public PlayerData(int connectionId) : this()
    {
        Debug.Log("CONNECTION ID");
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

        netId = currentUser.netId;

        ItemTier rewardTier = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(reward).itemIndex).tier;
        switch (rewardTier) {
            case ItemTier.Tier1:
                questComponents = GenerateQuestComponents(1, netId);
                break;
            case ItemTier.Tier2:
                questComponents = GenerateQuestComponents(2, netId);
                break;
            case ItemTier.Tier3:
                questComponents = GenerateQuestComponents(3, netId);
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
            // Lazy so using json
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
    private static PickupIndex GenerateReward() {
        WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>();

        weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.Questing.chanceCommon);
        weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.Questing.chanceUncommon);
        weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.Questing.chanceLegendary);

        List<PickupIndex> pickupList = weightedSelection.Evaluate(UnityEngine.Random.value);
        PickupIndex pickupIndex = pickupList[UnityEngine.Random.Range(0, pickupList.Count)];

        return pickupIndex;
    }
    public static List<QuestComponent> GenerateQuestComponents(int amount, NetworkInstanceId netId) {
        List<QuestComponent> questComponents = new List<QuestComponent>();
        List<QuestType> usedTypes = new List<QuestType>();
        for (int i = 0; i < amount; i++) {
            QuestType questType;
            do {
                questType = (QuestType)Run.instance.runRNG.RangeInt(0,3);
            } while (usedTypes.Contains(questType));
            usedTypes.Add(questType);
            questComponents.Add(new QuestComponent(questType, netId));
        }
        return questComponents;
    }
    public void Check() {
        questComponents.RemoveAll(QuestComponent.isComplete);
        if (questComponents.Count == 0)
        {
            CompleteQuest();
            foreach (var player in PlayerCharacterMasterController.instances) {
                Debug.Log(player.networkUser.netId);
                if (player.networkUser.netId == netId) {
                    player.master.inventory.GiveItem(PickupCatalog.GetPickupDef(reward).itemIndex);
                }
            }
            Chat.SimpleChatMessage message = new Chat.SimpleChatMessage();
            message.baseToken = "Good work, you have been rewarded.";
            Chat.SendBroadcastChat(message);
        }
    }
    public void CompleteQuest()
    {
        complete = true;
        completionTime = Run.instance.GetRunStopwatch();
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
    public static void CheckPlayerData(NetworkInstanceId netId)
    {
        for (int i = 0; i < Server.PlayerDatas.Count; i++)
        {
            if (Server.PlayerDatas[i].netId == netId)
            {
                Server.PlayerDatas[i].Check();
            }
        }
    }
    public static void Setup() {
        Client.PlayerData = null;
    }
    public static void CleanUp() {
        Server.PlayerDatas.Clear();
        Client.PlayerData = null;
        Events.commonKilled.RemoveAllListeners();
        Events.eliteKilled.RemoveAllListeners();
        Events.goldCollected.RemoveAllListeners();
    }
}
} // namespace Questing
} // namespace RPGMod
