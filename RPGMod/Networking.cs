using UnityEngine.Networking;
using RoR2;

namespace RPGMod {
static class Networking {
    private static float lastUpdate = 0;
    public static void RegisterHandlers() {
        NetworkClient client = NetworkManager.singleton.client;

        client.RegisterHandler(Config.Networking.questPort, Questing.PlayerData.Handler);
    }
    public static void Sync() {
        if (Config.Networking.updateRate == 0 || Run.instance.GetRunStopwatch() - lastUpdate >= Config.Networking.updateRate) {
            foreach(var playerData in Questing.Server.PlayerDatas) {
                NetworkServer.SendToClient(playerData.connectionId, Config.Networking.questPort, playerData);
            }
            lastUpdate = Run.instance.GetRunStopwatch();
        }
    }
}
} // namespace RPGMod