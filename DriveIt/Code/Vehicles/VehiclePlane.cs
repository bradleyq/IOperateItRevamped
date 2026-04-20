using DriveIt.Settings;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehiclePlane : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS = { -3.0f, 0.0f, 3.0f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R", "N", "F" };
        private const int ENGINE_GEAR_NEUTRAL = 1;

        protected override float enginePower { get => ModSettings.PlaneEnginePower; }
        protected override float brakingForce { get => ModSettings.PlaneBrakingForce; }
        protected override float downForce { get => ModSettings.PlaneDownForce; }
        protected override float driveBias { get => 0.5f; }
        protected override float brakeBias { get => ModSettings.PlaneBrakeBias; }
        protected override float springDamp { get => ModSettings.PlaneSpringDamp; }
        protected override float springOffset { get => ModSettings.PlaneSpringOffset; }
        protected override float springSwayBar { get => 0.0f; }
        protected override float massCenterHeight { get => ModSettings.PlaneMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.PlaneMassCenterBias; }

        protected override void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref RigidbodyConstraints constraints)
        {
            float height = Mathf.Max(m_vehicleInfo.m_generatedInfo.m_tyres[0].y, 0.0f);
            if (height > adjustedY)
            {
                adjustedBounds.y -= height - adjustedY;
            }
            adjustedY = height;

            foreach (Vector4 tirepos in m_vehicleInfo.m_generatedInfo.m_tyres)
            {
                m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(tirepos.x, tirepos.y + springOffset, tirepos.z), momentWheel, tirepos.w, false, tirepos.z > 0.0f));
            }

            m_gearRatios = ENGINE_GEAR_RATIOS;
            m_gearNames = ENGINE_GEAR_NAMES;
            m_gearNeutral = ENGINE_GEAR_NEUTRAL;
        }
    }
}
