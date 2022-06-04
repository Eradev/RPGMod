using System.Collections.Generic;
using System.Linq;
using RoR2;

namespace RPGMod.Questing
{
    internal static class Server
    {
        public static List<MissionType> AllowedTypes { get; } = new List<MissionType>();
        public static List<ClientData> ClientDatas { get; } = new List<ClientData>();
        public static float timeoutStart;

        public static void CompletedQuest(NetworkUser networkUser)
        {
            foreach (var clientData in ClientDatas.Where(clientData => clientData.networkUser == networkUser))
            {
                clientData.questsCompleted += 1;
            }
        }

        public static void CheckAllowedType(MissionType missionType)
        {
            if (AllowedTypes.Contains(missionType))
            {
                return;
            }

            AllowedTypes.Add(missionType);

            timeoutStart = Run.instance.GetRunStopwatch();
        }
    }
}