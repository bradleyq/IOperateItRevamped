using DriveIt.Settings;
using DriveIt.Utils;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleCar : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS_L = { -15.0f, 0.0f, 15.0f, 8.5f, 5.67f, 4.25f, 3.4f, 2.83f, 2.43f, 2.13f };
        private static readonly float[] ENGINE_GEAR_RATIOS_S = { -33.0f, 0.0f, 33.0f, 22.5f, 16.88f, 12.66f, 10.13f, 8.44f, 7.23f, 6.33f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R", "N", "1", "2", "3", "4", "5", "6", "7", "8" };
        private const int ENGINE_GEAR_NEUTRAL = 1;
        private const float STEER_MAX = 37.0f;
        private const float STAB_BOOST_HI = 0.5f;
        private const float STAB_BOOST_MID = 0.3f;
        private const float STAB_BOOST_LO = 0.1f;

        private bool m_constrained = false;
        protected override float enginePower { get => ModSettings.CarEnginePower; }
        protected override float brakingForce { get => ModSettings.CarBrakingForce; }
        protected override float downForce { get => ModSettings.CarDownForce; }
        protected override float driveBias { get => ModSettings.CarDriveBias; }
        protected override float brakeBias { get => ModSettings.CarBrakeBias; }
        protected override float springDamp { get => ModSettings.CarSpringDamp; }
        protected override float springOffset { get => ModSettings.CarSpringOffset; }
        protected override float springSwayBar { get => ModSettings.CarSpringSwayBar; }
        protected override float massCenterHeight { get => ModSettings.CarMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.CarMassCenterBias; }
        protected override float steerMax { get => STEER_MAX; }

        protected override void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref RigidbodyConstraints constraints)
        {
            int gearCount = ENGINE_GEAR_RATIOS_L.Length;
            float avgRadius = 0.0f;
            float height = Mathf.Max(m_vehicleInfo.m_generatedInfo.m_tyres[0].y, 0.0f);
            if (height > adjustedY)
            {
                adjustedBounds.y -= height - adjustedY;
            }
            adjustedY = height;

            foreach (Vector4 tirepos in m_vehicleInfo.m_generatedInfo.m_tyres)
            {
                m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(tirepos.x, tirepos.y + springOffset, tirepos.z), momentWheel, tirepos.w, true, tirepos.z > 0.0f));
                avgRadius += tirepos.w;
            }

            avgRadius /= wheelCount;
            m_gearRatios = new float[gearCount];
            m_gearNames = ENGINE_GEAR_NAMES;
            m_gearNeutral = ENGINE_GEAR_NEUTRAL;

            for (int gear = 0; gear < gearCount; gear++)
            {
                m_gearRatios[gear] = Mathf.Lerp(ENGINE_GEAR_RATIOS_L[gear], ENGINE_GEAR_RATIOS_S[gear], Mathf.Clamp01((avgRadius - 0.2f) / 0.7f));
            }
        }

        protected override void PhysicsFrictionCalculation(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            base.PhysicsFrictionCalculation(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);
            foreach (Wheel w in m_wheelObjects)
            {
                if (!w.isFront)
                {
                    // lateral friction boost when exitting drifts
                    float boost = 0.0f;
                    switch (ModSettings.TCSLevel)
                    {
                        case (int)DriveCommon.TRACTIONCTL_LEVEL.FULL:
                            boost = STAB_BOOST_HI;
                            break;
                        case (int)DriveCommon.TRACTIONCTL_LEVEL.SPORT:
                            boost = STAB_BOOST_MID;
                            break;
                        case (int)DriveCommon.TRACTIONCTL_LEVEL.TRACK:
                            boost = STAB_BOOST_LO;
                            break;
                        case (int)DriveCommon.TRACTIONCTL_LEVEL.OFF:
                        default:
                            break;
                    }
                    boost = boost * (1.0f - m_throttle) * (1.0f - m_handbrake) + 1.0f;
                    w.SetFriction(w.wheelFrictionCoeffX * boost, w.wheelFrictionCoeffZ);
                }
            }
        }

        protected override void PhysicsPreProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            bool slipping = false;

            if (m_constrained)
            {
                m_vehicleRigidBody.constraints = RigidbodyConstraints.None;
            }

            foreach (Wheel w in m_wheelObjects) {
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
