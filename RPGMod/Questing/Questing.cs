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
    collectGold,
    attemptShrineChance
}
public static class Events {
    public class QuestEvent : UnityEvent<int, NetworkUser>{}

    public static QuestEvent commonKilled = new QuestEvent();
    public static QuestEvent eliteKilled = new QuestEvent();
    public static QuestEvent goldCollected = new QuestEvent();
    public static QuestEvent chanceShrineAttempted = new QuestEvent();
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
        Client.announcements.Add(networkMessage.ReadMessage<Questing.Announcement>());
    }
}

static class Client
{
    private static QuestData questData = null;
    private static UI.Quest questUI;
    public static QuestData QuestData
    {
        get {
            return questData;
        }
        set {
            if (questData?.guid != value?.guid) {
                questUI?.Destroy();
            }
            questData = value;
            if (questData != null) {
                if (questUI == null && !(questData?.complete ?? true)) {
                    LocalUser localUser = LocalUserManager.GetFirstLocalUser();

                    if (localUser?.cameraRigController?.hud?.mainContainer != null)
                    {
                        questUI = localUser.cameraRigController.hud.mainContainer.AddComponent<UI.Quest>();
                    }
                }
                else if(questUI != null) {
                    questUI.UpdateData(questData);
                }
            }
        }
    }
    public static List<Announcement> announcements { get; set; } = new List<Announcement>();
    private static UI.Announcer announcerUI;
    public static void CleanUp() {
        questUI?.Destroy();
        announcerUI?.Destroy();
        announcements.Clear();
        questUI = null;
        questData = null;
    }
    public static void Update() {
        if (announcements.Count > 0) {
            if (announcerUI == null) {
                LocalUser localUser = LocalUserManager.GetFirstLocalUser();

                if (localUser?.cameraRigController?.hud?.mainContainer != null)
                {
                    announcerUI = localUser.cameraRigController.hud.mainContainer.AddComponent<UI.Announcer>();
                    announcerUI.SetMessage(announcements[0].message);
                }
                announcements.RemoveAt(0);
            }
        }
    }
}
static class Server
{
    public static List<QuestType> AllowedTypes { get; private set; } = new List<QuestType>();
    public static List<ClientData> ClientDatas { get; set; } = new List<ClientData>();
    public static float timeoutStart = 0f;
    public static void CompletedQuest(NetworkUser networkUser) {
        for (int i = 0; i < ClientDatas.Count; i++) {
            if (ClientDatas[i].networkUser == networkUser) {
                ClientDatas[i].questsCompleted += 1;
            }
        }
    }
    public static void CheckAllowedType(QuestType questType) {
        if (!AllowedTypes.Contains(questType)) {
            AllowedTypes.Add(questType);
            // if (AllowedTypes.Count == 1) {
            //     foreach (NetworkUser networkUser in NetworkUser.readOnlyInstancesList) {
            //         Questing.Announcement announcement = new Questing.Announcement("");
            //         Networking.SendAnnouncement(announcement, networkUser.connectionToClient.connectionId);
            //     }
            // }
            timeoutStart = Run.instance.GetRunStopwatch();
        }
    }
}
static class Manager
{
    public static void Update()
    {
        Questing.Server.ClientDatas.RemoveAll(BadClientData);

        // Create new quest if necessary
        for (int i = 0; i < Server.ClientDatas.Count; i++)
        {
            if (Server.ClientDatas[i].QuestData.complete && (Run.instance.GetRunStopwatch() - Server.ClientDatas[i].QuestData.completionTime) > Config.Questing.cooldown)
            {
                Server.ClientDatas[i].NewQuest();
            }
        }

        if (Server.AllowedTypes.Count > 0 && (Run.instance.GetRunStopwatch() - Server.timeoutStart) > 4f) {
            foreach (NetworkUser networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (networkUser?.connectionToClient?.isReady ?? false)
                {
                    bool match = false;
                    foreach (ClientData clientData in Server.ClientDatas) {
                        if (clientData.networkUser == networkUser) {
                            match = true;
                        }
                    }

                    if (!match) {
                        Server.ClientDatas.Add(new ClientData(networkUser));
                    }
                }
            }
        }
    }
    public static void CheckClientData(NetworkUser networkUser)
    {
        for (int i = 0; i < Server.ClientDatas.Count; i++)
        {
            if (Server.ClientDatas[i].networkUser == networkUser)
            {
                Server.ClientDatas[i].QuestData.Check();
            }
        }
    }
    private static bool BadClientData(Questing.ClientData clientData)
    {
        bool bad = true;
        foreach(NetworkUser networkUser in NetworkUser.readOnlyInstancesList)
        {
            if (networkUser == clientData.networkUser && networkUser.connectionToClient.isReady)
            {
                bad = false;
            }
        }
        return bad;
    }
    public static void CleanUp() {
        Server.ClientDatas.Clear();
        Server.AllowedTypes.Clear();
        Server.timeoutStart = 0f;
        Client.CleanUp();
        Events.commonKilled.RemoveAllListeners();
        Events.eliteKilled.RemoveAllListeners();
        Events.goldCollected.RemoveAllListeners();
    }
}

} // namespace Questing
} // namespace RPGMod
