using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace RPGMod
{
namespace Questing
{
public class ClientData : MessageBase
{
    public bool complete {get; private set;}
    public float completionTime {get; private set;}
    public int connectionId {get; private set;}
    public List<QuestComponent> questComponents {get; private set;}
    public PickupIndex reward {get; private set;}
    public NetworkInstanceId netId {get; private set;}
    public ClientData() {
        complete = false;
        completionTime = 0;
    }
    public ClientData(int connectionId) : this()
    {
        this.connectionId = connectionId;

        NetworkUser currentUser = null;
        foreach (NetworkUser user in NetworkUser.readOnlyInstancesList) {
            if (user?.connectionToClient?.connectionId == this.connectionId) {
                currentUser = user;
            }
        }

        if (currentUser is null) {
            Debug.LogError("User not found");
            return;
        }

        netId = currentUser.netId;
        reward = GenerateReward();

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
            missions.Add(UI.Quest.questTypeDict[questComponent.questType]);
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
        Questing.Client.ClientData = networkMessage.ReadMessage<Questing.ClientData>();
    }
    private PickupIndex GenerateReward() {
        WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>();

        weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.Questing.chanceCommon);
        weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.Questing.chanceUncommon);
        weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.Questing.chanceLegendary);

        List<PickupIndex> pickupList = weightedSelection.Evaluate(UnityEngine.Random.value);
        PickupIndex pickupIndex = pickupList[UnityEngine.Random.Range(0, pickupList.Count)];

        return pickupIndex;
    }
    public List<QuestComponent> GenerateQuestComponents(int amount, NetworkInstanceId netId) {
        List<QuestComponent> questComponents = new List<QuestComponent>();
        List<QuestType> usedTypes = new List<QuestType>();
        for (int i = 0; i < amount; i++) {
            QuestType questType;
            do {
                questType = (QuestType)Run.instance.runRNG.RangeInt(0, QuestType.GetNames(typeof(QuestType)).Length);
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
                    String.Format("Good work <b><color=orange>{0}</color></b>, you have been rewarded.", player.networkUser.GetNetworkPlayerName().GetResolvedName())
                );
                Networking.SendAnnouncement(message, connectionId);
            }
        }
    }
}
}
}