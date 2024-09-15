using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace RPGMod.SoftDependencies
{
    internal static class RiskOfOptionsHelper
    {
        private const string GUID = MyPluginInfo.PLUGIN_GUID;
        private const string ModName = MyPluginInfo.PLUGIN_NAME;

        public static void RegisterModInfo(string description, Sprite iconSprite = null)
        {
            if (!DependenciesManager.IsRiskOfOptionsPresent)
            {
                return;
            }

            ModSettingsManager.SetModDescription(description, GUID, ModName);
            if (iconSprite != null)
            {
                ModSettingsManager.SetModIcon(iconSprite, GUID, ModName);
            }
        }

        public static ConfigEntry<int> AddOptionSlider(this ConfigEntry<int> configEntry, bool restartRequired = false)
        {
            var min = int.MinValue;
            var max = int.MaxValue;

            if (configEntry.Description.AcceptableValues != null &&
                configEntry.Description.AcceptableValues.GetType() == typeof(AcceptableValueRange<int>))
            {
                var acceptableValueRange = configEntry.Description.AcceptableValues as AcceptableValueRange<int>;

                min = acceptableValueRange!.MinValue;
                max = acceptableValueRange.MaxValue;
            }

            return AddOptionSlider(configEntry, min, max, restartRequired);
        }

        public static ConfigEntry<int> AddOptionSlider(ConfigEntry<int> configEntry, int min, int max, bool restartRequired = false)
        {
            if (!DependenciesManager.IsRiskOfOptionsPresent)
            {
                return configEntry;
            }

            ModSettingsManager.AddOption(new IntSliderOption(configEntry, new IntSliderConfig
            {
                min = min,
                max = max,
                restartRequired = restartRequired
            }), GUID, ModName);

            return configEntry;
        }

        public static ConfigEntry<float> AddOptionSlider(this ConfigEntry<float> configEntry, bool restartRequired = false)
        {
            var min = float.MinValue;
            var max = float.MaxValue;

            if (configEntry.Description.AcceptableValues != null &&
                configEntry.Description.AcceptableValues.GetType() == typeof(AcceptableValueRange<float>))
            {
                var acceptableValueRange = configEntry.Description.AcceptableValues as AcceptableValueRange<float>;

                min = acceptableValueRange!.MinValue;
                max = acceptableValueRange.MaxValue;
            }

            return AddOptionSlider(configEntry, min, max, restartRequired);
        }

        public static ConfigEntry<float> AddOptionSlider(ConfigEntry<float> configEntry, float min, float max, bool restartRequired = false)
        {
            if (!DependenciesManager.IsRiskOfOptionsPresent)
            {
                return configEntry;
            }

            ModSettingsManager.AddOption(new StepSliderOption(configEntry, new StepSliderConfig
            {
                min = min,
                max = max,
                increment = 0.1f,
                restartRequired = restartRequired
            }), GUID, ModName);

            return configEntry;
        }

        public static ConfigEntry<bool> AddOptionBool(this ConfigEntry<bool> configEntry, bool restartRequired = false)
        {
            if (!DependenciesManager.IsRiskOfOptionsPresent)
            {
                return configEntry;
            }

            ModSettingsManager.AddOption(new CheckBoxOption(configEntry, restartRequired), GUID, ModName);

            return configEntry;
        }

        public static ConfigEntry<string> AddOptionString(this ConfigEntry<string> configEntry, bool restartRequired = false)
        {
            if (!DependenciesManager.IsRiskOfOptionsPresent)
            {
                return configEntry;
            }

            ModSettingsManager.AddOption(new StringInputFieldOption(configEntry, new InputFieldConfig
            {
                submitOn = InputFieldConfig.SubmitEnum.OnExitOrSubmit,
                restartRequired = restartRequired
            }), GUID, ModName);

            return configEntry;
        }
    }
}
