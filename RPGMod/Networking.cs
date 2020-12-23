using UnityEngine.Networking;
using System.Collections.Generic;
using RoR2;

namespace RPGMod {
public static class Networking {
    private static float lastUpdate;
    private static void RegisterHandlers() {
        NetworkClient client = NetworkManager.singleton.client;

        client.RegisterHandler(Config.Networking.msgType, Questing.PlayerData.Handler);
        client.RegisterHandler((short)(Config.Networking.msgType + 1), Questing.Announcement.Handler);
    }
    public static void Setup() {
        lastUpdate = 0;
        RegisterHandlers();
    }
    public static void Sync() {
        if (Config.Networking.updateRate == 0 || Run.instance.GetRunStopwatch() - lastUpdate >= (Config.Networking.updateRate / 1000)) {
            foreach (var playerData in Questing.Server.PlayerDatas) {
                NetworkServer.SendToClient(playerData.connectionId, Config.Networking.msgType, playerData);
            }

            lastUpdate = Run.instance.GetRunStopwatch();
        }
    }
    // NetworkWriter methods
    public static void Write(this NetworkWriter writer, Questing.QuestType questType)
    {
        writer.Write((int)questType);
    }
    public static void Write(this NetworkWriter writer, Questing.QuestComponent questComponent)
    {
        writer.Write(questComponent.questType);
        writer.Write(Questing.QuestComponent.getComplete(questComponent));
        writer.Write(questComponent.Progress);
        writer.Write(questComponent.objective);

    }
    public static void SendAnnouncement(Questing.Announcement message, int connectionId)
    {
        NetworkServer.SendToClient(connectionId, (short)(Config.Networking.msgType + 1), message);
    }
    // NetworkReader methods
    public static Questing.QuestComponent ReadQuestComponent(this NetworkReader reader)
    {
        return new Questing.QuestComponent(
            reader.ReadQuestType(),
            reader.ReadBoolean(),
            reader.ReadInt32(),
            reader.ReadInt32()
        );
    }
    public static Questing.QuestType ReadQuestType(this NetworkReader reader) {
        return (Questing.QuestType)reader.ReadInt32();
    }
}
} // namespace RPGMod