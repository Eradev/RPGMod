using BepInEx.Configuration;

namespace RPGMod
{
public static class Config {
    public struct Questing {
        static public int cooldown;
        static public short port;
        static public float chanceCommon;
        static public float chanceUncommon;
        static public float chanceLegendary;
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
        Questing.port = config.Bind<short>
        (
            new ConfigDefinition("Questing", "port"),
            1337,
            new ConfigDescription("The port used for communication of questing data")
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
    }

}

} // namespace RPGMod