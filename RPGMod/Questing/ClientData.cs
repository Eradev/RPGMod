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
        this.networkUser = networkUser;
        NewQuest();
    }
    public void NewQuest() {
        QuestData = new QuestData(networkUser);
    }

}

} // namespace Questing
} // namespace RPGMod