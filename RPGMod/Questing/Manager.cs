using System.Linq;
using RoR2;

namespace RPGMod.Questing
{
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

            foreach (var keyValuePair in Mission.EventsByMissionType)
            {
                keyValuePair.Value.RemoveAllListeners();
            }
        }
    }
}