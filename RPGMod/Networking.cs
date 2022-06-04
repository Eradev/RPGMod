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

            client.RegisterHandler(Config.Networking.msgType, QuestData.Handler);
            client.RegisterHandler((short)(Config.Networking.msgType + 1), Announcement.Handler);
            client.RegisterHandler((short)(Config.Networking.msgType + 2), ItemReceived.Handler);
        }

        public static void Setup()
        {
            _lastUpdate = 0;

            RegisterHandlers();
        }

        public static void Sync()
        {
            if (Config.Networking.updateRate != 0 && !(Run.instance.GetRunStopwatch() - _lastUpdate >= Config.Networking.updateRate / 1000f))
            {
                return;
            }

            foreach (var clientData in Server.ClientDatas)
            {
                NetworkServer.SendToClient(clientData.networkUser.connectionToClient.connectionId, Config.Networking.msgType, clientData.QuestData);
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
            NetworkServer.SendToClient(connectionId, (short)(Config.Networking.msgType + 1), message);
        }

        public static void SendItemReceivedMessage(ItemReceived itemReceived, int connectionId)
        {
            NetworkServer.SendToClient(connectionId, (short)(Config.Networking.msgType + 2), itemReceived);
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