using AlgernonCommons.Keybinding;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;

namespace DriveIt.Settings.Tabs
{
    internal class Keybinds
    {
        internal static void Populate(UIComponent panel)
        {
            float currentY = 0.0f;
            float headerWidth = panel.width;

            #region Keybind Group
            var groupKeybind = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_KEYS));
            currentY += groupKeybind.height = SettingsPanel.LargeMargin;

            var keyUUIToggle = Utils.UUISupport.UUIKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY);
            currentY += keyUUIToggle.Panel.height + SettingsPanel.Margin;

            var keyLightToggle = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYLIGHTTOGGLE), ModSettings.KeyLightToggle);
            currentY += keyLightToggle.Panel.height + SettingsPanel.Margin;

            var keySirenToggle = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYSIRENTOGGLE), ModSettings.KeySirenToggle);
            currentY += keySirenToggle.Panel.height + SettingsPanel.Margin;

            var keyMoveForward = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYMOVEFORWARD), ModSettings.KeyMoveForward);
            currentY += keyMoveForward.Panel.height + SettingsPanel.Margin;

            var keyMoveBackward = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYMOVEBACKWARD), ModSettings.KeyMoveBackward);
            currentY += keyMoveBackward.Panel.height + SettingsPanel.Margin;
            
            var keyMoveLeft = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYMOVELEFT), ModSettings.KeyMoveLeft);
            currentY += keyMoveLeft.Panel.height + SettingsPanel.Margin;

            var keyMoveRight = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYMOVERIGHT), ModSettings.KeyMoveRight);
            currentY += keyMoveRight.Panel.height + SettingsPanel.Margin;

            var keyResetVehicle = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYRESETVEHICLE), ModSettings.KeyResetVehicle);
            currentY += keyResetVehicle.Panel.height + SettingsPanel.Margin;

            var keyGearUp = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYGEARUP), ModSettings.KeyGearUp);
            currentY += keyGearUp.Panel.height + SettingsPanel.Margin;

            var keyGearDown = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYGEARDOWN), ModSettings.KeyGearDown);
            currentY += keyGearDown.Panel.height + SettingsPanel.Margin;

            var keyCamCursorToggle = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYCAMCURSORTOGGLE), ModSettings.KeyCamCursorToggle);
            currentY += keyCamCursorToggle.Panel.height + SettingsPanel.Margin;

            var keyCamZoomIn = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYCAMZOOMIN), ModSettings.KeyCamZoomIn);
            currentY += keyCamZoomIn.Panel.height + SettingsPanel.Margin;

            var keyCamZoomOut = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYCAMZOOMOUT), ModSettings.KeyCamZoomOut);
            currentY += keyCamZoomOut.Panel.height + SettingsPanel.Margin;

            var keyCamReset = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYCAMRESET), ModSettings.KeyCamReset);
            currentY += keyCamReset.Panel.height + SettingsPanel.Margin;

            var keyCamRotateUp = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYROTATECAMUP), ModSettings.KeyCamRotateUp);
            currentY += keyCamRotateUp.Panel.height + SettingsPanel.Margin;

            var keyCamRotateDown = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYROTATECAMDOWN), ModSettings.KeyCamRotateDown);
            currentY += keyCamRotateDown.Panel.height + SettingsPanel.Margin;

            var keyCamRotateLeft = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYROTATECAMLEFT), ModSettings.KeyCamRotateLeft);
            currentY += keyCamRotateLeft.Panel.height + SettingsPanel.Margin;

            var keyCamRotateRight = OptionsKeymapping.AddKeymapping(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_KEYROTATECAMRIGHT), ModSettings.KeyCamRotateRight);
            currentY += keyCamRotateRight.Panel.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
