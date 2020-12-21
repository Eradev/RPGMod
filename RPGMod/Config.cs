using BepInEx.Configuration;

namespace RPGMod
{
public static class Config {
    public struct Questing {
        public static int cooldown;
        public static float chanceCommon;
        public static float chanceUncommon;
        public static float chanceLegendary;
    }
    public struct Networking {
        public static short questPort;
        public static short updateRate;
    }
    public static void Load(ConfigFile config, bool reload)
    {
        if (reload)
        {
            config.Reload();
        }

        // Questing
        Questing.cooldown = config.Bind<int>
        (
            new ConfigDefinition("Questing", "cooldown"),
            45,
            new ConfigDescription("Time needed to wait after quest completion for a new quest to be generated")
        ).Value;
        Questing.chanceCommon = config.Bind<float>
        (
            new ConfigDefinition("Questing", "Common reward chance"),
            80f,
            new ConfigDescription("The percentage chance that the reward for a quest is of common rarity")
        ).Value / 100;
        Questing.chanceUncommon = config.Bind<float>
        (
            new ConfigDefinition("Questing", "Uncommon reward chance"),
            20f,
            new ConfigDescription("The percentage chance that the reward for a quest is of uncommon rarity")
        ).Value / 100;
        Questing.chanceLegendary = config.Bind<float>
        (
            new ConfigDefinition("Questing", "Legendary reward chance"),
            1f,
            new ConfigDescription("The percentage chance that the reward for a quest is of legendary rarity")
        ).Value / 100;
        // Networking
        Networking.questPort = config.Bind<short>
        (
            new ConfigDefinition("Networking", "Questing Port"),
            1337,
            new ConfigDescription("The port used for communication of questing data")
        ).Value;
        Networking.updateRate = config.Bind<short>
        (
            new ConfigDefinition("Networking", "Update Rate(ms)"),
            100,
            new ConfigDescription("How frequently clients are synced to the server")
        ).Value;
    }

}

} // namespace RPGMod