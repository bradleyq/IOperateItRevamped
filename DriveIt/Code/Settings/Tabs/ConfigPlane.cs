using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;

namespace DriveIt.Settings.Tabs
{
    internal class ConfigPlane
    {
        public static void Populate(UIComponent panel)
        {
            float currentY = 0.0f;
            float headerWidth = panel.width;

            #region Plane Vehicle Group
            var groupGeneral = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_PLANE));
            currentY += groupGeneral.height + SettingsPanel.LargeMargin;

            var enginePower_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_ENGINEPOWER), 500.0f, 100000.0f, 500.0f, 1.0f, "N0", " kW");
            enginePower_Slider.eventValueChanged += (_, value) => ModSettings.PlaneEnginePower = value;
            enginePower_Slider.parent.eventVisibilityChanged += (_, isVisible) => enginePower_Slider.value = ModSettings.PlaneEnginePower;
            currentY += enginePower_Slider.parent.height + SettingsPanel.Margin;

            var brakingForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKINGFORCE), 5.0f, 1000.0f, 1.0f, 1.0f, "N0", " kN");
            brakingForce_Slider.eventValueChanged += (_, value) => ModSettings.PlaneBrakingForce = value;
            brakingForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakingForce_Slider.value = ModSettings.PlaneBrakingForce;
            currentY += brakingForce_Slider.parent.height + SettingsPanel.Margin;

            var downForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DOWNFORCE), -1000.0f, 0.0f, 1.0f, 1.0f, "N0", " Ns/m");
            downForce_Slider.eventValueChanged += (_, value) => ModSettings.PlaneDownForce = value;
            downForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => downForce_Slider.value = ModSettings.PlaneDownForce;
            currentY += downForce_Slider.parent.height + SettingsPanel.Margin;

            var brakeBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKEBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            brakeBias_Slider.eventValueChanged += (_, value) => ModSettings.PlaneBrakeBias = value;
            brakeBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakeBias_Slider.value = ModSettings.PlaneBrakeBias;
            currentY += brakeBias_Slider.parent.height + SettingsPanel.Margin;

            var springDamp_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGDAMP), 0.0f, 20.0f, 0.1f, 1.0f, "N1", null);
            springDamp_Slider.eventValueChanged += (_, value) => ModSettings.PlaneSpringDamp = value;
            springDamp_Slider.parent.eventVisibilityChanged += (_, isVisible) => springDamp_Slider.value = ModSettings.PlaneSpringDamp;
            currentY += springDamp_Slider.parent.height + SettingsPanel.Margin;

            var springOffset_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGOFFSET), -2.0f, 2.0f, 0.025f, -1000.0f, "N0", " mm");
            springOffset_Slider.eventValueChanged += (_, value) => ModSettings.PlaneSpringOffset = value;
            springOffset_Slider.parent.eventVisibilityChanged += (_, isVisible) => springOffset_Slider.value = ModSettings.PlaneSpringOffset;
            currentY += springOffset_Slider.parent.height + SettingsPanel.Margin;

            var massCenterHeight_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERHEIGHT), -1.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterHeight_Slider.eventValueChanged += (_, value) => ModSettings.PlaneMassCenterHeight = value;
            massCenterHeight_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterHeight_Slider.value = ModSettings.PlaneMassCenterHeight;
            currentY += massCenterHeight_Slider.parent.height + SettingsPanel.Margin;

            var massCenterBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterBias_Slider.eventValueChanged += (_, value) => ModSettings.PlaneMassCenterBias = value;
            massCenterBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterBias_Slider.value = ModSettings.PlaneMassCenterBias;
            currentY += massCenterBias_Slider.parent.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
