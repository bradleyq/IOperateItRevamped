using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;

namespace DriveIt.Settings.Tabs
{
    internal class ConfigCar
    {
        public static void Populate(UIComponent panel)
        {
            float currentY = 0.0f;
            float headerWidth = panel.width;

            #region Car Vehicle Group
            var groupGeneral = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_CAR));
            currentY += groupGeneral.height + SettingsPanel.LargeMargin;

            var enginePower_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_ENGINEPOWER), 10f, 1000f, 1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 10, numberFormat: "N0", suffix: " KW"));
            enginePower_Slider.eventValueChanged += (_, value) => ModSettings.CarEnginePower = value;
            enginePower_Slider.parent.eventVisibilityChanged += (_, isVisible) => enginePower_Slider.value = ModSettings.CarEnginePower;
            currentY += enginePower_Slider.parent.height + SettingsPanel.Margin;

            var brakingForce_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKINGFORCE), 5f, 200f, 1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1, numberFormat: "N0", suffix: " KN"));
            brakingForce_Slider.eventValueChanged += (_, value) => ModSettings.CarBrakingForce = value;
            brakingForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakingForce_Slider.value = ModSettings.CarBrakingForce;
            currentY += brakingForce_Slider.parent.height + SettingsPanel.Margin;

            var downForce_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DOWNFORCE), 0.0f, 50.0f, 1.0f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1, numberFormat: "N"));
            downForce_Slider.eventValueChanged += (_, value) => ModSettings.CarDownForce = value;
            downForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => downForce_Slider.value = ModSettings.CarDownForce;
            currentY += downForce_Slider.parent.height + SettingsPanel.Margin;

            var driveBias_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DRIVEBIAS), 0.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 100.0f, roundToNearest: 1, numberFormat: "N", suffix: "%"));
            driveBias_Slider.eventValueChanged += (_, value) => ModSettings.CarDriveBias = value;
            driveBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => driveBias_Slider.value = ModSettings.CarDriveBias;
            currentY += driveBias_Slider.parent.height + SettingsPanel.Margin;

            var brakeBias_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKEBIAS), 0.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 100.0f, roundToNearest: 1, numberFormat: "N", suffix: "%"));
            brakeBias_Slider.eventValueChanged += (_, value) => ModSettings.CarBrakeBias = value;
            brakeBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakeBias_Slider.value = ModSettings.CarBrakeBias;
            currentY += brakeBias_Slider.parent.height + SettingsPanel.Margin;

            var springDamp_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGDAMP), 0.0f, 20.0f, 0.1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.1f, numberFormat: "N"));
            springDamp_Slider.eventValueChanged += (_, value) => ModSettings.CarSpringDamp = value;
            springDamp_Slider.parent.eventVisibilityChanged += (_, isVisible) => springDamp_Slider.value = ModSettings.CarSpringDamp;
            currentY += springDamp_Slider.parent.height + SettingsPanel.Margin;

            var springOffset_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGOFFSET), -2.0f, 2.0f, 0.025f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            springOffset_Slider.eventValueChanged += (_, value) => ModSettings.CarSpringOffset = value;
            springOffset_Slider.parent.eventVisibilityChanged += (_, isVisible) => springOffset_Slider.value = ModSettings.CarSpringOffset;
            currentY += springOffset_Slider.parent.height + SettingsPanel.Margin;

            var springSwayBar_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGSWAYBAR), 0.0f, 200.0f, 1.0f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1.0f, numberFormat: "N"));
            springSwayBar_Slider.eventValueChanged += (_, value) => ModSettings.CarSpringSwayBar = value;
            springSwayBar_Slider.parent.eventVisibilityChanged += (_, isVisible) => springSwayBar_Slider.value = ModSettings.CarSpringSwayBar;
            currentY += springSwayBar_Slider.parent.height + SettingsPanel.Margin;

            var massCenterHeight_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERHEIGHT), -1.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            massCenterHeight_Slider.eventValueChanged += (_, value) => ModSettings.CarMassCenterHeight = value;
            massCenterHeight_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterHeight_Slider.value = ModSettings.CarMassCenterHeight;
            currentY += massCenterHeight_Slider.parent.height + SettingsPanel.Margin;

            var massCenterBias_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERBIAS), 0.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            massCenterBias_Slider.eventValueChanged += (_, value) => ModSettings.CarMassCenterBias = value;
            massCenterBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterBias_Slider.value = ModSettings.CarMassCenterBias;
            currentY += massCenterBias_Slider.parent.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
