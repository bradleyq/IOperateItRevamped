using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;

namespace DriveIt.Settings.Tabs
{
    internal class ConfigTrailer
    {
        public static void Populate(UIComponent panel)
        {
            float currentY = 0.0f;
            float headerWidth = panel.width;

            #region Trailer Vehicle Group
            var groupGeneral = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_TRAILER));
            currentY += groupGeneral.height + SettingsPanel.LargeMargin;

            var brakingForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKINGFORCE), 5.0f, 200.0f, 1.0f, 1.0f, "N0", " kN");
            brakingForce_Slider.eventValueChanged += (_, value) => ModSettings.TrailerBrakingForce = value;
            brakingForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakingForce_Slider.value = ModSettings.TrailerBrakingForce;
            currentY += brakingForce_Slider.parent.height + SettingsPanel.Margin;

            var downForce_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_DOWNFORCE), 0.0f, 50.0f, 1.0f, 1.0f, "N0", " Ns/m");
            downForce_Slider.eventValueChanged += (_, value) => ModSettings.TrailerDownForce = value;
            downForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => downForce_Slider.value = ModSettings.TrailerDownForce;
            currentY += downForce_Slider.parent.height + SettingsPanel.Margin;

            var springDamp_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGDAMP), 0.0f, 20.0f, 0.1f, 1.0f, "N1", null);
            springDamp_Slider.eventValueChanged += (_, value) => ModSettings.TrailerSpringDamp = value;
            springDamp_Slider.parent.eventVisibilityChanged += (_, isVisible) => springDamp_Slider.value = ModSettings.TrailerSpringDamp;
            currentY += springDamp_Slider.parent.height + SettingsPanel.Margin;

            var springOffset_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGOFFSET), -2.0f, 2.0f, 0.025f, -1000.0f, "N0", " mm");
            springOffset_Slider.eventValueChanged += (_, value) => ModSettings.TrailerSpringOffset = value;
            springOffset_Slider.parent.eventVisibilityChanged += (_, isVisible) => springOffset_Slider.value = ModSettings.TrailerSpringOffset;
            currentY += springOffset_Slider.parent.height + SettingsPanel.Margin;

            var springSwayBar_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_SPRINGSWAYBAR), 0.0f, 200.0f, 1.0f, 1.0f, "N0", " kN/m");
            springSwayBar_Slider.eventValueChanged += (_, value) => ModSettings.TrailerSpringSwayBar = value;
            springSwayBar_Slider.parent.eventVisibilityChanged += (_, isVisible) => springSwayBar_Slider.value = ModSettings.TrailerSpringSwayBar;
            currentY += springSwayBar_Slider.parent.height + SettingsPanel.Margin;

            var massCenterHeight_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERHEIGHT), -1.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterHeight_Slider.eventValueChanged += (_, value) => ModSettings.TrailerMassCenterHeight = value;
            massCenterHeight_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterHeight_Slider.value = ModSettings.TrailerMassCenterHeight;
            currentY += massCenterHeight_Slider.parent.height + SettingsPanel.Margin;

            var massCenterBias_Slider = SettingsPanel.CreateSlider(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MASSCENTERBIAS), 0.0f, 1.0f, 0.05f, 100.0f, "N0", "%");
            massCenterBias_Slider.eventValueChanged += (_, value) => ModSettings.TrailerMassCenterBias = value;
            massCenterBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterBias_Slider.value = ModSettings.TrailerMassCenterBias;
            currentY += massCenterBias_Slider.parent.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
