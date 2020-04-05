using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;

namespace RPGMod
{
namespace Questing
{
    class PlayerData : MessageBase
    {
        public bool complete;
        public float completionTime;
        public int connectionId;

        public PlayerData(int connectionId)
        {
            complete = false;
            completionTime = 0;
            this.connectionId = connectionId;
        }

        public override void Deserialize(NetworkReader reader)
        {
            complete = reader.ReadBoolean();
            completionTime = reader.ReadSingle();
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(complete);
            writer.Write(completionTime);
        }

    }
    static class Client
    {
        static PlayerData PlayerData { get; set; }
    }
    static class Server
    {
        public static List<PlayerData> PlayerDatas { get; set; }
    }
    static class Handler
    {
        public static void Update()
        {
            for (int i = 0; i<NetworkServer.connections.Count; i++) {
                if (NetworkServer.connections[i].isReady) {
                    int dataIndex = -1;
                    for (int j = 0; j < Server.PlayerDatas.Count; j++)
                    {
                        if (Server.PlayerDatas[j].connectionId == NetworkServer.connections[i].connectionId) {
                            dataIndex = j;
                            break;
                        }
                    }
                    if (dataIndex != -1) {
                        if (Server.PlayerDatas[dataIndex].complete && (Run.instance.GetRunStopwatch() - Server.PlayerDatas[dataIndex].completionTime) > ConfigHandler.config.questWaitTime)
                        {
                            Server.PlayerDatas[dataIndex] = new PlayerData(NetworkServer.connections[i].connectionId);
                        }
                    }
                    else {
                        Server.PlayerDatas.Add(new PlayerData(NetworkServer.connections[i].connectionId));
                    }
                }
            }
        }
    }
} // namespace Questing
} // namespace RPGMod
