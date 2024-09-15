using RoR2;
using RPGMod.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace RPGMod.Questing
{
    internal static class Server
    {
        public static List<MissionType> AllowedMissionTypes { get; } = [];
        public static List<string> AllowedMonsterTypes { get; } = [];
        public static List<string> AllowedBuffTypes { get; } = [];
        public static List<ClientData> ClientDatas { get; } = [];
        public static float TimeoutStart;

        public static int MaxAvailableUniqueMissions =>
            AllowedMissionTypes.Count +
            (ConfigValues.Questing.MissionKillSpecificBuffEnabled && AllowedMissionTypes.Contains(MissionType.KillSpecificBuff)
                ? AllowedBuffTypes.Count - 1
                : 0) +
            (ConfigValues.Questing.KillSpecificTypeEnabled && AllowedMissionTypes.Contains(MissionType.KillSpecificName)
                ? AllowedMonsterTypes.Count - 1
                : 0);

        public static int MinUniqueMissionsRequired =>
            new[]
            {
                ConfigValues.Questing.NumMissionsCommonMin,
                ConfigValues.Questing.NumMissionsUncommonMin,
                ConfigValues.Questing.NumMissionsRareMin
            }.Min();

        public static ItemTier MaxAllowedRewardTier
        {
            get
            {
                var maxUniqueMissions = MaxAvailableUniqueMissions;

                if (maxUniqueMissions >= ConfigValues.Questing.NumMissionsRareMin)
                {
                    return ItemTier.Tier3;
                }

                return maxUniqueMissions >= ConfigValues.Questing.NumMissionsUncommonMin
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
            if (AllowedMissionTypes.Contains(missionType))
            {
                return;
            }

            AllowedMissionTypes.Add(missionType);
            RpgMod.Log.LogDebug($"Unlocked mission type {missionType}");

            TimeoutStart = Run.instance.GetRunStopwatch();
        }

        public static void UnlockMonsterType(CharacterBody enemy)
        {
            if (AllowedMonsterTypes.Contains(enemy.baseNameToken) ||
                ConfigValues.Questing.EnemyTypeBlacklist.Contains(enemy.baseNameToken))
            {
                return;
            }

            AllowedMonsterTypes.Add(enemy.baseNameToken);
            RpgMod.Log.LogDebug($"Unlocked enemy type {Language.GetString(enemy.baseNameToken)} ({enemy.baseNameToken})");
        }

        public static void UnlockBuffType(BuffDef buffDef)
        {
            if (AllowedBuffTypes.Contains(buffDef.eliteDef.modifierToken) ||
                ConfigValues.Questing.EliteBuffBlacklist.Contains(buffDef.name))
            {
                return;
            }

            AllowedBuffTypes.Add(buffDef.eliteDef.modifierToken);
            RpgMod.Log.LogDebug($"Unlocked buff type {buffDef.GetDisplayName().RemoveReplacementTokens()} ({buffDef.name})");
        }
    }
}