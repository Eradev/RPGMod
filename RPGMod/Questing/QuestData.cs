using RoR2;
using RPGMod.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RPGMod.Questing
{
    public class QuestData : MessageBase
    {
        public bool Complete { get; private set; }
        public bool Failed { get; private set; }
        public float CompletionTime { get; private set; }
        public float LimitTime { get; private set; }
        public List<Mission> QuestComponents { get; private set; }
        public PickupIndex Reward { get; private set; }
        public int Guid { get; private set; }

        private readonly NetworkUser _networkUser;

        public QuestData()
        {
            Complete = false;
            Failed = false;
            LimitTime = 0;
            CompletionTime = 0;
        }

        public QuestData(NetworkUser networkUser, int questsCompleted, int oldGuid) : this()
        {
            // Astronomically low chance that the GUID will be equal, but I'm paranoid
            do
            {
                Guid = System.Guid.NewGuid().GetHashCode();
            } while (Guid == oldGuid);

            this._networkUser = networkUser;

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
                    QuestComponents = GenerateQuestComponents(1, this._networkUser);
                    break;

                case ItemTier.Tier2:
                    QuestComponents = GenerateQuestComponents(2, this._networkUser);
                    break;

                case ItemTier.Tier3:
                    QuestComponents = GenerateQuestComponents(3, this._networkUser);
                    break;

                default:
                    RpgMod.Log.LogError(rewardTier);
                    break;
            }

            var missions = QuestComponents.Select(questComponent => UI.Quest.QuestTypeDict[questComponent.MissionType]).ToList();


            var timeLimit = Config.Questing.TimerBase + (QuestComponents.Count - 1) * Config.Questing.TimerExtra;
            LimitTime = Run.instance.GetRunStopwatch() + timeLimit;

            if (!Config.UI.SendNewQuestAnnouncement)
            {
                return;
            }

            var message = new Announcement(
                $"Alright <b><color=orange>{this._networkUser.GetNetworkPlayerName().GetResolvedName()}</color></b>, we'll be needing you to do these missions: <b>({string.Join(", ", missions)}),</b> to receive <b><color=#{ColorUtility.ToHtmlStringRGBA(rewardDef.baseColor)}>{Language.GetString(rewardDef.nameToken)}</color></b>");

            Networking.SendAnnouncement(message, this._networkUser.connectionToClient.connectionId);
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Guid);
            writer.Write(Complete);
            writer.Write(Failed);
            writer.Write(CompletionTime);
            writer.Write(LimitTime);
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
            Failed = reader.ReadBoolean();
            CompletionTime = reader.ReadSingle();
            LimitTime = reader.ReadSingle();
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

            weightedSelection.AddChoice(Blacklist.AvailableTier1DropList, Config.Questing.ChanceCommon - questsCompleted * Config.Questing.ChanceAdjustmentPercent);
            weightedSelection.AddChoice(Blacklist.AvailableTier2DropList, Config.Questing.ChanceUncommon + questsCompleted * Config.Questing.ChanceAdjustmentPercent / 2);
            weightedSelection.AddChoice(Blacklist.AvailableTier3DropList, Config.Questing.ChanceLegendary + questsCompleted * Config.Questing.ChanceAdjustmentPercent / 2);

            var pickupList = weightedSelection.Evaluate(Random.value);
            var pickupIndex = pickupList.Random();

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
                    missionType = Server.AllowedTypes.Random();
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
            else if (Run.instance.GetRunStopwatch() > LimitTime)
            {
                FailQuest();
            }
        }

        public void CompleteQuest()
        {
            Server.CompletedQuest(_networkUser);
            Complete = true;
            CompletionTime = Run.instance.GetRunStopwatch();

            var player = PlayerCharacterMasterController.instances.Single(x => x.networkUser == _networkUser);
            var reward = PickupCatalog.GetPickupDef(Reward);

            player.master.inventory.GiveItem(reward!.itemIndex);

            if (Config.UI.SendQuestCompleteAnnouncement)
            {
                var message = new Announcement(
                    $"Good work <b><color=orange>{_networkUser.GetNetworkPlayerName().GetResolvedName()}</color></b>, you have been rewarded with <b>{Language.GetString(reward.nameToken)}</b>."
                );

                Networking.SendAnnouncement(message, _networkUser.connectionToClient.connectionId);
            }

            Networking.SendItemReceivedMessage(new ItemReceived(reward.itemIndex), _networkUser.connectionToClient.connectionId);
        }

        public void FailQuest()
        {
            Server.FailQuest(_networkUser);

            Complete = true;
            Failed = true;
            CompletionTime = Run.instance.GetRunStopwatch();

            if (!Config.UI.SendQuestFailedAnnouncement)
            {
                return;
            }

            var message = new Announcement(
                $"I'm disappointed in you <b><color=orange>{_networkUser.GetNetworkPlayerName().GetResolvedName()}</color></b>. Maybe you'll do better next time..."
            );

            Networking.SendAnnouncement(message, _networkUser.connectionToClient.connectionId);
        }
    }
}