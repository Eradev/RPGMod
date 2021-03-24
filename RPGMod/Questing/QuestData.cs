using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace RPGMod
{
namespace Questing
{
public class QuestData : MessageBase
{
    public bool complete {get; private set;}
    public float completionTime {get; private set;}
    public List<QuestComponent> questComponents {get; private set;}
    public PickupIndex reward {get; private set;}
    public int guid {get; private set;}
    private NetworkUser networkUser;
    public QuestData() {
        complete = false;
        completionTime = 0;
    }
    public QuestData(NetworkUser networkUser, int questsCompleted, int oldGuid) : this()
    {
        // astronomically low chance that the guid will be equal but im paranoid
        do {
            guid = Guid.NewGuid().GetHashCode();
        } while (guid == oldGuid);

        this.networkUser = networkUser;

        reward = GenerateReward(questsCompleted);

        ItemTier rewardTier;
        do {
            rewardTier = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(reward).itemIndex).tier;
        } while (!((int)rewardTier < Server.AllowedTypes.Count));

        switch (rewardTier) {
            case ItemTier.Tier1:
                questComponents = GenerateQuestComponents(1, this.networkUser);
                break;
            case ItemTier.Tier2:
                questComponents = GenerateQuestComponents(2, this.networkUser);
                break;
            case ItemTier.Tier3:
                questComponents = GenerateQuestComponents(3, this.networkUser);
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
            this.networkUser.GetNetworkPlayerName().GetResolvedName(),
            String.Join(", ", missions),
            ColorUtility.ToHtmlStringRGBA(PickupCatalog.GetPickupDef(reward).baseColor),
            Language.GetString(PickupCatalog.GetPickupDef(reward).nameToken)));

        Networking.SendAnnouncement(message, this.networkUser.connectionToClient.connectionId);
    }
    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(guid);
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
        guid = reader.ReadInt32();
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
        Questing.Client.QuestData = networkMessage.ReadMessage<Questing.QuestData>();
    }
    private PickupIndex GenerateReward(int questsCompleted) {
        WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>();

        weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.Questing.chanceCommon - (questsCompleted * Config.Questing.chanceAdjustmentPercent));
        weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.Questing.chanceUncommon + (questsCompleted * Config.Questing.chanceAdjustmentPercent / 2));
        weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.Questing.chanceLegendary + (questsCompleted * Config.Questing.chanceAdjustmentPercent / 2));

        List<PickupIndex> pickupList = weightedSelection.Evaluate(UnityEngine.Random.value);
        PickupIndex pickupIndex = pickupList[UnityEngine.Random.Range(0, pickupList.Count)];

        return pickupIndex;
    }
    public List<QuestComponent> GenerateQuestComponents(int amount, NetworkUser networkUser) {
        List<QuestComponent> questComponents = new List<QuestComponent>();
        List<QuestType> usedTypes = new List<QuestType>();
        for (int i = 0; i < amount; i++) {
            QuestType questType;
            do {
                questType = Server.AllowedTypes[Run.instance.runRNG.RangeInt(0, Server.AllowedTypes.Count)];
            } while (usedTypes.Contains(questType));
            usedTypes.Add(questType);
            questComponents.Add(new QuestComponent(questType, networkUser));
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
        Server.CompletedQuest(networkUser);
        complete = true;
        completionTime = Run.instance.GetRunStopwatch();
        foreach (var player in PlayerCharacterMasterController.instances) {
            if (player.networkUser == networkUser) {
                player.master.inventory.GiveItem(PickupCatalog.GetPickupDef(reward).itemIndex);
                Announcement message = new Announcement(
                    String.Format("Good work <b><color=orange>{0}</color></b>, you have been rewarded.", networkUser.GetNetworkPlayerName().GetResolvedName())
                );
                Networking.SendAnnouncement(message, networkUser.connectionToClient.connectionId);
            }
        }
    }
}

} // namespace Questing
} // namespace RPGMod