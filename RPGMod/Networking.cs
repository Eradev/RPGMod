using RoR2;
using RPGMod.Questing;
using UnityEngine.Networking;

namespace RPGMod
{
    public static class Networking
    {
        private static float _lastUpdate;

        private static void RegisterHandlers()
        {
            var client = NetworkManager.singleton.client;

            client.RegisterHandler(Config.Networking.MsgType, QuestData.Handler);
            client.RegisterHandler((short)(Config.Networking.MsgType + 1), Announcement.Handler);
            client.RegisterHandler((short)(Config.Networking.MsgType + 2), ItemReceived.Handler);
        }

        public static void Setup()
        {
            _lastUpdate = 0;

            RegisterHandlers();
        }

        public static void Sync()
        {
            if (Config.Networking.UpdateRate != 0 && !(Run.instance.GetRunStopwatch() - _lastUpdate >= Config.Networking.UpdateRate / 1000f))
            {
                return;
            }

            foreach (var clientData in Server.ClientDatas)
            {
                NetworkServer.SendToClient(clientData.NetworkUser.connectionToClient.connectionId, Config.Networking.MsgType, clientData.QuestData);
            }

            _lastUpdate = Run.instance.GetRunStopwatch();
        }

        // NetworkWriter methods
        public static void Write(this NetworkWriter writer, MissionType missionType)
        {
            writer.Write((int)missionType);
        }

        public static void Write(this NetworkWriter writer, Mission mission)
        {
            writer.Write(mission.MissionType);
            writer.Write(mission.IsCompleted);
            writer.Write(mission.Progress);
            writer.Write(mission.Objective);
        }

        public static void SendAnnouncement(Announcement message, int connectionId)
        {
            NetworkServer.SendToClient(connectionId, (short)(Config.Networking.MsgType + 1), message);
        }

        public static void SendItemReceivedMessage(ItemReceived itemReceived, int connectionId)
        {
            NetworkServer.SendToClient(connectionId, (short)(Config.Networking.MsgType + 2), itemReceived);
        }

        // NetworkReader methods
        public static Mission ReadMission(this NetworkReader reader)
        {
            return new Mission(
                reader.ReadMissionType(),
                reader.ReadBoolean(),
                reader.ReadInt32(),
                reader.ReadInt32()
            );
        }

        public static MissionType ReadMissionType(this NetworkReader reader)
        {
            return (MissionType)reader.ReadInt32();
        }
    }
}