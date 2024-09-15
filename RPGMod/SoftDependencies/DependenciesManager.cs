using BepInEx.Bootstrap;

namespace RPGMod.SoftDependencies
{
    internal static class DependenciesManager
    {
        // ReSharper disable once StringLiteralTypo
        public const string RiskOfOptionsGuid = "com.rune580.riskofoptions";

        public static bool IsRiskOfOptionsPresent;

        public static void CheckForDependencies()
        {
            IsRiskOfOptionsPresent = Chainloader.PluginInfos.ContainsKey(RiskOfOptionsGuid);

        }
    }
}
