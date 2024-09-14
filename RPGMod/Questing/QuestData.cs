using RoR2;
using RPGMod.Extensions;
using RPGMod.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using Random = System.Random;

namespace RPGMod.Questing
{
    public class QuestData : MessageBase
    {
        public bool Complete { get; private set; }
        public bool Failed { get; private set; }
        public float CompletionTime { get; private set; }
        public float LimitTime { get; private set; }
        public List<Mission> Missions { get; private set; }
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

            _networkUser = networkUser;

            ItemTier rewardTier;

            do
            {
                Reward = GenerateReward(questsCompleted);
                var rewardDef = PickupCatalog.GetPickupDef(Reward);
                rewardTier = ItemCatalog.GetItemDef(rewardDef!.itemIndex).tier;
            } while (rewardTier > Server.MaxAllowedRewardTier);

            var r = new Random();

            switch (rewardTier)
            {
                case ItemTier.Tier1:
                    Missions = GenerateMissionComponents(Math.Min(r.Next(Config.Questing.MinNumMissionsTier1, Config.Questing.MaxNumMissionsTier1), Server.MaxAvailableUniqueMissions), _networkUser);
                    break;

                case ItemTier.Tier2:
                    Missions = GenerateMissionComponents(Math.Min(r.Next(Config.Questing.MinNumMissionsTier2, Config.Questing.MaxNumMissionsTier2), Server.MaxAvailableUniqueMissions), _networkUser);
                    break;

                case ItemTier.Tier3:
                    Missions = GenerateMissionComponents(Math.Min(r.Next(Config.Questing.MinNumMissionsTier3, Config.Questing.MaxNumMissionsTier3), Server.MaxAvailableUniqueMissions), _networkUser);
                    break;

                default:
                    RpgMod.Log.LogError($"Reward tier not supported: {rewardTier}");

                    break;
            }

            var timeLimit = Config.Questing.TimerBase + (Missions.Count - 1) * Config.Questing.TimerExtra;
            LimitTime = Run.instance.GetRunStopwatch() + timeLimit;

            if (!Config.UI.SendNewQuestAnnouncement)
            {
                return;
            }

            var message = new Announcement(string.Format(Messages.NewQuestAnnouncement, _networkUser.GetNetworkPlayerName().GetResolvedName()));

            Networking.SendAnnouncement(message, _networkUser.connectionToClient.connectionId);
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Guid);
            writer.Write(Complete);
            writer.Write(Failed);
            writer.Write(CompletionTime);
            writer.Write(LimitTime);
            writer.Write(Reward);
            writer.Write(Missions.Count);

            foreach (var questComponent in Missions)
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
            Missions = new List<Mission>();

            var questCount = reader.ReadInt32();
            for (var i = 0; i < questCount; i++)
            {
                Missions.Add(reader.ReadMission());
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

            var pickupList = weightedSelection.Evaluate(UnityEngine.Random.value);
            var pickupIndex = pickupList.Random();

            return pickupIndex;
        }

        public List<Mission> GenerateMissionComponents(int amount, NetworkUser networkUser)
        {
            var questComponents = new List<Mission>();
            var usedTypes = new List<(MissionType, string)>();

            for (var i = 0; i < amount; i++)
            {
                MissionType missionType;
                string missionSpecification;

                do
                {
                    missionType = Server.AllowedTypes.Random();

                    missionSpecification = missionType switch
                    {
                        MissionType.KillSpecificName => Server.AllowedMonsterTypes.Random(),
                        MissionType.KillSpecificBuff => Server.AllowedBuffTypes.Random(),
                        _ => null
                    };
                } while (usedTypes.Contains((missionType, missionSpecification)));

                usedTypes.Add((missionType, missionSpecification));
                questComponents.Add(new Mission(missionType, missionSpecification, networkUser));
            }

            return questComponents;
        }

        public void Check()
        {
            if (Complete)
            {
                return;
            }

            if (Missions.Count(x => x.IsCompleted) == Missions.Count)
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
            if (Complete)
            {
                return;
            }

            Server.CompletedQuest(_networkUser);

            Complete = true;
            CompletionTime = Run.instance.GetRunStopwatch();

            var player = PlayerCharacterMasterController.instances.Single(x => x.networkUser == _networkUser);
            var reward = PickupCatalog.GetPickupDef(Reward);

            player.master.inventory.GiveItem(reward!.itemIndex);

            if (Config.UI.SendQuestCompleteAnnouncement)
            {
                var message = new Announcement(string.Format(Messages.QuestCompleteAnnouncement, _networkUser.GetNetworkPlayerName().GetResolvedName()));

                Networking.SendAnnouncement(message, _networkUser.connectionToClient.connectionId);
            }

            Networking.SendItemReceivedMessage(new ItemReceived(reward.itemIndex), _networkUser.connectionToClient.connectionId);
        }

        public void FailQuest()
        {
            if (Complete)
            {
                return;
            }

            Server.FailQuest(_networkUser);

            Complete = true;
            Failed = true;
            CompletionTime = Run.instance.GetRunStopwatch();

            foreach (var mission in Missions)
            {
                mission.Abort();
            }

            if (!Config.UI.SendQuestFailedAnnouncement)
            {
                return;
            }

            var message = new Announcement(string.Format(Messages.QuestFailedAnnouncement, _networkUser.GetNetworkPlayerName().GetResolvedName()));

            Networking.SendAnnouncement(message, _networkUser.connectionToClient.connectionId);
        }
    }
}