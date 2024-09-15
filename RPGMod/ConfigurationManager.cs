using BepInEx.Configuration;
using RPGMod.Questing;
using RPGMod.SoftDependencies;
using System;

namespace RPGMod
{
    internal static class ConfigurationManager
    {
        public struct Questing
        {
            public static ConfigEntry<int> Cooldown;
            public static ConfigEntry<float> RewardChanceCommon;
            public static ConfigEntry<float> RewardChanceUncommon;
            public static ConfigEntry<float> RewardChanceRare;
            public static ConfigEntry<float> RewardChanceAdjustmentPercent;
            public static ConfigEntry<int> TimerBase;
            public static ConfigEntry<int> TimerExtra;

            public static ConfigEntry<string> RewardBlacklist;
            public static ConfigEntry<string> EnemyTypeBlacklist;
            public static ConfigEntry<string> EliteBuffBlacklist;

            public static ConfigEntry<bool> MissionKillAnyEnabled;
            public static ConfigEntry<int> MissionKillAnyMin;
            public static ConfigEntry<int> MissionKillAnyMax;

            public static ConfigEntry<bool> MissionKillCommonEnabled;
            public static ConfigEntry<int> MissionKillCommonMin;
            public static ConfigEntry<int> MissionKillCommonMax;
            public static ConfigEntry<bool> MissionKillEliteEnabled;
            public static ConfigEntry<int> MissionKillEliteMin;
            public static ConfigEntry<int> MissionKillEliteMax;
            public static ConfigEntry<bool> MissionKillChampionEnabled;
            public static ConfigEntry<int> MissionKillChampionMin;
            public static ConfigEntry<int> MissionKillChampionMax;

            public static ConfigEntry<bool> MissionKillFlyingEnabled;
            public static ConfigEntry<int> MissionKillFlyingMin;
            public static ConfigEntry<int> MissionKillFlyingMax;

            public static ConfigEntry<bool> MissionKillSpecificTypeEnabled;
            public static ConfigEntry<int> MissionKillSpecificTypeMin;
            public static ConfigEntry<int> MissionKillSpecificTypeMax;

            public static ConfigEntry<bool> MissionKillSpecificBuffEnabled;
            public static ConfigEntry<int> MissionKillSpecificBuffMin;
            public static ConfigEntry<int> MissionKillSpecificBuffMax;

            public static ConfigEntry<bool> MissionCollectGoldEnabled;
            public static ConfigEntry<int> MissionCollectGoldMin;
            public static ConfigEntry<int> MissionCollectGoldMax;

            public static ConfigEntry<int> NumMissionsCommonMin;
            public static ConfigEntry<int> NumMissionsCommonMax;

            public static ConfigEntry<int> NumMissionsUncommonMin;
            public static ConfigEntry<int> NumMissionsUncommonMax;

            public static ConfigEntry<int> NumMissionsRareMin;
            public static ConfigEntry<int> NumMissionsRareMax;
        }

        public struct Networking
        {
            public static ConfigEntry<short> MsgType;
            public static ConfigEntry<short> UpdateRate;
        }

        public struct UI
        {
            public static ConfigEntry<bool> UseHUDScale;
            public static ConfigEntry<float> HUDScaleOverride;
            public static ConfigEntry<float> QuestPositionX;
            public static ConfigEntry<float> QuestPositionY;
            public static ConfigEntry<float> AnnouncerScaleX;
            public static ConfigEntry<float> AnnouncerPositionY;

            public static ConfigEntry<bool> SendNewQuestAnnouncement;
            public static ConfigEntry<bool> SendQuestCompleteAnnouncement;
            public static ConfigEntry<bool> SendQuestFailedAnnouncement;
        }

