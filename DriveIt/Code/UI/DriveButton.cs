using AlgernonCommons.Translation;
using ColossalFramework;
using ColossalFramework.UI;
using DriveIt.Utils;
using UnityEngine;

namespace DriveIt.UI
{
    public class DriveButton : MonoBehaviour
    {
        public static DriveButton instance { get; private set; }

        private CitizenVehicleWorldInfoPanel citizenVehicleInfo_Panel;
        private UIButton citizenVehicleInfo_Button;

        private CityServiceVehicleWorldInfoPanel cityServiceVehicleInfo_Panel;
        private UIButton cityServiceVehicleInfo_Button;

        private PublicTransportVehicleWorldInfoPanel publicTransportVehicleInfo_Panel;
        private UIButton publicTransportVehicleInfo_Button;

        private RaceVehicleWorldInfoPanel raceVehicleWorldInfo_Panel;
        private UIButton raceVehicleWorldInfo_Button;

        private void Awake()
        {
            if (instance)
            {
                Destroy(this);
                return;
            }
            instance = this;

            citizenVehicleInfo_Button = Initialize(ref citizenVehicleInfo_Panel);
            cityServiceVehicleInfo_Button = Initialize(ref cityServiceVehicleInfo_Panel);
            publicTransportVehicleInfo_Button = Initialize(ref publicTransportVehicleInfo_Panel);
            raceVehicleWorldInfo_Button = Initialize(ref raceVehicleWorldInfo_Panel);
        }

        private void Update()
        {
            UpdateButtonVisibility(citizenVehicleInfo_Panel, citizenVehicleInfo_Button);
            UpdateButtonVisibility(cityServiceVehicleInfo_Panel, cityServiceVehicleInfo_Button);
            UpdateButtonVisibility(publicTransportVehicleInfo_Panel, publicTransportVehicleInfo_Button);
            UpdateButtonVisibility(raceVehicleWorldInfo_Panel, raceVehicleWorldInfo_Button);
        }

        private void OnDestroy()
        {
            Destroy(citizenVehicleInfo_Button);
            Destroy(cityServiceVehicleInfo_Button);
            Destroy(publicTransportVehicleInfo_Button);
            Destroy(raceVehicleWorldInfo_Button);
        }

        public void SetEnable() => enabled = true;

        public void SetDisable() => enabled = false;

        private UIButton Initialize<T>(ref T panel) where T : WorldInfoPanel, new()
        {
            panel = UIView.library.Get<T>(typeof(T).Name);
            return CreateDriveButton(panel);
        }

        private UIButton CreateDriveButton<T>(T panel) where T : WorldInfoPanel, new()
        {            
            UIButton button = panel.component.AddUIComponent<UIButton>();
            DriveCommon.FormatDriveButton(button);
            button.name = panel.component.name + "_Drive";
            button.tooltip = Translations.Translate(DriveCommon.TK_DRIVEBTN_TOOLTIP);
            button.eventClick += (_, p) =>
            {
                InstanceID instanceID = WorldInfoPanel.GetCurrentInstanceID();
                if (instanceID.Type == InstanceType.Vehicle)
                {
                    ref Vehicle vehicle = ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[instanceID.Vehicle];
                    Color color = vehicle.Info.m_vehicleAI.GetColor(instanceID.Vehicle, ref vehicle, Singleton<InfoManager>.instance.CurrentMode, Singleton<InfoManager>.instance.CurrentSubMode);
                    VehicleInfo vehicleAltInfo = null;
                    Vehicle.Flags flagsAlt = 0;
                    int variationAlt = 0;
                    if (vehicle.m_trailingVehicle > 0)
                    {
                        ref Vehicle vehicleAlt = ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicle.m_trailingVehicle];
                        vehicleAltInfo = vehicleAlt.Info;
                        flagsAlt = vehicleAlt.m_flags;
                        variationAlt = vehicleAlt.m_gateIndex;
                    }
                    DriveController.instance.StartDriving(vehicle.GetLastFramePosition(), vehicle.GetLastFrameData().m_rotation, vehicle.Info, vehicle.m_flags, vehicle.m_gateIndex, vehicleAltInfo, flagsAlt, variationAlt, color, true);
                    MainPanel.instance._vehicleList.FindItem<uint>(vehicle.m_infoIndex);
                }
                else if (instanceID.Type == InstanceType.ParkedVehicle)
                {
                    ref VehicleParked vehicleParked = ref Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[instanceID.ParkedVehicle];
                    Color color = vehicleParked.Info.m_vehicleAI.GetColor(instanceID.Vehicle, ref vehicleParked, Singleton<InfoManager>.instance.CurrentMode, Singleton<InfoManager>.instance.CurrentSubMode);
                    DriveController.instance.StartDriving(vehicleParked.m_position, vehicleParked.m_rotation, vehicleParked.Info, 0, 0, null, 0, 0, color, true);
                    MainPanel.instance._vehicleList.FindItem<uint>(vehicleParked.m_infoIndex);
                }
                panel.component.isVisible = false;
            };
            button.AlignTo(panel.component, UIAlignAnchor.TopRight);
            button.relativePosition = new Vector2
           (
                button.relativePosition.x - 5f,
                button.relativePosition.y + 45f);
            return button;
        }
        private void UpdateButtonVisibility<T>(T panel, UIButton button) where T : WorldInfoPanel, new()
        {
            if (panel.component.isVisible)
            {
                var instanceID = WorldInfoPanel.GetCurrentInstanceID();
                button.isVisible = instanceID != default;
            }
        }
    }
}
