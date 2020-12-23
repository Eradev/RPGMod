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
public class QuestComponent {
    private int progress;
    public int objective { get; private set; }
    public bool complete { get; private set; }
    private NetworkInstanceId netId;
    public QuestType questType { get; private set; }
    public int Progress { get { return progress; } set { if (!complete) { progress = value; if (progress >= objective) { complete = true; RemoveListener(); Handler.CheckPlayerData(netId); } } } }

    private QuestComponent() {
        complete = false;
        progress = 0;
    }
    public QuestComponent(QuestType questType, NetworkInstanceId netId) : this() {
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
    public QuestComponent(QuestType questType, bool complete, int progress, int objective) {
        this.questType = questType;
        this.complete = complete;
        this.progress = progress;
        this.objective = objective;
    }
    private int GenerateObjective(QuestType questType) {
        int objective;
        // TODO: Scale values for difficulty
        switch (questType) {
            case QuestType.killCommon:
                objective = UnityEngine.Random.Range(Config.Questing.killCommonMin, Config.Questing.killCommonMax);
                break;
            case QuestType.killElite:
                objective = UnityEngine.Random.Range(Config.Questing.killEliteMin, Config.Questing.killEliteMax);
                break;
            case QuestType.collectGold:
                objective = Run.instance.GetDifficultyScaledCost(UnityEngine.Random.Range(Config.Questing.collectGoldMin, Config.Questing.collectGoldMax));
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
    public static bool getComplete(QuestComponent questComponent)
    {
        return questComponent.complete;
    }
}
public class Announcement : MessageBase
{
    public String message { get; private set; }
    public Announcement() {
        this.message = null;
    }
    public Announcement(String message) {
        this.message = message;
    }
    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(message);
    }
    public override void Deserialize(NetworkReader reader)
    {
        message = reader.ReadString();
    }
    public static void Handler(NetworkMessage networkMessage) {
        Client.announcement = networkMessage.ReadMessage<Questing.Announcement>();
        Client.announced = true;
    }
}
public class PlayerData : MessageBase
{
    public bool complete {get; private set;}
    public float completionTime {get; private set;}
    public int connectionId {get; private set;}
    public List<QuestComponent> questComponents {get; private set;}
    public PickupIndex reward {get; private set;}
    public NetworkInstanceId netId {get; private set;}
    public PlayerData() {
        complete = false;
        completionTime = 0;
        reward = GenerateReward();
    }

    public PlayerData(int connectionId) : this()
    {
        this.connectionId = connectionId;

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

        List<String> missions = new List<string>();
        foreach (var questComponent in questComponents) {
            missions.Add(QuestUI.questTypeDict[questComponent.questType]);
        }

        Announcement message = new Announcement(string.Format(
            "Alright <b><color=orange>{0}</color></b>, we'll be needing you to do these missions: <b>({1}),</b> to receive <b><color=#{2}>{3}</color></b>",
            currentUser.GetNetworkPlayerName().GetResolvedName(),
            String.Join(", ", missions),
            ColorUtility.ToHtmlStringRGBA(PickupCatalog.GetPickupDef(reward).baseColor),
            Language.GetString(PickupCatalog.GetPickupDef(reward).nameToken)));

        Networking.SendAnnouncement(message, connectionId);
    }
    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(complete);
        writer.Write(completionTime);
        writer.Write(reward);
        writer.Write(questComponents.Count);
        for (int i = 0; i<questComponents.Count; i++) {
            writer.Write(questComponents[i]);
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
            questComponents.Add(reader.ReadQuestComponent());
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
        if (questComponents.FindAll(QuestComponent.getComplete).Count == questComponents.Count)
        {
            CompleteQuest();
        }
    }
    public void CompleteQuest()
    {
        complete = true;
        completionTime = Run.instance.GetRunStopwatch();
        foreach (var player in PlayerCharacterMasterController.instances) {
            if (player.networkUser.netId == netId) {
                player.master.inventory.GiveItem(PickupCatalog.GetPickupDef(reward).itemIndex);
                Announcement message = new Announcement(
                    String.Format("Good work {0}, you have been rewarded.", player.networkUser.GetNetworkPlayerName().GetResolvedName())
                );
                Networking.SendAnnouncement(message, connectionId);
            }
        }
    }
}
static class Client
{
    private static PlayerData playerData = null;
    private static QuestUI questUI;
    public static PlayerData PlayerData
    {
        get {
            return playerData;
        }
        set {
            playerData = value;

            if (questUI == null && !playerData.complete) {
                LocalUser localUser = LocalUserManager.GetFirstLocalUser();

                if (localUser?.cachedBody != null)
                {
                    questUI = localUser.cachedBody.gameObject.AddComponent<QuestUI>();
                }
            }
            if (questUI != null) {
                questUI.UpdateData(playerData);
            }
        }
    }
    public static Announcement announcement;
    public static bool announced = false;
    public static void CleanUp() {
        UnityEngine.Object.Destroy(questUI);
        questUI = null;
        playerData = null;
    }
    public static void Update() {
        if (announcement != null && announced) {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser?.cachedBody != null)
            {
                localUser.cachedBody.gameObject.AddComponent<AnnouncerUI>().SetMessage(announcement.message);
                announced = false;
            }
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
        Questing.Server.PlayerDatas.RemoveAll(BadPlayerData);
        for (int i = 0; i<NetworkServer.connections.Count; i++) {
            if (NetworkServer.connections[i]?.connectionId != null && NetworkServer.connections[i].isReady) {
                int dataIndex = -1;
                // Find existing playerData index
                for (int j = 0; j < Server.PlayerDatas.Count; j++)
                {
                    if (Server.PlayerDatas[j].connectionId == NetworkServer.connections[i].connectionId) {
                        dataIndex = j;
                        break;
                    }
                }
                // If playerData index was found, update if the quest is completed
                if (dataIndex != -1) {
                    if (Server.PlayerDatas[dataIndex].complete && (Run.instance.GetRunStopwatch() - Server.PlayerDatas[dataIndex].completionTime) > Config.Questing.cooldown)
                    {
                        Server.PlayerDatas[dataIndex] = new PlayerData(NetworkServer.connections[i].connectionId);
                    }
                }
                // Otherwise no playerData exists for the player and a new playerData is added
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
    private static bool BadPlayerData(Questing.PlayerData playerData)
    {
        bool bad = true;
        for (int i = 0; i < NetworkServer.connections.Count; i++)
        {
            if (NetworkServer.connections[i]?.connectionId != null && NetworkServer.connections[i].connectionId == playerData.connectionId)
            {
                bad = false;
            }
        }
        return bad;
    }
    public static void CleanUp() {
        Server.PlayerDatas.Clear();
        Client.CleanUp();
        UI.Setup();
        Events.commonKilled.RemoveAllListeners();
        Events.eliteKilled.RemoveAllListeners();
        Events.goldCollected.RemoveAllListeners();
    }
}
} // namespace Questing
} // namespace RPGMod
