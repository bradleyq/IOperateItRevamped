using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;

namespace DriveIt.Settings.Tabs
{
    internal class ConfigTrain
    {
        public static void Populate(UIComponent panel)
        {
            float currentY = 0.0f;
            float headerWidth = panel.width;

            #region Train Vehicle Group
            var groupGeneral = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_TRAIN));
            currentY += groupGeneral.height + SettingsPanel.LargeMargin;

            var enginePower_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_ENGINEPOWER), 5.0f, 5000.0f, 5.0f, 1.0f, "N0", " kW");
            enginePower_Slider.eventValueChanged += (_, value) => ModSettings.TrainEnginePower = value;
            enginePower_Slider.parent.eventVisibilityChanged += (_, isVisible) => enginePower_Slider.value = ModSettings.TrainEnginePower;
            currentY += enginePower_Slider.parent.height + SettingsPanel.Margin;

            var brakingForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKINGFORCE), 5.0f, 500.0f, 1.0f, 1.0f, "N0", " kN");
            brakingForce_Slider.eventValueChanged += (_, value) => ModSettings.TrainBrakingForce = value;
            brakingForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakingForce_Slider.value = ModSettings.TrainBrakingForce;
            currentY += brakingForce_Slider.parent.height + SettingsPanel.Margin;

            var downForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DOWNFORCE), 0.0f, 50.0f, 1.0f, 1.0f, "N0", " Ns/m");
            downForce_Slider.eventValueChanged += (_, value) => ModSettings.TrainDownForce = value;
            downForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => downForce_Slider.value = ModSettings.TrainDownForce;
            currentY += downForce_Slider.parent.height + SettingsPanel.Margin;

            var driveBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DRIVEBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            driveBias_Slider.eventValueChanged += (_, value) => ModSettings.TrainDriveBias = value;
            driveBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => driveBias_Slider.value = ModSettings.TrainDriveBias;
            currentY += driveBias_Slider.parent.height + SettingsPanel.Margin;

            var brakeBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKEBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            brakeBias_Slider.eventValueChanged += (_, value) => ModSettings.TrainBrakeBias = value;
            brakeBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakeBias_Slider.value = ModSettings.TrainBrakeBias;
            currentY += brakeBias_Slider.parent.height + SettingsPanel.Margin;

            var springDamp_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGDAMP), 0.0f, 20.0f, 0.1f, 1.0f, "N1", null);
            springDamp_Slider.eventValueChanged += (_, value) => ModSettings.TrainSpringDamp = value;
            springDamp_Slider.parent.eventVisibilityChanged += (_, isVisible) => springDamp_Slider.value = ModSettings.TrainSpringDamp;
            currentY += springDamp_Slider.parent.height + SettingsPanel.Margin;

            var springOffset_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGOFFSET), -2.0f, 2.0f, 0.025f, -1000.0f, "N0", " mm");
            springOffset_Slider.eventValueChanged += (_, value) => ModSettings.TrainSpringOffset = value;
            springOffset_Slider.parent.eventVisibilityChanged += (_, isVisible) => springOffset_Slider.value = ModSettings.TrainSpringOffset;
            currentY += springOffset_Slider.parent.height + SettingsPanel.Margin;

            var springSwayBar_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGSWAYBAR), 0.0f, 200.0f, 1.0f, 1.0f, "N0", " kN/m");
            springSwayBar_Slider.eventValueChanged += (_, value) => ModSettings.TrainSpringSwayBar = value;
            springSwayBar_Slider.parent.eventVisibilityChanged += (_, isVisible) => springSwayBar_Slider.value = ModSettings.TrainSpringSwayBar;
            currentY += springSwayBar_Slider.parent.height + SettingsPanel.Margin;

            var massCenterHeight_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERHEIGHT), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterHeight_Slider.eventValueChanged += (_, value) => ModSettings.TrainMassCenterHeight = value;
            massCenterHeight_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterHeight_Slider.value = ModSettings.TrainMassCenterHeight;
            currentY += massCenterHeight_Slider.parent.height + SettingsPanel.Margin;

            var massCenterBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterBias_Slider.eventValueChanged += (_, value) => ModSettings.TrainMassCenterBias = value;
            massCenterBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterBias_Slider.value = ModSettings.TrainMassCenterBias;
            currentY += massCenterBias_Slider.parent.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
