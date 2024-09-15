namespace RPGMod
{
    public static class ConfigValues
    {
        public struct Questing
        {
            public static int Cooldown => ConfigurationManager.Questing.Cooldown.Value;
            public static float RewardChanceCommon => ConfigurationManager.Questing.RewardChanceCommon.Value / 100;
            public static float RewardChanceUncommon => ConfigurationManager.Questing.RewardChanceUncommon.Value / 100;
            public static float RewardChanceRare => ConfigurationManager.Questing.RewardChanceRare.Value / 100;
            public static float RewardChanceAdjustmentPercent => ConfigurationManager.Questing.RewardChanceAdjustmentPercent.Value / 100;
            public static float TimerBase => ConfigurationManager.Questing.TimerBase.Value;
            public static float TimerExtra => ConfigurationManager.Questing.TimerExtra.Value;

            public static string[] RewardBlacklist => ConfigurationManager.Questing.RewardBlacklist.Value.Split(',');

            public static bool MissionKillAnyEnabled => ConfigurationManager.Questing.MissionKillAnyEnabled.Value;
            public static int MissionKillAnyMin => ConfigurationManager.Questing.MissionKillAnyMin.Value;
            public static int MissionKillAnyMax => ConfigurationManager.Questing.MissionKillAnyMax.Value;

            public static bool KillSpecificTypeEnabled => ConfigurationManager.Questing.MissionKillSpecificTypeEnabled.Value;
            public static int KillSpecificNameMin => ConfigurationManager.Questing.MissionKillSpecificTypeMin.Value;
            public static int KillSpecificNameMax => ConfigurationManager.Questing.MissionKillSpecificTypeMax.Value;

            public static string[] EnemyTypeBlacklist => ConfigurationManager.Questing.EnemyTypeBlacklist.Value.Split(',');

            public static bool MissionKillCommonEnabled => ConfigurationManager.Questing.MissionKillCommonEnabled.Value;
            public static int MissionKillCommonMin => ConfigurationManager.Questing.MissionKillCommonMin.Value;
            public static int MissionKillCommonMax => ConfigurationManager.Questing.MissionKillCommonMax.Value;

            public static bool MissionKillEliteEnabled => ConfigurationManager.Questing.MissionKillEliteEnabled.Value;
            public static int MissionKillEliteMin => ConfigurationManager.Questing.MissionKillEliteMin.Value;
            public static int MissionKillEliteMax => ConfigurationManager.Questing.MissionKillEliteMax.Value;

            public static bool MissionKillChampionEnabled => ConfigurationManager.Questing.MissionKillChampionEnabled.Value;
            public static int MissionKillChampionMin => ConfigurationManager.Questing.MissionKillChampionMin.Value;
            public static int MissionKillChampionMax => ConfigurationManager.Questing.MissionKillChampionMax.Value;

            public static bool MissionKillFlyingEnabled => ConfigurationManager.Questing.MissionKillFlyingEnabled.Value;
            public static int MissionKillFlyingMin => ConfigurationManager.Questing.MissionKillFlyingMin.Value;
            public static int MissionKillFlyingMax => ConfigurationManager.Questing.MissionKillFlyingMax.Value;

            public static bool MissionKillSpecificBuffEnabled => ConfigurationManager.Questing.MissionKillSpecificBuffEnabled.Value;
            public static int MissionKillSpecificBuffMin => ConfigurationManager.Questing.MissionKillSpecificBuffMin.Value;
            public static int MissionKillSpecificBuffMax => ConfigurationManager.Questing.MissionKillSpecificBuffMax.Value;

            public static string[] EliteBuffBlacklist => ConfigurationManager.Questing.EliteBuffBlacklist.Value.Split(',');

            public static bool MissionCollectGoldEnabled => ConfigurationManager.Questing.MissionCollectGoldEnabled.Value;
            public static int MissionCollectGoldMin => ConfigurationManager.Questing.MissionCollectGoldMin.Value;
            public static int MissionCollectGoldMax => ConfigurationManager.Questing.MissionCollectGoldMax.Value;

            public static int NumMissionsCommonMin => ConfigurationManager.Questing.NumMissionsCommonMin.Value;
            public static int NumMissionsCommonMax => ConfigurationManager.Questing.NumMissionsCommonMax.Value;

            public static int NumMissionsUncommonMin => ConfigurationManager.Questing.NumMissionsUncommonMin.Value;
            public static int NumMissionsUncommonMax => ConfigurationManager.Questing.NumMissionsUncommonMax.Value;

            public static int NumMissionsRareMin => ConfigurationManager.Questing.NumMissionsRareMin.Value;
            public static int NumMissionsRareMax => ConfigurationManager.Questing.NumMissionsRareMax.Value;
        }

        public struct Networking
        {
            public static short MsgType => ConfigurationManager.Networking.MsgType.Value;
            public static short UpdateRate => ConfigurationManager.Networking.UpdateRate.Value;
        }

        public struct UI
        {
            public static bool UseHUDScale => ConfigurationManager.UI.UseHUDScale.Value;
            public static float HUDScaleOverride => ConfigurationManager.UI.HUDScaleOverride.Value / 100;
            public static float QuestPositionX => ConfigurationManager.UI.QuestPositionX.Value / 100;
            public static float QuestPositionY => ConfigurationManager.UI.QuestPositionY.Value / 100;
            public static float AnnouncerScaleX => ConfigurationManager.UI.AnnouncerScaleX.Value / 100;
            public static float AnnouncerPositionY => ConfigurationManager.UI.AnnouncerPositionY.Value / 100;

            public static bool SendNewQuestAnnouncement => ConfigurationManager.UI.SendNewQuestAnnouncement.Value;
            public static bool SendQuestCompleteAnnouncement => ConfigurationManager.UI.SendQuestCompleteAnnouncement.Value;
            public static bool SendQuestFailedAnnouncement => ConfigurationManager.UI.SendQuestFailedAnnouncement.Value;
        }
    }
}