        public static void Init(ConfigFile configFile)
        {
            var commonKillMinDesc = new ConfigDescription(
                "The minimum amount for the range of the kill goal",
                new AcceptableValueRange<int>(1, 100)
            );
            var commonKillMaxDesc = new ConfigDescription(
                "The maximum amount for the range of the kill goal",
                new AcceptableValueRange<int>(1, 100)
            );

            RiskOfOptionsHelper.RegisterModInfo("Options for RPG Mod");

            Questing.Cooldown = configFile.Bind
            (
                new ConfigDefinition("Questing", "Cooldown"),
                45,
                new ConfigDescription(
                    "Cooldown period between each quest in seconds",
                    new AcceptableValueRange<int>(0, 600))
            ).AddOptionSlider();

            Questing.RewardChanceCommon = configFile.Bind
            (
                new ConfigDefinition("Questing", "RewardChanceCommon"),
                76f,
                new ConfigDescription(
                    "Percentage chance to roll for a common reward",
                    new AcceptableValueRange<float>(0f, 100f))
            ).AddOptionSlider();

            Questing.RewardChanceUncommon = configFile.Bind
            (
                new ConfigDefinition("Questing", "RewardChanceUncommon"),
                16f,
                new ConfigDescription(
                    "Percentage chance to roll for an uncommon reward",
                    new AcceptableValueRange<float>(0f, 100f))
            ).AddOptionSlider();

            Questing.RewardChanceRare = configFile.Bind
            (
                new ConfigDefinition("Questing", "RewardChanceRare"),
                8f,
                new ConfigDescription(
                    "Percentage chance to roll for a rare reward",
                    new AcceptableValueRange<float>(0f, 100f))
            ).AddOptionSlider();

            Questing.RewardChanceAdjustmentPercent = configFile.Bind
            (
                new ConfigDefinition("Questing", "RewardChanceAdjustmentPercentage"),
            2f,
            new ConfigDescription(
                    "Percentage multiplier to increase the chance of uncommon and rare rewards per successful quest",
                    new AcceptableValueRange<float>(0f, 100f))
            ).AddOptionSlider();

            Questing.RewardBlacklist = configFile.Bind
            (
                new ConfigDefinition("Questing", "RewardBlacklist"),
                string.Empty,
                new ConfigDescription("Items ID to blacklist as quest reward. Separated with a comma.")
            ).AddOptionString();
            Questing.RewardBlacklist.SettingChanged += (_, _) => Blacklist.ClearCache();

            Questing.TimerBase = configFile.Bind
            (
            new ConfigDefinition("Questing", "TimerBase"),
            180,
                new ConfigDescription(
                    "Number of seconds to complete a quest",
                    new AcceptableValueRange<int>(1, 600)
                )
            ).AddOptionSlider();

            Questing.TimerExtra = configFile.Bind
            (
                new ConfigDefinition("Questing", "TimerExtra"),
                30,
                new ConfigDescription(
                    "Extra seconds to complete the quest per extra mission",
                    new AcceptableValueRange<int>(0, 120)
                )
            ).AddOptionSlider();

            Questing.MissionKillAnyEnabled = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillAnyEnabled"),
                true,
                new ConfigDescription("Enable Kill any enemies mission")
            ).AddOptionBool();

            Questing.MissionKillAnyMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillAnyMin"),
                10,
                commonKillMinDesc
            ).AddOptionSlider();

