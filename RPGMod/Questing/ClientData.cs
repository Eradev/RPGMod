using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace RPGMod
{
namespace Questing
{
public class ClientData {
    public QuestData QuestData {get; private set;}
    public NetworkUser networkUser;
    public int questsCompleted;
    public ClientData(NetworkUser networkUser) {
        questsCompleted = 0;
        this.networkUser = networkUser;
        NewQuest();
    }
    public void NewQuest() {
        if (Server.AllowedTypes.Count <= 0) {
            return;
        }
        QuestData = new QuestData(networkUser, questsCompleted, QuestData?.guid ?? 0);
    }

}

} // namespace Questing
} // namespace RPGMod