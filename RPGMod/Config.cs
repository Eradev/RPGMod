using BepInEx.Configuration;
using RPGMod.Questing;

namespace RPGMod
{
    public static class Config
    {
        public struct Questing
        {
            public static int Cooldown;
            public static float ChanceCommon;
            public static float ChanceUncommon;
            public static float ChanceLegendary;
            public static float ChanceAdjustmentPercent;

            public static string[] Blacklist;

            public static bool KillAnyEnabled;
            public static int KillAnyMin;
            public static int KillAnyMax;
            public static bool KillAnyBuffEnabled;
            public static int KillAnyBuffMin;
            public static int KillAnyBuffMax;

            public static bool KillCommonEnabled;
            public static int KillCommonMin;
            public static int KillCommonMax;
            public static bool KillEliteEnabled;
            public static int KillEliteMin;
            public static int KillEliteMax;
            public static bool KillChampionEnabled;
            public static int KillChampionMin;
            public static int KillChampionMax;

            public static bool KillFlyingEnabled;
            public static int KillFlyingMin;
            public static int KillFlyingMax;

            public static bool KillRedEnabled;
            public static int KillRedMin;
            public static int KillRedMax;
            public static bool KillHauntedEnabled;
            public static int KillHauntedMin;
            public static int KillHauntedMax;
            public static bool KillWhiteEnabled;
            public static int KillWhiteMin;
            public static int KillWhiteMax;
            public static bool KillPoisonEnabled;
            public static int KillPoisonMin;
            public static int KillPoisonMax;
            public static bool KillBlueEnabled;
            public static int KillBlueMin;
            public static int KillBlueMax;
            public static bool KillLunarEnabled;
            public static int KillLunarMin;
            public static int KillLunarMax;
            public static bool KillEarthEnabled;
            public static int KillEarthMin;
            public static int KillEarthMax;
            public static bool KillVoidEnabled;
            public static int KillVoidMin;
            public static int KillVoidMax;
            public static bool KillAurelioniteEnabled;
            public static int KillAurelioniteMin;
            public static int KillAurelioniteMax;
            public static bool KillBeadEnabled;
            public static int KillBeadMin;
            public static int KillBeadMax;

            public static int CollectGoldMin;
            public static int CollectGoldMax;
        }

        public struct Networking
        {
            public static short MsgType;
            public static short UpdateRate;
        }

        public struct UI
        {
            public static bool UseHUDScale;
            public static float OverrideHUDScale;
            public static float QuestPositionX;
            public static float QuestPositionY;
            public static float AnnouncerScaleX;
            public static float AnnouncerPositionY;

            public static bool SendNewQuestAnnouncement;
            public static bool SendQuestCompleteAnnouncement;
        }

