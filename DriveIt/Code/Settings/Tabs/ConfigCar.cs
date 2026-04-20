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

            var enginePower_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_ENGINEPOWER), 5.0f, 1000.0f, 5.0f, 1.0f, "N0", " kW");
            enginePower_Slider.eventValueChanged += (_, value) => ModSettings.CarEnginePower = value;
            enginePower_Slider.parent.eventVisibilityChanged += (_, isVisible) => enginePower_Slider.value = ModSettings.CarEnginePower;
            currentY += enginePower_Slider.parent.height + SettingsPanel.Margin;

            var brakingForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKINGFORCE), 5.0f, 200.0f, 1.0f, 1.0f, "N0", " kN");
            brakingForce_Slider.eventValueChanged += (_, value) => ModSettings.CarBrakingForce = value;
            brakingForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakingForce_Slider.value = ModSettings.CarBrakingForce;
            currentY += brakingForce_Slider.parent.height + SettingsPanel.Margin;

            var downForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DOWNFORCE), 0.0f, 50.0f, 1.0f, 1.0f, "N0", " Ns/m");
            downForce_Slider.eventValueChanged += (_, value) => ModSettings.CarDownForce = value;
            downForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => downForce_Slider.value = ModSettings.CarDownForce;
            currentY += downForce_Slider.parent.height + SettingsPanel.Margin;

            var driveBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DRIVEBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            driveBias_Slider.eventValueChanged += (_, value) => ModSettings.CarDriveBias = value;
            driveBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => driveBias_Slider.value = ModSettings.CarDriveBias;
            currentY += driveBias_Slider.parent.height + SettingsPanel.Margin;

            var brakeBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKEBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            brakeBias_Slider.eventValueChanged += (_, value) => ModSettings.CarBrakeBias = value;
            brakeBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakeBias_Slider.value = ModSettings.CarBrakeBias;
            currentY += brakeBias_Slider.parent.height + SettingsPanel.Margin;

            var springDamp_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGDAMP), 0.0f, 20.0f, 0.1f, 1.0f, "N1", null);
            springDamp_Slider.eventValueChanged += (_, value) => ModSettings.CarSpringDamp = value;
            springDamp_Slider.parent.eventVisibilityChanged += (_, isVisible) => springDamp_Slider.value = ModSettings.CarSpringDamp;
            currentY += springDamp_Slider.parent.height + SettingsPanel.Margin;

            var springOffset_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGOFFSET), -2.0f, 2.0f, 0.025f, -1000.0f, "N0", " mm");
            springOffset_Slider.eventValueChanged += (_, value) => ModSettings.CarSpringOffset = value;
            springOffset_Slider.parent.eventVisibilityChanged += (_, isVisible) => springOffset_Slider.value = ModSettings.CarSpringOffset;
            currentY += springOffset_Slider.parent.height + SettingsPanel.Margin;

            var springSwayBar_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGSWAYBAR), 0.0f, 200.0f, 1.0f, 1.0f, "N0", " kN/m");
            springSwayBar_Slider.eventValueChanged += (_, value) => ModSettings.CarSpringSwayBar = value;
            springSwayBar_Slider.parent.eventVisibilityChanged += (_, isVisible) => springSwayBar_Slider.value = ModSettings.CarSpringSwayBar;
            currentY += springSwayBar_Slider.parent.height + SettingsPanel.Margin;

            var massCenterHeight_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERHEIGHT), -1.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterHeight_Slider.eventValueChanged += (_, value) => ModSettings.CarMassCenterHeight = value;
            massCenterHeight_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterHeight_Slider.value = ModSettings.CarMassCenterHeight;
            currentY += massCenterHeight_Slider.parent.height + SettingsPanel.Margin;

            var massCenterBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterBias_Slider.eventValueChanged += (_, value) => ModSettings.CarMassCenterBias = value;
            massCenterBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterBias_Slider.value = ModSettings.CarMassCenterBias;
            currentY += massCenterBias_Slider.parent.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
