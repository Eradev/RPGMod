using RoR2;
using System.Linq;

namespace RPGMod.Questing
{
    internal static class Manager
    {
        public static void Update()
        {
            Server.ClientDatas.RemoveAll(BadClientData);

            foreach (var clientData in Server.ClientDatas)
            {
                switch (clientData.QuestData.Complete)
                {
                    // Create new quest if necessary
                    case true when Run.instance.GetRunStopwatch() - clientData.QuestData.CompletionTime > Config.Questing.Cooldown:
                        clientData.NewQuest();

                        break;

                    // Fail quests that exceeded the time limit
                    case false when Run.instance.GetRunStopwatch() > clientData.QuestData.LimitTime:
                        CheckClientData(clientData.NetworkUser);

                        break;
                }
            }

            if (Server.AllowedTypes.Count <= 0 || !(Run.instance.GetRunStopwatch() - Server.TimeoutStart > 4f))
            {
                return;
            }

            foreach (var networkUser in NetworkUser.readOnlyInstancesList)
            {
                if (!(networkUser?.connectionToClient?.isReady ?? false))
                {
                    continue;
                }

                if (Server.ClientDatas.All(x => x.NetworkUser != networkUser))
                {
                    Server.ClientDatas.Add(new ClientData(networkUser));
                }
            }
        }

        public static void CheckClientData(NetworkUser networkUser)
        {
            Server.ClientDatas.First(clientData => clientData.NetworkUser == networkUser).QuestData.Check();
        }

        private static bool BadClientData(ClientData clientData)
        {
            return NetworkUser.readOnlyInstancesList.All(networkUser => networkUser != clientData.NetworkUser && networkUser.connectionToClient.isReady);
        }

        public static void CleanUp()
        {
            Server.ClientDatas.Clear();
            Server.AllowedTypes.Clear();
            Server.AllowedMonsterTypes.Clear();
            Server.AllowedBuffTypes.Clear();
            Server.TimeoutStart = 0f;
            Client.CleanUp();

            foreach (var keyValuePair in Mission.EventsByMissionType)
            {
                keyValuePair.Value.RemoveAllListeners();
            }
        }
    }
}