        public static void Load(ConfigFile config, bool reload)
        {
            if (reload)
            {
                config.Reload();
            }

            // Questing
            Questing.Cooldown = config.Bind
            (
                new ConfigDefinition("Questing", "Quest cooldown"),
                45,
                new ConfigDescription("Time needed to wait after quest completion for a new quest to be generated")
            ).Value;

            Questing.ChanceCommon = config.Bind
            (
                new ConfigDefinition("Questing", "Common reward chance"),
                76f,
                new ConfigDescription("The percentage chance that the reward for a quest is of common rarity")
            ).Value / 100;
            Questing.ChanceUncommon = config.Bind
            (
                new ConfigDefinition("Questing", "Uncommon reward chance"),
                16f,
                new ConfigDescription("The percentage chance that the reward for a quest is of uncommon rarity")
            ).Value / 100;
            Questing.ChanceLegendary = config.Bind
            (
                new ConfigDefinition("Questing", "Legendary reward chance"),
                8f,
                new ConfigDescription("The percentage chance that the reward for a quest is of legendary rarity")
            ).Value / 100;

            Questing.ChanceAdjustmentPercent = config.Bind
            (
                new ConfigDefinition("Questing", "Chance Adjustment Percentage"),
                2f,
                new ConfigDescription("Multiplied by the amount of quests completed, the percentage to increase the chance of uncommon and legendary rewards and decrease the chance of common quests")
            ).Value / 100;

            Questing.Blacklist = config.Bind
            (
                new ConfigDefinition("Questing", "Blacklist"),
                string.Empty,
                new ConfigDescription("Items ID to blacklist. They will not show up as quest reward. Separated with a comma.")
            ).Value.Split(',');

            Questing.KillAnyEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Any kill enabled"),
                true,
                new ConfigDescription("Enable Kill any enemies mission")
            ).Value;
            Questing.KillAnyMin = config.Bind
            (
                new ConfigDefinition("Questing", "Any kill goal minimum"),
                10,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillAnyMax = config.Bind
            (
                new ConfigDefinition("Questing", "Any kill goal maximum"),
                25,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillAnyBuffEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Any buffed enemy kill enabled"),
                true,
                new ConfigDescription("Enable Kill any buffed enemies mission")
            ).Value;
            Questing.KillAnyBuffMin = config.Bind
            (
                new ConfigDefinition("Questing", "Any buffed enemy kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillAnyBuffMax = config.Bind
            (
                new ConfigDefinition("Questing", "Any buffed enemy kill goal maximum"),
                3,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillCommonEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Common kill enabled"),
                true,
                new ConfigDescription("Enable Kill common enemies mission")
            ).Value;
            Questing.KillCommonMin = config.Bind
            (
                new ConfigDefinition("Questing", "Common kill goal minimum"),
                5,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillCommonMax = config.Bind
            (
                new ConfigDefinition("Questing", "Common kill goal maximum"),
                9,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillEliteEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Elite kill enabled"),
                true,
                new ConfigDescription("Enable Kill elite enemies mission")
            ).Value;
            Questing.KillEliteMin = config.Bind
            (
                new ConfigDefinition("Questing", "Elite kill goal minimum"),
                3,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillEliteMax = config.Bind
            (
                new ConfigDefinition("Questing", "Elite kill goal maximum"),
                7,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillChampionEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Champion kill enabled"),
                true,
                new ConfigDescription("Enable Kill champion enemies mission")
            ).Value;
            Questing.KillChampionMin = config.Bind
            (
                new ConfigDefinition("Questing", "Champion kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillChampionMax = config.Bind
            (
                new ConfigDefinition("Questing", "Champion kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillFlyingEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Flying kill enabled"),
                true,
                new ConfigDescription("Enable Kill flying enemies mission")
            ).Value;
            Questing.KillFlyingMin = config.Bind
            (
                new ConfigDefinition("Questing", "Flying kill goal minimum"),
                2,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillFlyingMax = config.Bind
            (
                new ConfigDefinition("Questing", "Flying kill goal maximum"),
                5,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillRedEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Blazing kill enabled"),
                true,
                new ConfigDescription("Enable Kill Blazing enemies mission")
            ).Value;
            Questing.KillRedMin = config.Bind
            (
                new ConfigDefinition("Questing", "Blazing kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillRedMax = config.Bind
            (
                new ConfigDefinition("Questing", "Blazing kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillHauntedEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Celestine kill enabled"),
                true,
                new ConfigDescription("Enable Kill Celestine enemies mission")
            ).Value;
            Questing.KillHauntedMin = config.Bind
            (
                new ConfigDefinition("Questing", "Celestine kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillHauntedMax = config.Bind
            (
                new ConfigDefinition("Questing", "Celestine kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillWhiteEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Glacial kill enabled"),
                true,
                new ConfigDescription("Enable Kill Glacial enemies mission")
            ).Value;
            Questing.KillWhiteMin = config.Bind
            (
                new ConfigDefinition("Questing", "Glacial kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillWhiteMax = config.Bind
            (
                new ConfigDefinition("Questing", "Glacial kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillPoisonEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Malachite kill enabled"),
                true,
                new ConfigDescription("Enable Kill Malachite enemies mission")
            ).Value;
            Questing.KillPoisonMin = config.Bind
            (
                new ConfigDefinition("Questing", "Malachite kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillPoisonMax = config.Bind
            (
                new ConfigDefinition("Questing", "Malachite kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillBlueEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Overloading kill enabled"),
                true,
                new ConfigDescription("Enable Kill Overloading enemies mission")
            ).Value;
            Questing.KillBlueMin = config.Bind
            (
                new ConfigDefinition("Questing", "Overloading kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillBlueMax = config.Bind
            (
                new ConfigDefinition("Questing", "Overloading kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillLunarEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Perfected kill enabled"),
                true,
                new ConfigDescription("Enable Kill Perfected enemies mission")
            ).Value;
            Questing.KillLunarMin = config.Bind
            (
                new ConfigDefinition("Questing", "Perfected kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillLunarMax = config.Bind
            (
                new ConfigDefinition("Questing", "Perfected kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillEarthEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Mending kill enabled"),
                true,
                new ConfigDescription("Enable Kill Mending enemies mission")
            ).Value;
            Questing.KillEarthMin = config.Bind
            (
                new ConfigDefinition("Questing", "Mending kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillEarthMax = config.Bind
            (
                new ConfigDefinition("Questing", "Mending kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillVoidEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Void kill enabled"),
                true,
                new ConfigDescription("Enable Kill Void enemies mission")
            ).Value;
            Questing.KillVoidMin = config.Bind
            (
                new ConfigDefinition("Questing", "Void kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillVoidMax = config.Bind
            (
                new ConfigDefinition("Questing", "Void kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillAurelioniteEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Gilded kill enabled"),
                true,
                new ConfigDescription("Enable Kill Gilded enemies mission")
            ).Value;
            Questing.KillAurelioniteMin = config.Bind
            (
                new ConfigDefinition("Questing", "Gilded kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillAurelioniteMax = config.Bind
            (
                new ConfigDefinition("Questing", "Gilded kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.KillBeadEnabled = config.Bind
            (
                new ConfigDefinition("Questing", "Twisted kill enabled"),
                true,
                new ConfigDescription("Enable Kill Twisted enemies mission")
            ).Value;
            Questing.KillBeadMin = config.Bind
            (
                new ConfigDefinition("Questing", "Twisted kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.KillBeadMax = config.Bind
            (
                new ConfigDefinition("Questing", "Twisted kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.CollectGoldMin = config.Bind
            (
                new ConfigDefinition("Questing", "Collect gold goal minimum"),
                12,
                new ConfigDescription("The minimum amount for the range of the gold goal, before difficulty scaling (refer to this as the start of the game cost)")
            ).Value;
            Questing.CollectGoldMax = config.Bind
            (
                new ConfigDefinition("Questing", "Collect gold goal maximum"),
                31,
                new ConfigDescription("The maximum amount for the range of the gold goal, before difficulty scaling (refer to this as the start of the game cost)")
            ).Value;

            // Networking
            Networking.MsgType = config.Bind<short>
            (
                new ConfigDefinition("Networking", "Message type"),
                1337,
                new ConfigDescription("The starting number used for identification of messages sent between the client and server (both clients and server must be the same) (it is highly unlikely existing mods will conflict with the default value)")
            ).Value;
            Networking.UpdateRate = config.Bind<short>
            (
                new ConfigDefinition("Networking", "Update rate(ms)"),
                100,
                new ConfigDescription("How frequently clients are synced to the server")
            ).Value;

            // UI
            UI.UseHUDScale = config.Bind
            (
                new ConfigDefinition("UI", "Use game HUD scale"),
                true,
                new ConfigDescription("Determines whether or not the in-game HUD Scale setting is used")
            ).Value;
            UI.OverrideHUDScale = config.Bind
            (
                new ConfigDefinition("UI", "HUD scale override"),
                1f,
                new ConfigDescription("If useHUDScale is false, this value is used to determine scale instead")
            ).Value / 100;
            UI.QuestPositionX = config.Bind
            (
                new ConfigDefinition("UI", "Quest position on screen (x axis)"),
                87f,
                new ConfigDescription("Location of the quest on the screen (x axis), represented by screen width percentage")
            ).Value / 100;
            UI.QuestPositionY = config.Bind
            (
                new ConfigDefinition("UI", "Quest position on screen (y axis)"),
                70f,
                new ConfigDescription("Location of the quest on the screen (y axis), represented by screen height percentage")
            ).Value / 100;
            UI.AnnouncerScaleX = config.Bind
            (
                new ConfigDefinition("UI", "Width scale of announcer (x axis)"),
                0.3f,
                new ConfigDescription("Percentage of screen width (x axis) used for the announcer box")
            ).Value;
            UI.AnnouncerPositionY = config.Bind
            (
                new ConfigDefinition("UI", "Announcer position on the screen (y axis)"),
                0.03f,
                new ConfigDescription("Location of the announcer on the screen (y axis), represented by screen height percentage")
            ).Value;

            UI.SendNewQuestAnnouncement = config.Bind
            (
                new ConfigDefinition("UI", "Send new quest announcement"),
                true,
                new ConfigDescription("Send new quest announcement")
            ).Value;

            UI.SendQuestCompleteAnnouncement = config.Bind
            (
                new ConfigDefinition("UI", "Send quest complete announcement"),
                true,
                new ConfigDescription("Send quest complete announcement")
            ).Value;

            Blacklist.Recalculate();

            RpgMod.Log.LogDebug("Config loaded.");
        }
    }
}