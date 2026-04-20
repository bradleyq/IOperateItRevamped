using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;

namespace DriveIt.Settings.Tabs
{
    internal class ConfigBike
    {
        public static void Populate(UIComponent panel)
        {
            float currentY = 0.0f;
            float headerWidth = panel.width;

            #region Bike Vehicle Group
            var groupGeneral = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_BIKE));
            currentY += groupGeneral.height + SettingsPanel.LargeMargin;

            var enginePower_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_ENGINEPOWER), 5.0f, 500.0f, 5.0f, 1.0f, "N0", " kW");
            enginePower_Slider.eventValueChanged += (_, value) => ModSettings.BikeEnginePower = value;
            enginePower_Slider.parent.eventVisibilityChanged += (_, isVisible) => enginePower_Slider.value = ModSettings.BikeEnginePower;
            currentY += enginePower_Slider.parent.height + SettingsPanel.Margin;

            var brakingForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKINGFORCE), 5.0f, 200.0f, 1.0f, 1.0f, "N0", " kN");
            brakingForce_Slider.eventValueChanged += (_, value) => ModSettings.BikeBrakingForce = value;
            brakingForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakingForce_Slider.value = ModSettings.BikeBrakingForce;
            currentY += brakingForce_Slider.parent.height + SettingsPanel.Margin;

            var downForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DOWNFORCE), 0.0f, 50.0f, 1.0f, 1.0f, "N0", " Ns/m");
            downForce_Slider.eventValueChanged += (_, value) => ModSettings.BikeDownForce = value;
            downForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => downForce_Slider.value = ModSettings.BikeDownForce;
            currentY += downForce_Slider.parent.height + SettingsPanel.Margin;

            var driveBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DRIVEBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            driveBias_Slider.eventValueChanged += (_, value) => ModSettings.BikeDriveBias = value;
            driveBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => driveBias_Slider.value = ModSettings.BikeDriveBias;
            currentY += driveBias_Slider.parent.height + SettingsPanel.Margin;

            var brakeBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKEBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            brakeBias_Slider.eventValueChanged += (_, value) => ModSettings.BikeBrakeBias = value;
            brakeBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakeBias_Slider.value = ModSettings.BikeBrakeBias;
            currentY += brakeBias_Slider.parent.height + SettingsPanel.Margin;

            var springDamp_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGDAMP), 0.0f, 20.0f, 0.1f, 1.0f, "N1", null);
            springDamp_Slider.eventValueChanged += (_, value) => ModSettings.BikeSpringDamp = value;
            springDamp_Slider.parent.eventVisibilityChanged += (_, isVisible) => springDamp_Slider.value = ModSettings.BikeSpringDamp;
            currentY += springDamp_Slider.parent.height + SettingsPanel.Margin;

            var springOffset_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGOFFSET), -2.0f, 2.0f, 0.025f, -1000.0f, "N0", " mm");
            springOffset_Slider.eventValueChanged += (_, value) => ModSettings.BikeSpringOffset = value;
            springOffset_Slider.parent.eventVisibilityChanged += (_, isVisible) => springOffset_Slider.value = ModSettings.BikeSpringOffset;
            currentY += springOffset_Slider.parent.height + SettingsPanel.Margin;

            var massCenterHeight_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERHEIGHT), -1.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterHeight_Slider.eventValueChanged += (_, value) => ModSettings.BikeMassCenterHeight = value;
            massCenterHeight_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterHeight_Slider.value = ModSettings.BikeMassCenterHeight;
            currentY += massCenterHeight_Slider.parent.height + SettingsPanel.Margin;

            var massCenterBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterBias_Slider.eventValueChanged += (_, value) => ModSettings.BikeMassCenterBias = value;
            massCenterBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterBias_Slider.value = ModSettings.BikeMassCenterBias;
            currentY += massCenterBias_Slider.parent.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
