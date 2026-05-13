using DriveIt.Settings;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleTrain : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS = { -1.9f, -2.53f, -3.38f, -4.5f, -6.0f, -8.0f, -12.0f, -24.0f, 0.0f, 24.0f, 12.0f, 8.0f, 6.0f, 4.5f, 3.38f, 2.53f, 1.9f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R8", "R7", "R6", "R5", "R4", "R3", "R2", "R1", "N", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8" };
        private const int ENGINE_GEAR_NEUTRAL = 8;
        private const float STAB_BOOST = 2.0f;
        private const float STEER_MAX = 10.0f;

        private bool m_constrained = false;

        protected override float enginePower { get => ModSettings.TrainEnginePower; }
        protected override float brakingForce { get => ModSettings.TrainBrakingForce; }
        protected override float downForce { get => ModSettings.TrainDownForce; }
        protected override float driveBias { get => ModSettings.TrainDriveBias; }
        protected override float brakeBias { get => ModSettings.TrainBrakeBias; }
        protected override float springDamp { get => ModSettings.TrainSpringDamp; }
        protected override float springOffset { get => ModSettings.TrainSpringOffset; }
        protected override float springSwayBar { get => ModSettings.TrainSpringSwayBar; }
        protected override float massCenterHeight { get => ModSettings.TrainMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.TrainMassCenterBias; }
        protected virtual float steerMax { get => STEER_MAX; }

        protected override void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref float groundY, ref RigidbodyConstraints constraints)
        {
            /* bound assumptions:
             * - contact height at min of lowest wheel and springOffset height
             * - ride height max of m_tyres and fixed rideHeight from contact height
             * - ground at springOffset from contact height
             */
            float contactHeight = -springOffset;
            float wheelRideHeight = -1000.0f;
            if (m_vehicleInfo.m_generatedInfo.m_tyres?.Length > 0)
            {
                foreach (Vector4 tirepos in m_vehicleInfo.m_generatedInfo.m_tyres)
                {
                    if (tirepos.y > 0.0f && tirepos.w <= 2.0f)
                    {
                        contactHeight = Mathf.Min(contactHeight, tirepos.y - tirepos.w);
                        wheelRideHeight = Mathf.Max(wheelRideHeight, tirepos.y);
                    }
                }
            }

            float height = Mathf.Max(contactHeight + rideHeight, wheelRideHeight);
            if (adjustedY < height)
            {
                adjustedBounds.y += adjustedY - height;
                adjustedY = height;
            }
            groundY = contactHeight + springOffset;

            base.InitializeInternal(ref adjustedBounds, ref adjustedY, ref adjustedZ, ref groundY, ref constraints);

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
            base.PhysicsFrictionCalculation(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);
            foreach (Wheel w in m_wheelObjects)
            {
                w.SetFriction(w.wheelFrictionCoeffX * STAB_BOOST, w.wheelFrictionCoeffZ);
            }
        }
        protected override void PhysicsPreProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            bool slipping = false;

            if (m_constrained)
            {
                m_vehicleRigidBody.constraints = RigidbodyConstraints.None;
            }

            foreach (Wheel w in m_wheelObjects)
            {
                slipping |= w.wheelHighSlip > 0.0f;
            }

            if (vehicleVel.magnitude < 3.0f && m_throttle == 0.0f && m_brake > 0.0f && !slipping)
            {
                Vector3 sideVec = Vector3.Cross(forwardVec, upVec).normalized;
                m_vehicleRigidBody.velocity = m_vehicleRigidBody.velocity - Vector3.Dot(m_vehicleRigidBody.velocity, sideVec) * sideVec;

                if (vehicleVel.magnitude < parkSpeed)
                {
                    m_vehicleRigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
                    m_constrained = true;
                }
            }
        }
    }
}
