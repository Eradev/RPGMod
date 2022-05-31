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
        public static void Write(this NetworkWriter writer, QuestType questType)
        {
            writer.Write((int)questType);
        }

        public static void Write(this NetworkWriter writer, QuestComponent questComponent)
        {
            writer.Write(questComponent.QuestType);
            writer.Write(QuestComponent.GetComplete(questComponent));
            writer.Write(questComponent.Progress);
            writer.Write(questComponent.Objective);

        }

        public static void SendAnnouncement(Announcement message, int connectionId)
        {
            NetworkServer.SendToClient(connectionId, (short)(Config.Networking.msgType + 1), message);
        }

        // NetworkReader methods
        public static QuestComponent ReadQuestComponent(this NetworkReader reader)
        {
            return new QuestComponent(
                reader.ReadQuestType(),
                reader.ReadBoolean(),
                reader.ReadInt32(),
                reader.ReadInt32()
            );
        }

        public static QuestType ReadQuestType(this NetworkReader reader)
        {
            return (QuestType)reader.ReadInt32();
        }
    }
}