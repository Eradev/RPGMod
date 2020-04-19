using UnityEngine.Networking;

namespace RPGMod {
static class Networking {
    public static void RegisterHandlers() {
        NetworkClient client = NetworkManager.singleton.client;

        client.RegisterHandler(Config.Questing.port, Questing.PlayerData.Handler);
    }
    public static void Sync() {
        foreach(var playerData in Questing.Server.PlayerDatas) {
            NetworkServer.SendToClient(playerData.connectionId, Config.Questing.port, playerData);
        }
    }
}
} // namespace RPGMod