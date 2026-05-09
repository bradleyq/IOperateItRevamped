using DriveIt.Settings;
using DriveIt.UI;
using DriveIt.Utils;
using DriveIt.Vehicles;
using UnifiedUI.Helpers;
using UnityEngine;
using static DriveIt.Vehicles.VehicleGeneric;

namespace DriveIt
{
    public class DriveController : MonoBehaviour
    {
        public static DriveController instance { get; private set; }

        private DriveColliders m_collidersManager = new DriveColliders();
        private VehicleInfo m_vehicleInfo;
        private VehicleInfo m_vehicleInfoAlt;
        private Color m_vehicleColor;
        private Color m_vehicleColorAlt;
        private bool m_setColor = false;
        private bool m_setColorAlt = false;
        private VehicleGeneric m_vehicle;
        private VehicleGeneric m_vehicleAlt;

        public bool OnEsc()
        {
            if (enabled)
            {
                StopDriving();
                return true;
            }
            return false;
        }

        public void UpdateColors(Color color, bool enable, Color colorAlt, bool enableAlt)
        {
            UpdateColor(color, enable);
            UpdateColorAlt(colorAlt, enableAlt);
        }

        public void UpdateColor(Color color, bool enable)
        {
            m_vehicleColor = color;
            m_setColor = enable;
        }

        public void UpdateColorAlt(Color color, bool enable)
        {
            m_vehicleColorAlt = color;
            m_setColorAlt = enable;
        }

        public void UpdateVehicleInfos(VehicleInfo info, VehicleInfo infoAlt)
        {
            UpdateVehicleInfo(info);
            UpdateVehicleInfoAlt(infoAlt);  
        }

        public void UpdateVehicleInfo(VehicleInfo info)
        {
            m_vehicleInfo = info;
        }

        public void UpdateVehicleInfoAlt(VehicleInfo info)
        {
            m_vehicleInfoAlt = info;
        }

        public bool IsVehicleInfoSet()
        {
            return m_vehicleInfo != null;
        }
        public bool IsVehicleInfoAltSet()
        {
            return m_vehicleInfoAlt != null;
        }

        public void StartDriving(Vector3 position, Quaternion rotation) => StartDriving(position, rotation, m_vehicleInfo, 0, 0, m_vehicleInfoAlt, 0, 0, m_vehicleColor, m_setColor);

        public void StartDriving(Vector3 position, Quaternion rotation, 
            VehicleInfo vehicleInfo, Vehicle.Flags flags, int variation, 
            VehicleInfo vehicleInfoAlt, Vehicle.Flags flagsAlt, int variationAlt, Color vehicleColor, bool setColor)
        {
            enabled = true;
            Vector3 spawnPosition = position;
            Quaternion spawnRotation = rotation;
            bool alreadyCreated = false;

            if (m_vehicle)
            {
                spawnPosition = m_vehicle.transform.position;
                spawnRotation = m_vehicle.transform.rotation;
                alreadyCreated = true;

                m_vehicle.Deinitialize();
                m_vehicle.Destroy();
                GameObject.Destroy(m_vehicle.gameObject);
                m_vehicle = null;

                if (m_vehicleAlt)
                {
                    m_vehicleAlt.Deinitialize();
                    m_vehicleAlt.Destroy();
                    GameObject.Destroy(m_vehicleAlt.gameObject);
                    m_vehicleAlt = null;
                }
            }

            m_vehicle = InstanceVehicle(vehicleInfo);
            m_vehicle.Initialize(spawnPosition, spawnRotation, vehicleInfo, flags, variation, vehicleColor, setColor, true);

            if (vehicleInfoAlt)
            {
                m_vehicleAlt = InstanceVehicleAlt(vehicleInfoAlt);
                float offset = (vehicleInfo.m_generatedInfo.m_size.z + vehicleInfoAlt.m_generatedInfo.m_size.z) * 0.5f - (vehicleInfo.m_attachOffsetBack + vehicleInfoAlt.m_attachOffsetFront);
                m_vehicleAlt.Initialize(spawnPosition + spawnRotation * (offset * Vector3.back), spawnRotation, vehicleInfoAlt, flagsAlt, variationAlt, vehicleColor, setColor);

                SpringJoint joint = m_vehicle.gameObject.AddComponent<SpringJoint>();
                joint.axis = Vector3.up;
                joint.anchor = Vector3.forward * (vehicleInfo.m_attachOffsetBack - 0.5f * vehicleInfo.m_generatedInfo.m_size.z) + 0.5f * Vector3.up * vehicleInfo.m_generatedInfo.m_tyres[0].w;
                joint.autoConfigureConnectedAnchor = true;
                joint.breakForce = Mathf.Infinity;
                joint.breakTorque = Mathf.Infinity;
                joint.enableCollision = false;
                joint.spring = DriveCommon.LINKAGE_K;
                joint.damper = DriveCommon.LINKAGE_D;
                joint.connectedBody = m_vehicleAlt.GetRigidbody();
            }

            if (!alreadyCreated)
            {
                DriveCam.instance.EnableCam(m_vehicle.GetRigidbody(), m_vehicle.GetBoxCollider().size.z * 2.0f);
                DriveButton.instance.SetDisable();
            }
            else
            {
                DriveCam.instance.RetargetCam(m_vehicle.GetRigidbody(), m_vehicle.GetBoxCollider().size.z * 2.0f);
            }
        }

        public void StopDriving()
        {
            StartCoroutine(m_collidersManager.DisableColliders());
            DriveCam.instance?.DisableCam();
            DriveButton.instance?.SetEnable();
            if (m_vehicle)
            {
                m_vehicle.Deinitialize();
                m_vehicle.Destroy();
                GameObject.Destroy(m_vehicle.gameObject);
                m_vehicle = null;
            }
            if (m_vehicleAlt)
            {
                m_vehicleAlt.Deinitialize();
                m_vehicleAlt.Destroy();
                GameObject.Destroy(m_vehicleAlt.gameObject);
                m_vehicleAlt = null;
            }
            m_setColor = false;
            m_setColorAlt = false;
            m_vehicleColor = default;
            m_vehicleColorAlt = default;
            m_vehicleInfo = null;
            m_vehicleInfoAlt = null;
            enabled = false;
        }

        private void Awake()
        {
            if (instance)
            {
                Destroy(this);
                return;
            }
            instance = this;

            StartCoroutine(m_collidersManager.InitializeColliders());
            enabled = false;
        }
        private void Update()
        {

        }

        private void FixedUpdate()
        {
            if (m_vehicle)
            {
                m_collidersManager.UpdateColliders(m_vehicle.GetRigidbody().transform);
                m_collidersManager.UpdateGroundCollider(m_vehicle.GetBoxCollider(), m_vehicleAlt?.GetBoxCollider());
            }
        }

        private void OnDestroy()
        {
            StopDriving();
            m_collidersManager.DestroyColliders();
        }
    }
}