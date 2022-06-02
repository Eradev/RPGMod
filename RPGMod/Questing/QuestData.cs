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
        public List<QuestComponent> QuestComponents { get; private set; }
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
            do
            {
                Reward = GenerateReward(questsCompleted);

                rewardTier = ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(Reward).itemIndex).tier;
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
                    Debug.LogError(rewardTier);
                    break;
            }

            var missions = QuestComponents.Select(questComponent => UI.Quest.QuestTypeDict[questComponent.QuestType]).ToList();

            var message = new Announcement(
                $"Alright <b><color=orange>{this.networkUser.GetNetworkPlayerName().GetResolvedName()}</color></b>, we'll be needing you to do these missions: <b>({string.Join(", ", missions)}),</b> to receive <b><color=#{ColorUtility.ToHtmlStringRGBA(PickupCatalog.GetPickupDef(Reward).baseColor)}>{Language.GetString(PickupCatalog.GetPickupDef(Reward).nameToken)}</color></b>");

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
            QuestComponents = new List<QuestComponent>();

            var questCount = reader.ReadInt32();
            for (var i = 0; i < questCount; i++)
            {
                QuestComponents.Add(reader.ReadQuestComponent());
            }
        }

        public static void Handler(NetworkMessage networkMessage)
        {
            Client.QuestData = networkMessage.ReadMessage<QuestData>();
        }

        private PickupIndex GenerateReward(int questsCompleted)
        {
            var weightedSelection = new WeightedSelection<List<PickupIndex>>();

            weightedSelection.AddChoice(Run.instance.availableTier1DropList, Config.Questing.chanceCommon - questsCompleted * Config.Questing.chanceAdjustmentPercent);
            weightedSelection.AddChoice(Run.instance.availableTier2DropList, Config.Questing.chanceUncommon + questsCompleted * Config.Questing.chanceAdjustmentPercent / 2);
            weightedSelection.AddChoice(Run.instance.availableTier3DropList, Config.Questing.chanceLegendary + questsCompleted * Config.Questing.chanceAdjustmentPercent / 2);

            var pickupList = weightedSelection.Evaluate(UnityEngine.Random.value);
            var pickupIndex = pickupList[UnityEngine.Random.Range(0, pickupList.Count)];

            return pickupIndex;
        }

        public List<QuestComponent> GenerateQuestComponents(int amount, NetworkUser networkUser)
        {
            var questComponents = new List<QuestComponent>();
            var usedTypes = new List<QuestType>();

            for (var i = 0; i < amount; i++)
            {
                QuestType questType;
                do
                {
                    questType = Server.AllowedTypes[Run.instance.runRNG.RangeInt(0, Server.AllowedTypes.Count)];

                } while (usedTypes.Contains(questType));

                usedTypes.Add(questType);
                questComponents.Add(new QuestComponent(questType, networkUser));
            }

            return questComponents;
        }

        public void Check()
        {
            if (QuestComponents.FindAll(QuestComponent.GetComplete).Count == QuestComponents.Count)
            {
                CompleteQuest();
            }
        }

        public void CompleteQuest()
        {
            Server.CompletedQuest(networkUser);
            Complete = true;
            CompletionTime = Run.instance.GetRunStopwatch();

            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player.networkUser != networkUser)
                {
                    continue;
                }

                player.master.inventory.GiveItem(PickupCatalog.GetPickupDef(Reward).itemIndex);

                var message = new Announcement(
                    $"Good work <b><color=orange>{networkUser.GetNetworkPlayerName().GetResolvedName()}</color></b>, you have been rewarded."
                );

                Networking.SendAnnouncement(message, networkUser.connectionToClient.connectionId);
            }
        }
    }
}