using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;

namespace DriveIt.Settings.Tabs
{
    internal class ConfigHeli
    {
        public static void Populate(UIComponent panel)
        {
            float currentY = 0.0f;
            float headerWidth = panel.width;

            #region Heli Vehicle Group
            var groupGeneral = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_HELI));
            currentY += groupGeneral.height + SettingsPanel.LargeMargin;

            var enginePower_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_ENGINEPOWER), 10f, 1000f, 1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 10, numberFormat: "N0", suffix: " KW"));
            enginePower_Slider.eventValueChanged += (_, value) => ModSettings.HeliEnginePower = value;
            enginePower_Slider.parent.eventVisibilityChanged += (_, isVisible) => enginePower_Slider.value = ModSettings.HeliEnginePower;
            currentY += enginePower_Slider.parent.height + SettingsPanel.Margin;

            var springDamp_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGDAMP), 0.0f, 20.0f, 0.1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.1f, numberFormat: "N"));
            springDamp_Slider.eventValueChanged += (_, value) => ModSettings.HeliSpringDamp = value;
            springDamp_Slider.parent.eventVisibilityChanged += (_, isVisible) => springDamp_Slider.value = ModSettings.HeliSpringDamp;
            currentY += springDamp_Slider.parent.height + SettingsPanel.Margin;

            var springOffset_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGOFFSET), -2.0f, 2.0f, 0.025f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            springOffset_Slider.eventValueChanged += (_, value) => ModSettings.HeliSpringOffset = value;
            springOffset_Slider.parent.eventVisibilityChanged += (_, isVisible) => springOffset_Slider.value = ModSettings.HeliSpringOffset;
            currentY += springOffset_Slider.parent.height + SettingsPanel.Margin;

            var massCenterHeight_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERHEIGHT), -1.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            massCenterHeight_Slider.eventValueChanged += (_, value) => ModSettings.HeliMassCenterHeight = value;
            massCenterHeight_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterHeight_Slider.value = ModSettings.HeliMassCenterHeight;
            currentY += massCenterHeight_Slider.parent.height + SettingsPanel.Margin;

            var massCenterBias_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERBIAS), 0.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            massCenterBias_Slider.eventValueChanged += (_, value) => ModSettings.HeliMassCenterBias = value;
            massCenterBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterBias_Slider.value = ModSettings.HeliMassCenterBias;
            currentY += massCenterBias_Slider.parent.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
