using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace RPGMod.Questing
{
    public enum QuestType
    {
        killCommon,
        killElite,
        killChampion,
        collectGold,
        killFlying,
        killRed,
        killHaunted,
        killWhite,
        killPoison,
        killBlue,
        killLunar,
        killEarthDLC1,
        killVoidDLC1,
        killByBackstab
    }

    public static class Events
    {
        public class QuestEvent : UnityEvent<int, NetworkUser> { }

        public static readonly QuestEvent CommonKilled = new QuestEvent();
        public static readonly QuestEvent EliteKilled = new QuestEvent();
        public static readonly QuestEvent ChampionKilled = new QuestEvent();

        public static readonly QuestEvent FlyingKilled = new QuestEvent();

        public static readonly QuestEvent RedKilled = new QuestEvent();
        public static readonly QuestEvent HauntedKilled = new QuestEvent();
        public static readonly QuestEvent WhiteKilled = new QuestEvent();
        public static readonly QuestEvent PoisonKilled = new QuestEvent();
        public static readonly QuestEvent BlueKilled = new QuestEvent();
        public static readonly QuestEvent LunarKilled = new QuestEvent();

        public static readonly QuestEvent EarthKilledDLC1 = new QuestEvent();
        public static readonly QuestEvent VoidKilledDLC1 = new QuestEvent();

        public static readonly QuestEvent KilledByBackstab = new QuestEvent();

        public static readonly QuestEvent GoldCollected = new QuestEvent();
    }

    public class Announcement : MessageBase
    {
        public string Message { get; private set; }

        public Announcement()
        {
            Message = null;
        }

        public Announcement(string message)
        {
            Message = message;
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Message);
        }

        public override void Deserialize(NetworkReader reader)
        {
            Message = reader.ReadString();
        }

        public static void Handler(NetworkMessage networkMessage)
        {
            Client.Announcements.Add(networkMessage.ReadMessage<Announcement>());
        }
    }

    internal static class Client
    {
        private static QuestData _questData;
        private static UI.Quest _questUI;
        public static QuestData QuestData
        {
            get => _questData;
            set
            {
                if (_questData?.Guid != value?.Guid)
                {
                    _questUI?.Destroy();
                }

                _questData = value;

                if (_questData == null)
                {
                    return;
                }

                if (_questUI == null && !((bool)_questData?.Complete))
                {
                    var localUser = LocalUserManager.GetFirstLocalUser();

                    if (localUser?.cameraRigController?.hud?.mainContainer != null)
                    {
                        _questUI = localUser.cameraRigController.hud.mainContainer.AddComponent<UI.Quest>();
                    }
                }
                else if (_questUI != null)
                {
                    _questUI.UpdateData(_questData);
                }
            }
        }

        public static List<Announcement> Announcements { get; set; } = new List<Announcement>();

        private static UI.Announcer _announcerUI;

        public static void CleanUp()
        {
            _questUI?.Destroy();
            _announcerUI?.Destroy();
            Announcements.Clear();
            _questUI = null;
            _questData = null;
        }

        public static void Update()
        {
            if (Announcements.Count <= 0 || _announcerUI != null)
            {
                return;
            }

            var localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser?.cameraRigController?.hud?.mainContainer != null)
            {
                _announcerUI = localUser.cameraRigController.hud.mainContainer.AddComponent<UI.Announcer>();
                _announcerUI.SetMessage(Announcements[0].Message);
            }
            Announcements.RemoveAt(0);
        }
    }

    internal static class Server
    {
        public static List<QuestType> AllowedTypes { get; } = new List<QuestType>();
        public static List<ClientData> ClientDatas { get; set; } = new List<ClientData>();
        public static float timeoutStart;

        public static void CompletedQuest(NetworkUser networkUser)
        {
            foreach (var clientData in ClientDatas.Where(clientData => clientData.networkUser == networkUser))
            {
                clientData.questsCompleted += 1;
            }
        }

        public static void CheckAllowedType(QuestType questType)
        {
            if (AllowedTypes.Contains(questType))
            {
                return;
            }

            AllowedTypes.Add(questType);

            timeoutStart = Run.instance.GetRunStopwatch();
        }
    }

    internal static class Manager
    {
        public static void Update()
        {
            Server.ClientDatas.RemoveAll(BadClientData);

            // Create new quest if necessary
            foreach (var clientData in Server.ClientDatas
                         .Where(clientData => clientData.QuestData.Complete && Run.instance.GetRunStopwatch() - clientData.QuestData.CompletionTime > Config.Questing.cooldown))
            {
                clientData.NewQuest();
            }

            if (Server.AllowedTypes.Count <= 0 || !(Run.instance.GetRunStopwatch() - Server.timeoutStart > 4f))
            {
                return;
            }

            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (!(networkUser?.connectionToClient?.isReady ?? false))
                {
                    continue;
                }

                if (Server.ClientDatas.All(x => x.networkUser != networkUser))
                {
                    Server.ClientDatas.Add(new ClientData(networkUser));
                }
            }
        }

        public static void CheckClientData(NetworkUser networkUser)
        {
            foreach (var clientData in Server.ClientDatas.Where(clientData => clientData.networkUser == networkUser))
            {
                clientData.QuestData.Check();
            }
        }

        private static bool BadClientData(ClientData clientData)
        {
            return NetworkUser.readOnlyInstancesList.All(networkUser => networkUser != clientData.networkUser && networkUser.connectionToClient.isReady);
        }

        public static void CleanUp()
        {
            Server.ClientDatas.Clear();
            Server.AllowedTypes.Clear();
            Server.timeoutStart = 0f;
            Client.CleanUp();

            foreach (var keyValuePair in QuestComponent.QuestEventByQuestType)
            {
                keyValuePair.Value.RemoveAllListeners();
            }
        }
    }
}