using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace RPGMod.Questing
{
    internal static class Server
    {
        public static List<MissionType> AllowedTypes { get; } = new List<MissionType>();
        public static List<ClientData> ClientDatas { get; } = new List<ClientData>();
        public static float TimeoutStart;

        public static void CompletedQuest(NetworkUser networkUser)
        {
            ClientDatas.First(clientData => clientData.NetworkUser == networkUser).QuestsCompleted += 1;
        }

        public static void FailQuest(NetworkUser networkUser)
        {
            ClientDatas.First(clientData => clientData.NetworkUser == networkUser).QuestsCompleted -= 1;
        }

        public static void UnlockMissionType(MissionType missionType)
        {
            if (AllowedTypes.Contains(missionType))
            {
                return;
            }

            AllowedTypes.Add(missionType);
            RpgMod.Log.LogDebug($"Unlocked mission type {missionType}");

            TimeoutStart = Run.instance.GetRunStopwatch();
        }
    }
}