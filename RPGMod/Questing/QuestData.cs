using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod.Questing
{
    public class QuestData : MessageBase
    {
        public bool Complete { get; private set; }
        public float CompletionTime { get; private set; }
        public List<Mission> QuestComponents { get; private set; }
        public PickupIndex Reward { get; private set; }
        public int Guid { get; private set; }

        private readonly NetworkUser networkUser;

        public QuestData()
        {
            Complete = false;
            CompletionTime = 0;
        }

        public QuestData(NetworkUser networkUser, int questsCompleted, int oldGuid) : this()
        {
            // Astronomically low chance that the GUID will be equal but I'm paranoid
            do
            {
                Guid = System.Guid.NewGuid().GetHashCode();
            } while (Guid == oldGuid);

            this.networkUser = networkUser;

            ItemTier rewardTier;
            PickupDef rewardDef;
            do
            {
                Reward = GenerateReward(questsCompleted);
                rewardDef = PickupCatalog.GetPickupDef(Reward);
                rewardTier = ItemCatalog.GetItemDef(rewardDef!.itemIndex).tier;
            } while (!((int)rewardTier < Server.AllowedTypes.Count));

            switch (rewardTier)
            {
                case ItemTier.Tier1:
                    QuestComponents = GenerateQuestComponents(1, this.networkUser);
                    break;

                case ItemTier.Tier2:
                    QuestComponents = GenerateQuestComponents(2, this.networkUser);
                    break;

                case ItemTier.Tier3:
                    QuestComponents = GenerateQuestComponents(3, this.networkUser);
                    break;

                default:
                    RPGMod.Log.LogError(rewardTier);
                    break;
            }

            var missions = QuestComponents.Select(questComponent => UI.Quest.QuestTypeDict[questComponent.MissionType]).ToList();

            var message = new Announcement(
                $"Alright <b><color=orange>{this.networkUser.GetNetworkPlayerName().GetResolvedName()}</color></b>, we'll be needing you to do these missions: <b>({string.Join(", ", missions)}),</b> to receive <b><color=#{ColorUtility.ToHtmlStringRGBA(rewardDef.baseColor)}>{Language.GetString(rewardDef.nameToken)}</color></b>");

            Networking.SendAnnouncement(message, this.networkUser.connectionToClient.connectionId);
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Guid);
            writer.Write(Complete);
            writer.Write(CompletionTime);
            writer.Write(Reward);
            writer.Write(QuestComponents.Count);

            foreach (var questComponent in QuestComponents)
            {
                writer.Write(questComponent);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            Guid = reader.ReadInt32();
            Complete = reader.ReadBoolean();
            CompletionTime = reader.ReadSingle();
            Reward = reader.ReadPickupIndex();
            QuestComponents = new List<Mission>();

            var questCount = reader.ReadInt32();
            for (var i = 0; i < questCount; i++)
            {
                QuestComponents.Add(reader.ReadMission());
            }
        }

        public static void Handler(NetworkMessage networkMessage)
        {
            Client.QuestData = networkMessage.ReadMessage<QuestData>();
        }

        private PickupIndex GenerateReward(int questsCompleted)
        {
            var weightedSelection = new WeightedSelection<List<PickupIndex>>();

            weightedSelection.AddChoice(Blacklist.AvailableTier1DropList, Config.Questing.chanceCommon - questsCompleted * Config.Questing.chanceAdjustmentPercent);
            weightedSelection.AddChoice(Blacklist.AvailableTier2DropList, Config.Questing.chanceUncommon + questsCompleted * Config.Questing.chanceAdjustmentPercent / 2);
            weightedSelection.AddChoice(Blacklist.AvailableTier3DropList, Config.Questing.chanceLegendary + questsCompleted * Config.Questing.chanceAdjustmentPercent / 2);

            var pickupList = weightedSelection.Evaluate(Random.value);
            var pickupIndex = pickupList[Random.Range(0, pickupList.Count)];

            return pickupIndex;
        }

        public List<Mission> GenerateQuestComponents(int amount, NetworkUser networkUser)
        {
            var questComponents = new List<Mission>();
            var usedTypes = new List<MissionType>();

            for (var i = 0; i < amount; i++)
            {
                MissionType missionType;
                do
                {
                    missionType = Server.AllowedTypes[Run.instance.runRNG.RangeInt(0, Server.AllowedTypes.Count)];

                } while (usedTypes.Contains(missionType));

                usedTypes.Add(missionType);
                questComponents.Add(new Mission(missionType, networkUser));
            }

            return questComponents;
        }

        public void Check()
        {
            if (QuestComponents.Count(x => x.IsCompleted) == QuestComponents.Count)
            {
                CompleteQuest();
            }
        }

        public void CompleteQuest()
        {
            Server.CompletedQuest(networkUser);
            Complete = true;
            CompletionTime = Run.instance.GetRunStopwatch();

            var player = PlayerCharacterMasterController.instances.Single(x => x.networkUser == networkUser);
            var reward = PickupCatalog.GetPickupDef(Reward);

            player.master.inventory.GiveItem(reward.itemIndex);

            var message = new Announcement(
                $"Good work <b><color=orange>{networkUser.GetNetworkPlayerName().GetResolvedName()}</color></b>, you have been rewarded with <b>{Language.GetString(reward.nameToken)}</b>."
            );

            Networking.SendAnnouncement(message, networkUser.connectionToClient.connectionId);
            Networking.SendItemReceivedMessage(new ItemReceived(reward.itemIndex), networkUser.connectionToClient.connectionId);
        }
    }
}