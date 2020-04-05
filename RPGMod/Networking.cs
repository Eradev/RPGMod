using UnityEngine.Networking;

namespace RPGMod {
    static class Networking {
        public static void Sync() {
            foreach(var playerData in Questing.Server.PlayerDatas) {
                NetworkServer.SendToClient(playerData.connectionId, 1337, playerData);
            }
        }
    }
}