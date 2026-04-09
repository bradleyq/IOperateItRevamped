using AlgernonCommons;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using DriveIt.Utils;
using UnityEngine;

namespace DriveIt.Settings.Tabs
{
    internal class Configuration
    {
        public static void Refresh(UIComponent panel)
        {
            panel.isVisible = false;
            panel.Disable();

            panel.Enable();
            panel.isVisible = true;
        }
        public static void Populate(UIComponent panel)
        {
            float currentY = 0.0f;
            float headerWidth = panel.width;

            #region General Group
            var groupGeneral = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_GENERAL));
            currentY += groupGeneral.height + SettingsPanel.LargeMargin;

            var language_DropDown = UIDropDowns.AddPlainDropDown(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_LANGUAGE_CHOICE), Translations.LanguageList, Translations.Index, width:300);
            language_DropDown.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<SettingsPanel>.LocaleChanged();
                UI.MainPanel.instance?.LocaleChanged();
            };
            language_DropDown.parent.eventVisibilityChanged += (_, isVisible) => language_DropDown.selectedIndex = Translations.Index;
            currentY += language_DropDown.parent.height + SettingsPanel.Margin;

            var logging_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_DETAIL_LOGGING));
            logging_CheckBox.eventCheckChanged += (_, isChecked) => Logging.DetailLogging = isChecked;
            logging_CheckBox.eventVisibilityChanged += (_, isVisible) => logging_CheckBox.isChecked = Logging.DetailLogging;
            currentY += logging_CheckBox.height + SettingsPanel.Margin;

            var restoreDefaults_Button = UIButtons.AddButton(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_RESTOREDEFAULT));
            restoreDefaults_Button.eventClicked += (_, value) => { ModSettings.RestoreDefaults(); Refresh(panel); };
            currentY += restoreDefaults_Button.height + SettingsPanel.MediumMargin;
            #endregion

            #region Game Group
            var groupGame = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_GAME));
            currentY += groupGame.height += SettingsPanel.LargeMargin;

            var buildingCollision_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BUILDINGCOLLISION));
            buildingCollision_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.BuildingCollision = isChecked;
            buildingCollision_CheckBox.eventVisibilityChanged += (_, isVisible) => buildingCollision_CheckBox.isChecked = ModSettings.BuildingCollision;
            currentY += buildingCollision_CheckBox.height + SettingsPanel.Margin;

            var vehicleCollision_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_VEHICLECOLLISION));
            vehicleCollision_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.VehicleCollision = isChecked;
            vehicleCollision_CheckBox.eventVisibilityChanged += (_, isVisible) => vehicleCollision_CheckBox.isChecked = ModSettings.VehicleCollision;
            currentY += vehicleCollision_CheckBox.height + SettingsPanel.MediumMargin;
            #endregion

            #region Vehicle Group
            var groupVehicle = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_VEHICLE));
            currentY += groupVehicle.height + SettingsPanel.LargeMargin;

            var maxVelocity_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_MAXVELOCITY), 25f, 350f, 1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1, numberFormat: "N0", suffix: " km/h"));
            maxVelocity_Slider.eventValueChanged += (_, value) => ModSettings.MaxVelocity = value;
            maxVelocity_Slider.parent.eventVisibilityChanged += (_, isVisible) => { maxVelocity_Slider.value = ModSettings.MaxVelocity; };
            currentY += maxVelocity_Slider.parent.height + SettingsPanel.Margin;

            var enginePower_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_ENGINEPOWER), 10f, 1000f, 1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 10, numberFormat: "N0", suffix: " KW"));
            enginePower_Slider.eventValueChanged += (_, value) => ModSettings.EnginePower = value;
            enginePower_Slider.parent.eventVisibilityChanged += (_, isVisible) => enginePower_Slider.value = ModSettings.EnginePower;
            currentY += enginePower_Slider.parent.height + SettingsPanel.Margin;

            var brakingForce_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKINGFORCE), 5f, 200f, 1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1, numberFormat: "N0", suffix: " KN"));
            brakingForce_Slider.eventValueChanged += (_, value) => ModSettings.BrakingForce = value;
            brakingForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakingForce_Slider.value = ModSettings.BrakingForce;
            currentY += brakingForce_Slider.parent.height + SettingsPanel.Margin;

            var AutoTrans_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_AUTOMATICTRANS));
            AutoTrans_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.AutoTrans = isChecked;
            AutoTrans_CheckBox.eventVisibilityChanged += (_, isVisible) => AutoTrans_CheckBox.isChecked = ModSettings.AutoTrans;
            currentY += AutoTrans_CheckBox.height + SettingsPanel.Margin;

            var brakingABS_CheckBox = UICheckBoxes.AddPlainCheckBox(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_BRAKINGABS));
            brakingABS_CheckBox.eventCheckChanged += (_, isChecked) => ModSettings.BrakingABS = isChecked;
            brakingABS_CheckBox.eventVisibilityChanged += (_, isVisible) => brakingABS_CheckBox.isChecked = ModSettings.BrakingABS;
            currentY += brakingABS_CheckBox.height + SettingsPanel.Margin;

            string[] tractionLevels = {
                Translations.Translate(DriveCommon.TK_SETTINGS_TRACTIONCTL_FULL),
                Translations.Translate(DriveCommon.TK_SETTINGS_TRACTIONCTL_SPORT),
                Translations.Translate(DriveCommon.TK_SETTINGS_TRACTIONCTL_TRACK),
                Translations.Translate(DriveCommon.TK_SETTINGS_TRACTIONCTL_OFF)
            };
            var TCS_Dropdown = UIDropDowns.AddPlainDropDown(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_TRACTIONCTL), tractionLevels, width:300);
            TCS_Dropdown.eventSelectedIndexChanged += (_, index) => ModSettings.TCSLevel = index;
            TCS_Dropdown.parent.eventVisibilityChanged += (_, isVisible) => TCS_Dropdown.selectedIndex = ModSettings.TCSLevel;
            currentY += TCS_Dropdown.parent.height + SettingsPanel.Margin;

            var downForce_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_DOWNFORCE"), 0.0f, 50.0f, 1.0f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1, numberFormat: "N"));
            downForce_Slider.eventValueChanged += (_, value) => ModSettings.DownForce = value;
            downForce_Slider.parent.eventVisibilityChanged += (_, isVisible) => downForce_Slider.value = ModSettings.DownForce;
            currentY += downForce_Slider.parent.height + SettingsPanel.Margin;

            var driveBias_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_DRIVEBIAS"), 0.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            driveBias_Slider.eventValueChanged += (_, value) => ModSettings.DriveBias = value;
            driveBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => driveBias_Slider.value = ModSettings.DriveBias;
            currentY += driveBias_Slider.parent.height + SettingsPanel.Margin;

            var brakeBias_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_BRAKEBIAS"), 0.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            brakeBias_Slider.eventValueChanged += (_, value) => ModSettings.BrakeBias = value;
            brakeBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => brakeBias_Slider.value = ModSettings.BrakeBias;
            currentY += brakeBias_Slider.parent.height + SettingsPanel.Margin;

            var gripCoeffS_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_GRIPCOEFFS"), 0.0f, 2.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            gripCoeffS_Slider.eventValueChanged += (_, value) => ModSettings.GripCoeffS = value;
            gripCoeffS_Slider.parent.eventVisibilityChanged += (_, isVisible) => gripCoeffS_Slider.value = ModSettings.GripCoeffS;
            currentY += gripCoeffS_Slider.parent.height + SettingsPanel.Margin;

            var gripCoeffK_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_GRIPCOEFFK"), 0.0f, 2.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            gripCoeffK_Slider.eventValueChanged += (_, value) => ModSettings.GripCoeffK = value;
            gripCoeffK_Slider.parent.eventVisibilityChanged += (_, isVisible) => gripCoeffK_Slider.value = ModSettings.GripCoeffK;
            currentY += gripCoeffK_Slider.parent.height + SettingsPanel.Margin;

            var springDamp_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_SPRINGDAMP"), 0.0f, 20.0f, 0.1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.1f, numberFormat: "N"));
            springDamp_Slider.eventValueChanged += (_, value) => ModSettings.SpringDamp = value;
            springDamp_Slider.parent.eventVisibilityChanged += (_, isVisible) => springDamp_Slider.value = ModSettings.SpringDamp;
            currentY += springDamp_Slider.parent.height + SettingsPanel.Margin;

            var springOffset_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_SPRINGOFFSET"), -2.0f, 2.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            springOffset_Slider.eventValueChanged += (_, value) => ModSettings.SpringOffset = value;
            springOffset_Slider.parent.eventVisibilityChanged += (_, isVisible) => springOffset_Slider.value = ModSettings.SpringOffset;
            currentY += springOffset_Slider.parent.height + SettingsPanel.Margin;

            var springSwayBar_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_SPRINGSWAYBAR"), 0.0f, 200.0f, 1.0f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1.0f, numberFormat: "N"));
            springSwayBar_Slider.eventValueChanged += (_, value) => ModSettings.SpringSwayBar = value;
            springSwayBar_Slider.parent.eventVisibilityChanged += (_, isVisible) => springSwayBar_Slider.value = ModSettings.SpringSwayBar;
            currentY += springSwayBar_Slider.parent.height + SettingsPanel.Margin;

            var massFactor_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_MASSFACTOR"), 5.0f, 200.0f, 5.0f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 5.0f, numberFormat: "N"));
            massFactor_Slider.eventValueChanged += (_, value) => ModSettings.MassFactor = value;
            massFactor_Slider.parent.eventVisibilityChanged += (_, isVisible) => massFactor_Slider.value = ModSettings.MassFactor;
            currentY += massFactor_Slider.parent.height + SettingsPanel.Margin;

            var massCenterHeight_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_MASSCENTERHEIGHT"), -1.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            massCenterHeight_Slider.eventValueChanged += (_, value) => ModSettings.MassCenterHeight = value;
            massCenterHeight_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterHeight_Slider.value = ModSettings.MassCenterHeight;
            currentY += massCenterHeight_Slider.parent.height + SettingsPanel.Margin;

            var massCenterBias_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_MASSCENTERBIAS"), 0.0f, 1.0f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            massCenterBias_Slider.eventValueChanged += (_, value) => ModSettings.MassCenterBias = value;
            massCenterBias_Slider.parent.eventVisibilityChanged += (_, isVisible) => massCenterBias_Slider.value = ModSettings.MassCenterBias;
            currentY += massCenterBias_Slider.parent.height + SettingsPanel.Margin;
            #endregion

            #region Camera Group
            var groupCamera = UISpacers.AddTitleSpacer(panel, SettingsPanel.Margin, currentY, headerWidth, Translations.Translate(DriveCommon.TK_SETTINGS_GROUP_CAMERA));
            currentY += groupCamera.height + SettingsPanel.LargeMargin;

            var cameraX_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_OFFSET_X), -20f, 20f, 0.1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.1f, numberFormat: "N"));
            cameraX_Slider.eventValueChanged += (_, value) => ModSettings.Offset.x = value;
            cameraX_Slider.parent.eventVisibilityChanged += (_, isVisible) => cameraX_Slider.value = ModSettings.Offset.x;
            currentY += cameraX_Slider.parent.height + SettingsPanel.Margin;

            var cameraY_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_OFFSET_Y), -20f, 20f, 0.1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.1f, numberFormat: "N"));
            cameraY_Slider.eventValueChanged += (_, value) => ModSettings.Offset.y = value;
            cameraY_Slider.parent.eventVisibilityChanged += (_, isVisible) => cameraY_Slider.value = ModSettings.Offset.y;
            currentY += cameraY_Slider.parent.height + SettingsPanel.Margin;

            var cameraZ_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate(DriveCommon.TK_SETTINGS_OFFSET_Z), -20f, 20f, 0.1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.1f, numberFormat: "N"));
            cameraZ_Slider.eventValueChanged += (_, value) => ModSettings.Offset.z = value;
            cameraZ_Slider.parent.eventVisibilityChanged += (_, isVisible) => cameraZ_Slider.value = ModSettings.Offset.z;
            currentY += cameraZ_Slider.parent.height + SettingsPanel.Margin;

            var cameraMouseRot_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_CAM_MOUSE_ROTATE_SENSITIVITY"), 0.1f, 4f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            cameraMouseRot_Slider.eventValueChanged += (_, value) => ModSettings.CamMouseRotateSensitivity = value;
            cameraMouseRot_Slider.parent.eventVisibilityChanged += (_, isVisible) => cameraMouseRot_Slider.value = ModSettings.CamMouseRotateSensitivity;
            currentY += cameraMouseRot_Slider.parent.height + SettingsPanel.Margin;

            var cameraKeyRot_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_CAM_KEY_ROTATE_SENSITIVITY"), 0.1f, 4f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            cameraKeyRot_Slider.eventValueChanged += (_, value) => ModSettings.CamKeyRotateSensitivity = value;
            cameraKeyRot_Slider.parent.eventVisibilityChanged += (_, isVisible) => cameraKeyRot_Slider.value = ModSettings.CamKeyRotateSensitivity;
            currentY += cameraKeyRot_Slider.parent.height + SettingsPanel.Margin;

            var cameraFOV_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_CAM_FIELD_OF_VIEW"), 30f, 120f, 1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0"));
            cameraFOV_Slider.eventValueChanged += (_, value) => ModSettings.CamFieldOfView = value;
            cameraFOV_Slider.parent.eventVisibilityChanged += (_, isVisible) => cameraFOV_Slider.value = ModSettings.CamFieldOfView;
            currentY += cameraFOV_Slider.parent.height + SettingsPanel.Margin;

            var cameraPitch_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_CAM_MAX_PITCH_DEG"), 0f, 89f, 1f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 1f, numberFormat: "N0"));
            cameraPitch_Slider.eventValueChanged += (_, value) => ModSettings.CamMaxPitch = value;
            cameraPitch_Slider.parent.eventVisibilityChanged += (_, isVisible) => cameraPitch_Slider.value = ModSettings.CamMaxPitch;
            currentY += cameraPitch_Slider.parent.height + SettingsPanel.Margin;

            var cameraSmoothing_Slider = UISliders.AddPlainSliderWithValue(panel, SettingsPanel.MediumMargin, currentY, Translations.Translate("SETTINGS_CAM_SMOOTHING"), 0.0f, 4f, 0.05f, 0f, new UISliders.SliderValueFormat(valueMultiplier: 1, roundToNearest: 0.05f, numberFormat: "N"));
            cameraSmoothing_Slider.eventValueChanged += (_, value) => ModSettings.CamSmoothing = value;
            cameraSmoothing_Slider.parent.eventVisibilityChanged += (_, isVisible) => cameraSmoothing_Slider.value = ModSettings.CamSmoothing;
            currentY += cameraSmoothing_Slider.parent.height + SettingsPanel.Margin;
            #endregion
        }
    }
}
