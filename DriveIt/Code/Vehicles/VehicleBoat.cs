using DriveIt.Settings;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleBoat : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS = { -5.0f, -10.0f, 0.0f, 10.0f, 5.0f, 3.0f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R2", "R1", "N", "D1", "D2", "D3" };
        private const int ENGINE_GEAR_NEUTRAL = 2;
        private const float DRAG_FACTOR = 0.5f;
        private const float STAB_BOOST = 1.25f;

        protected override float enginePower { get => ModSettings.BoatEnginePower; }
        protected override float brakingForce { get => ModSettings.BoatBrakingForce; }
        protected override float downForce { get => ModSettings.BoatDownForce; }
        protected override float driveBias { get => ModSettings.BoatDriveBias; }
        protected override float brakeBias { get => ModSettings.BoatBrakeBias; }
        protected override float springDamp { get => 1.0f; }
        protected override float springOffset { get => ModSettings.BoatSpringOffset; }
        protected override float springSwayBar { get => 0.0f; }
        protected override float massCenterHeight { get => ModSettings.BoatMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.BoatMassCenterBias; }
        protected override float linearDrag { get => DRAG_FACTOR; }
        protected override float angularDrag { get => DRAG_FACTOR; }

        protected override void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref RigidbodyConstraints constraints)
        {
            if (0.0f > adjustedY)
            {
                adjustedBounds.y += adjustedY;
                adjustedY = 0.0f;
            }

            base.InitializeInternal(ref adjustedBounds, ref adjustedY, ref adjustedZ, ref constraints);

            adjustedBounds.y += springOffset;
            adjustedY -= springOffset;
            m_gearRatios = ENGINE_GEAR_RATIOS;
            m_gearNames = ENGINE_GEAR_NAMES;
            m_gearNeutral = ENGINE_GEAR_NEUTRAL;
        }

        protected override void InitializeAdjust(ref float frontTorque, ref float rearTorque, ref float frontBraking, ref float rearBraking, ref float frontEBraking, ref float rearEBraking)
        {
            frontEBraking = 0.0f;
            rearEBraking = 0.0f;
        }

        protected override void PhysicsFrictionCalculation(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            foreach (Wheel w in m_wheelObjects)
            {
                w.SetFriction(ModSettings.GripCoeffK * STAB_BOOST, ModSettings.GripCoeffK);
            }
        }

        protected override void PhysicsAdjustSuspension(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {

        }
    }
}
