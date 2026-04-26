using DriveIt.Settings;
using DriveIt.Utils;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleBike : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS = { -30.0f, 0.0f, 15.0f, 8.5f, 5.67f, 4.25f, 3.4f, 2.83f, 2.43f, 2.13f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R", "N", "1", "2", "3", "4", "5", "6", "7", "8" };
        private const int ENGINE_GEAR_NEUTRAL = 1;
        private const float STEER_TILT_SCALE = 0.5f;
        private const float STEER_MAX = 10.0f;
        private const float STAB_BOOST = 3.0f;

        private float m_tilt = 0.0f;
        private class TiltKalmanFilter
        {
            // Process noise covariance (how much we expect the true value to change)
            private static float q = 0.2f;

            // Measurement noise covariance (how noisy the measurements are)
            private static float r = 1000.0f;

            // Estimate error covariance
            private static float p = 1.0f;

            // Current estimate
            private static float x = 0.0f;

            // Kalman gain
            private static float k = 0.0f;

            public static float Update(float measurement)
            {
                p = p + q;
                k = p / (p + r);
                x = x + k * (measurement - x);
                p = (1f - k) * p;
                return x;
            }
            public static float CurrentEstimate => x;
        }

        protected override float enginePower { get => ModSettings.BikeEnginePower; }
        protected override float brakingForce { get => ModSettings.BikeBrakingForce; }
        protected override float downForce { get => ModSettings.BikeDownForce; }
        protected override float driveBias { get => ModSettings.BikeDriveBias; }
        protected override float brakeBias { get => ModSettings.BikeBrakeBias; }
        protected override float springDamp { get => ModSettings.BikeSpringDamp; }
        protected override float springOffset { get => ModSettings.BikeSpringOffset; }
        protected override float springSwayBar { get => 0.0f; }
        protected override float massCenterHeight { get => ModSettings.BikeMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.BikeMassCenterBias; }
        protected override float steerMax { get => STEER_MAX; }

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
                m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(tirepos.x, tirepos.y + springOffset, tirepos.z), momentWheel, tirepos.w, true, tirepos.z > 0.0f));
            }

            m_gearRatios = ENGINE_GEAR_RATIOS;
            m_gearNames = ENGINE_GEAR_NAMES;
            m_gearNeutral = ENGINE_GEAR_NEUTRAL;

            constraints = RigidbodyConstraints.FreezeRotationZ;
        }

        protected override void PhysicsFrictionCalculation(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            base.PhysicsFrictionCalculation(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);
            foreach (Wheel w in m_wheelObjects)
            {
                if (!w.isFront)
                {
                    w.SetFriction(w.wheelFrictionCoeffX * STAB_BOOST, w.wheelFrictionCoeffZ);
                }
            }
        }

        protected override void PhysicsAdjustSuspension(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {

        }

        protected override void PhysicsPostProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            Vector3 netNetImpulse = Vector3.zero;
            foreach (Wheel w in m_wheelObjects)
            {
                netNetImpulse += w.wheelNormalImpulse * w.wheelGroundNormal;
                netNetImpulse += w.wheelBinormalImpulse * w.wheelGroundBinormal;
                netNetImpulse += w.wheelTangentImpulse * w.wheelGroundTangent;
            }
            Vector3 sideVec = Vector3.Cross(m_vehicleRigidBody.transform.forward, Vector3.up).normalized;
            float tilt = Mathf.Atan(Vector3.Dot(netNetImpulse, sideVec) / (m_vehicleRigidBody.mass * ModSettings.Gravity * Time.fixedDeltaTime)) * 180.0f / Mathf.PI * STEER_TILT_SCALE;
            TiltKalmanFilter.Update(tilt);
            m_tilt = TiltKalmanFilter.CurrentEstimate;
            Quaternion rot = Quaternion.AngleAxis(TiltKalmanFilter.CurrentEstimate, m_vehicleRigidBody.transform.forward) * Quaternion.LookRotation(m_vehicleRigidBody.transform.forward);
            m_vehicleRigidBody.transform.rotation = rot;
        }


    }
}
