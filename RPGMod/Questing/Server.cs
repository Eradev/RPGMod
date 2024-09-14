using RoR2;
using RPGMod.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace RPGMod.Questing
{
    internal static class Server
    {
        public static List<MissionType> AllowedTypes { get; } = new List<MissionType>();
        public static List<string> AllowedMonsterTypes { get; } = new List<string>();
        public static List<string> AllowedBuffTypes { get; } = new List<string>();
        public static List<ClientData> ClientDatas { get; } = new List<ClientData>();
        public static float TimeoutStart;

        public static int MaxAvailableUniqueMissions =>
            AllowedTypes.Count +
            (Config.Questing.KillSpecificBuffEnabled ? AllowedBuffTypes.Count : 0) +
            (Config.Questing.KillSpecificNameEnabled ? AllowedMonsterTypes.Count : 0);

        public static int MinUniqueMissionsRequired =>
            new[]
            {
                Config.Questing.MinNumMissionsTier1,
                Config.Questing.MinNumMissionsTier2,
                Config.Questing.MinNumMissionsTier3
            }.Min();

        public static ItemTier MaxAllowedRewardTier
        {
            get
            {
                var maxUniqueMissions = MaxAvailableUniqueMissions;

                if (maxUniqueMissions >= Config.Questing.MinNumMissionsTier3)
                {
                    return ItemTier.Tier3;
                }

                return maxUniqueMissions >= Config.Questing.MinNumMissionsTier2
                    ? ItemTier.Tier2
                    : ItemTier.Tier1;
            }
        }

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

        public static void UnlockMonsterType(CharacterBody enemy)
        {
            if (AllowedMonsterTypes.Contains(enemy.baseNameToken))
            {
                return;
            }

            AllowedMonsterTypes.Add(enemy.baseNameToken);
            RpgMod.Log.LogDebug($"Unlocked enemy type {Language.GetString(enemy.baseNameToken)}");
        }

        public static void UnlockBuffType(BuffDef buffDef)
        {

            if (AllowedBuffTypes.Contains(buffDef.eliteDef.modifierToken))
            {
                return;
            }

            AllowedBuffTypes.Add(buffDef.eliteDef.modifierToken);
            RpgMod.Log.LogDebug($"Unlocked buff type {buffDef.GetDisplayName().RemoveReplacementTokens()}");
        }
    }
}