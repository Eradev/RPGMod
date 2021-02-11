using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Events;

namespace RPGMod
{
namespace Questing
{
public enum QuestType {
    killCommon,
    killElite,
    collectGold
}
public static class Events {
    public class QuestEvent : UnityEvent<int, NetworkInstanceId>{}

    public static QuestEvent commonKilled = new QuestEvent();
    public static QuestEvent eliteKilled = new QuestEvent();
    public static QuestEvent goldCollected = new QuestEvent();
}

public class Announcement : MessageBase
{
    public String message { get; private set; }
    public Announcement() {
        this.message = null;
    }
    public Announcement(String message) {
        this.message = message;
    }
    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(message);
    }
    public override void Deserialize(NetworkReader reader)
    {
        message = reader.ReadString();
    }
    public static void Handler(NetworkMessage networkMessage) {
        Client.announcement = networkMessage.ReadMessage<Questing.Announcement>();
        Client.announced = true;
    }
}

static class Client
{
    private static ClientData clientData = null;
    private static QuestUI questUI;
    public static ClientData ClientData
    {
        get {
            return clientData;
        }
        set {
            clientData = value;

            if (questUI is null && !(clientData?.complete ?? true)) {
                LocalUser localUser = LocalUserManager.GetFirstLocalUser();

                if (localUser?.cachedBody != null)
                {
                    questUI = localUser.cachedBody.gameObject.AddComponent<QuestUI>();
                }
            }
            if (questUI != null) {
                questUI.UpdateData(clientData);
            }
        }
    }
    public static Announcement announcement;
    public static bool announced = false;
    public static void CleanUp() {
        UnityEngine.Object.Destroy(questUI);
        announced = false;
        questUI = null;
        clientData = null;
    }
    public static void Update() {
        if (announcement != null && announced) {
            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser?.cachedBody != null)
            {
                localUser.cachedBody.gameObject.AddComponent<AnnouncerUI>().SetMessage(announcement.message);
                announced = false;
            }
        }
    }
}
static class Server
{
    public static List<ClientData> ClientDatas { get; set; } = new List<ClientData>();
}
static class Manager
{
    public static void Update()
    {
        Questing.Server.ClientDatas.RemoveAll(BadClientData);
        for (int i = 0; i<NetworkServer.connections.Count; i++) {
            if (NetworkServer.connections[i]?.connectionId != null && NetworkServer.connections[i].isReady) {
                int dataIndex = -1;
                // Find existing clientData index
                for (int j = 0; j < Server.ClientDatas.Count; j++)
                {
                    if (Server.ClientDatas[j].connectionId == NetworkServer.connections[i].connectionId) {
                        dataIndex = j;
                        break;
                    }
                }
                // If clientData index was found, update if the quest is completed
                if (dataIndex != -1) {
                    if (Server.ClientDatas[dataIndex].complete && (Run.instance.GetRunStopwatch() - Server.ClientDatas[dataIndex].completionTime) > Config.Questing.cooldown)
                    {
                        Server.ClientDatas[dataIndex] = new ClientData(NetworkServer.connections[i].connectionId);
                    }
                }
                // Otherwise no clientData exists for the player and a new clientData is added
                else {
                    Server.ClientDatas.Add(new ClientData(NetworkServer.connections[i].connectionId));
                }
            }
        }
    }
    public static void CheckClientData(NetworkInstanceId netId)
    {
        for (int i = 0; i < Server.ClientDatas.Count; i++)
        {
            if (Server.ClientDatas[i].netId == netId)
            {
                Server.ClientDatas[i].Check();
            }
        }
    }
    private static bool BadClientData(Questing.ClientData clientData)
    {
        bool bad = true;
        for (int i = 0; i < NetworkServer.connections.Count; i++)
        {
            if (NetworkServer.connections[i]?.connectionId != null && NetworkServer.connections[i].connectionId == clientData.connectionId)
            {
                bad = false;
            }
        }
        return bad;
    }
    public static void CleanUp() {
        Server.ClientDatas.Clear();
        Client.CleanUp();
        UI.Setup();
        Events.commonKilled.RemoveAllListeners();
        Events.eliteKilled.RemoveAllListeners();
        Events.goldCollected.RemoveAllListeners();
    }
}
} // namespace Questing
} // namespace RPGMod