            Questing.MissionKillAnyMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillAnyMax"),
                15,
                commonKillMaxDesc
            ).AddOptionSlider();

            Questing.MissionKillSpecificTypeEnabled = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillSpecificTypeEnabled"),
                true,
                new ConfigDescription("Enable Kill specific enemies mission")
            ).AddOptionBool();

            Questing.MissionKillSpecificTypeMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillSpecificTypeMin"),
                2,
                commonKillMinDesc
            ).AddOptionSlider();

            Questing.EnemyTypeBlacklist = configFile.Bind
            (
                new ConfigDefinition("Questing", "EnemyTypeBlacklist"),
                string.Empty,
                new ConfigDescription("Enemy types (CharacterBody.baseNameToken) to blacklist as quest specification. Separated with a comma.")
            ).AddOptionString();
            Questing.EnemyTypeBlacklist.SettingChanged += (_, _) =>
            {
                foreach (var bannedType in ConfigValues.Questing.EnemyTypeBlacklist)
                {
                    Server.AllowedMonsterTypes.Remove(bannedType);
                }

                if (Server.AllowedMonsterTypes.Count == 0)
                {
                    Server.AllowedMissionTypes.Remove(MissionType.KillSpecificName);
                }
            };

            Questing.MissionKillSpecificTypeMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillSpecificTypeMax"),
                5,
                commonKillMaxDesc
            ).AddOptionSlider();

            Questing.MissionKillCommonEnabled = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillCommonEnabled"),
                true,
                new ConfigDescription("Enable Kill common enemies mission")
            ).AddOptionBool();

            Questing.MissionKillCommonMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillCommonMin"),
                5,
                commonKillMinDesc
            ).AddOptionSlider();

            Questing.MissionKillCommonMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillCommonMax"),
                9,
                commonKillMaxDesc
            ).AddOptionSlider();

            Questing.MissionKillEliteEnabled = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillEliteEnabled"),
                true,
                new ConfigDescription("Enable Kill elite enemies mission")
            ).AddOptionBool();

            Questing.MissionKillEliteMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillEliteMin"),
                3,
                commonKillMinDesc
            ).AddOptionSlider();

            Questing.MissionKillEliteMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillEliteMax"),
                7,
                commonKillMaxDesc
            ).AddOptionSlider();

            Questing.MissionKillChampionEnabled = configFile.Bind
            (
                new ConfigDefinition("Questing", "KillChampionMissionEnabled"),
                true,
                new ConfigDescription("Enable Kill champion enemies mission")
            ).AddOptionBool();

            Questing.MissionKillChampionMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillChampionMin"),
                1,
                commonKillMinDesc
            ).AddOptionSlider();

            Questing.MissionKillChampionMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillChampionMax"),
                2,
                commonKillMaxDesc
            ).AddOptionSlider();

            Questing.MissionKillFlyingEnabled = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillFlyingEnabled"),
                true,
                new ConfigDescription("Enable Kill flying enemies mission")
            ).AddOptionBool();

            Questing.MissionKillFlyingMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillFlyingMin"),
                2,
                commonKillMinDesc
            ).AddOptionSlider();

            Questing.MissionKillFlyingMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillFlyingMax"),
                5,
                commonKillMaxDesc
            ).AddOptionSlider();

            Questing.MissionKillSpecificBuffEnabled = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillSpecificBuffEnabled"),
                true,
                new ConfigDescription("Enable Kill enemies with specific buff mission")
            ).AddOptionBool();

            Questing.MissionKillSpecificBuffMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillSpecificBuffMin"),
                1,
                commonKillMinDesc
            ).AddOptionSlider();

            Questing.MissionKillSpecificBuffMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionKillSpecificBuffMax"),
                2,
                commonKillMaxDesc
            ).AddOptionSlider();

            Questing.EliteBuffBlacklist = configFile.Bind
            (
                new ConfigDefinition("Questing", "EliteBuffBlacklist"),
                string.Empty,
                new ConfigDescription("Elite buff name (BuffDef.name) to blacklist as mission specification. Separated with a comma.")
            ).AddOptionString();
            Questing.EnemyTypeBlacklist.SettingChanged += (_, _) =>
            {
                foreach (var bannedType in ConfigValues.Questing.EliteBuffBlacklist)
                {
                    Server.AllowedBuffTypes.Remove(bannedType);
                }

                if (Server.AllowedBuffTypes.Count == 0)
                {
                    Server.AllowedMissionTypes.Remove(MissionType.KillSpecificBuff);
                }
            };

            Questing.MissionCollectGoldEnabled = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionCollectGoldEnabled"),
                true,
                new ConfigDescription("Enable Collect gold mission")
            ).AddOptionBool();

            Questing.MissionCollectGoldMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionCollectGoldMin"),
                12,
                new ConfigDescription(
                    "The minimum amount for the collect gold mission",
                    new AcceptableValueRange<int>(1, 100)
                )
            ).AddOptionSlider();

            Questing.MissionCollectGoldMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "MissionCollectGoldMax"),
                31,
                new ConfigDescription(
                    "The maximum amount for the collect gold mission",
                    new AcceptableValueRange<int>(1, 100)
                )
            ).AddOptionSlider();


            Questing.NumMissionsCommonMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "NumMissionsCommonMin"),
                1,
                new ConfigDescription(
                    "The minimum amount of missions to generate for Tier 1 items",
                    new AcceptableValueRange<int>(1, 10)
                )
            ).AddOptionSlider();

            Questing.NumMissionsCommonMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "NumMissionsCommonMax"),
                1,
                new ConfigDescription(
                    "The maximum amount of missions to generate for common items",
                    new AcceptableValueRange<int>(1, 10)
                )
            ).AddOptionSlider();

            Questing.NumMissionsUncommonMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "NumMissionsUncommonMin"),
                2,
                new ConfigDescription(
                    "The minimum amount of missions to generate for uncommon items",
                    new AcceptableValueRange<int>(1, 10)
                )
            ).AddOptionSlider();

            Questing.NumMissionsUncommonMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "NumMissionsUncommonMax"),
                2,
                new ConfigDescription(
                    "The maximum amount of missions to generate for uncommon items",
                    new AcceptableValueRange<int>(1, 10)
                )
            ).AddOptionSlider();

            Questing.NumMissionsRareMin = configFile.Bind
            (
                new ConfigDefinition("Questing", "NumMissionsRareMin"),
                3,
                new ConfigDescription(
                    "The minimum amount of missions to generate for rare items",
                    new AcceptableValueRange<int>(1, 10)
                )
            ).AddOptionSlider();

            Questing.NumMissionsRareMax = configFile.Bind
            (
                new ConfigDefinition("Questing", "NumMissionsRareMax"),
                4,
                new ConfigDescription(
                    "The maximum amount of missions to generate for rare items",
                    new AcceptableValueRange<int>(1, 10)
                )
            ).AddOptionSlider();

            // Networking
            Networking.MsgType = configFile.Bind<short>
            (
                new ConfigDefinition("Networking", "MessageType"),
                1337,
                new ConfigDescription("The starting number used for identification of messages sent between the client and server (both clients and server must be the same)")
            );

            Networking.UpdateRate = configFile.Bind<short>
            (
                new ConfigDefinition("Networking", "UpdateRate"),
                100,
                new ConfigDescription("How frequently clients are synced to the server (in ms)")
            );

            // UI
            UI.UseHUDScale = configFile.Bind
            (
                new ConfigDefinition("UI", "UseHUDScale"),
                true,
                new ConfigDescription("Determines whether or not the in-game HUD Scale setting is used")
            ).AddOptionBool();
            UI.UseHUDScale.SettingChanged += RefreshUI;

            UI.HUDScaleOverride = configFile.Bind
            (
                new ConfigDefinition("UI", "HUDScaleOverride"),
                1f,
                new ConfigDescription(
                    "If useHUDScale is false, this value is used to determine scale instead",
                    new AcceptableValueRange<float>(0.1f, 10f)
                )
            ).AddOptionSlider();
            UI.HUDScaleOverride.SettingChanged += RefreshUI;

            UI.QuestPositionX = configFile.Bind
            (
                new ConfigDefinition("UI", "QuestPositionX"),
                87f,
                new ConfigDescription(
                    "Location of the quest on the screen (x axis), represented by screen width percentage",
                    new AcceptableValueRange<float>(0f, 100f)
                )
            ).AddOptionSlider();
            UI.QuestPositionX.SettingChanged += RefreshUI;

            UI.QuestPositionY = configFile.Bind
            (
                new ConfigDefinition("UI", "QuestPositionY"),
                70f,
                new ConfigDescription(
                    "Location of the quest on the screen (y axis), represented by screen height percentage",
                    new AcceptableValueRange<float>(0f, 100f)
                )
            ).AddOptionSlider();
            UI.QuestPositionY.SettingChanged += RefreshUI;

            UI.AnnouncerScaleX = configFile.Bind
            (
                new ConfigDefinition("UI", "AnnouncerScaleX"),
                30f,
                new ConfigDescription(
                    "Percentage of screen width (x axis) used for the announcer box",
                    new AcceptableValueRange<float>(0f, 100f)
                )
            ).AddOptionSlider();
            UI.AnnouncerScaleX.SettingChanged += RefreshUI;

            UI.AnnouncerPositionY = configFile.Bind
            (
                new ConfigDefinition("UI", "AnnouncerPositionY"),
                3f,
                new ConfigDescription(
                    "Location of the announcer on the screen (y axis), represented by screen height percentage",
                    new AcceptableValueRange<float>(0f, 100f)
                )
            ).AddOptionSlider();
            UI.AnnouncerPositionY.SettingChanged += RefreshUI;

            UI.SendNewQuestAnnouncement = configFile.Bind
            (
                new ConfigDefinition("UI", "SendNewQuestAnnouncement"),
                true,
                new ConfigDescription("Send new quest announcement")
            ).AddOptionBool();

            UI.SendQuestCompleteAnnouncement = configFile.Bind
            (
                new ConfigDefinition("UI", "SendQuestCompleteAnnouncement"),
                true,
                new ConfigDescription("Send quest complete announcement")
            ).AddOptionBool();

            UI.SendQuestFailedAnnouncement = configFile.Bind
            (
                new ConfigDefinition("UI", "SendQuestFailedAnnouncement"),
                true,
                new ConfigDescription("Send quest failed announcement")
            ).AddOptionBool();
        }

        private static void RefreshUI(object sender, EventArgs e)
        {
            if (RpgMod.Instance == null)
            {
                return;
            }

            if (RpgMod.Instance.ModState == ModState.Started)
            {
                RpgMod.Instance.RefreshUI();
            }
        }
    }
}
