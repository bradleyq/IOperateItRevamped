using DriveIt.Settings;
using DriveIt.Utils;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehiclePlane : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS = { 0.1f, 0.0f, 1.0f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R", "N", "F" };
        private const int ENGINE_GEAR_NEUTRAL = 1;
        private const float DRAG_FACTOR = 0.75f;
        private const float ENGINE_PEAK_RPS = 300.0f;
        private const float ENGINE_OVER_RPS = 315.0f;
        private const float ENGINE_IDLE_RPS = 10.0f;
        private const float ENGINE_INERTIA = 0.8f;
        private const float GEAR_RESP = 0.1f;

        private static float s_engine_inertia;

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
        protected override float vehicleDrag { get => DRAG_FACTOR; }

        protected override void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref RigidbodyConstraints constraints,
            ref float frontTorque, ref float rearTorque, ref float frontBraking, ref float rearBraking, ref float frontEBraking, ref float rearEBraking)
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
            s_engine_inertia = (float)System.Math.Pow(ENGINE_INERTIA, Time.fixedDeltaTime);
        }

        protected override void PhysicsFeedbackWheelAndEngine(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            float radpsTrans = 0.0f;

            foreach (Wheel w in m_wheelObjects)
            {
                // record distance travelled from previous tick
                m_distanceTravelled += w.wheelRadps * w.wheelTorqueFract * w.wheelRadius * Time.fixedDeltaTime;

                // apply wheel drag from previous tick
                w.ApplyDrag();

                radpsTrans += w.wheelRadps;
            }

            m_radpsTrans = radpsTrans / wheelCount;

            float engineRps = m_throttle * m_gearRatios[m_gear] * ENGINE_PEAK_RPS;

            m_prevRadps = m_radps;
            engineRps = Mathf.Clamp(engineRps, ENGINE_IDLE_RPS, ENGINE_OVER_RPS);
            m_radps = Mathf.Lerp(engineRps, m_radps, s_engine_inertia);
        }

        protected override float GetTorque(float radps)
        {
            return Mathf.Max(radps, ENGINE_IDLE_RPS) * enginePower / ENGINE_PEAK_RPS;
        }

        // Function runs immediately after PhysicsFeedbackWheelAndEngine with auto transmissions. Selects a new gear based on engine state.
        protected override void PhysicsSelectGear(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            if (m_driveMode == ENGINE_MODE_FORWARD)
            {
                if (m_gear != m_gearNeutral + 1 && Time.time > m_nextGearChange)
                {
                    m_gear = m_gearNeutral + 1;
                    m_nextGearChange = Time.time + GEAR_RESP;
                }
            }
            else if (m_driveMode == ENGINE_MODE_REVERSE)
            {
                if (m_gear != m_gearNeutral - 1 && Time.time > m_nextGearChange)
                {
                    m_gear = m_gearNeutral - 1;
                    m_nextGearChange = Time.time + GEAR_RESP;
                }
            }
            else
            {
                m_gear = m_gearNeutral;
            }
        }

        protected override void PhysicsFeedForwardWheelAndEngine(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            foreach (Wheel w in m_wheelObjects)
            {
                float wheelTorque = 0.0f;

                // braking ABS
                float totalBrake = (w.wheelSlip < GRIP_OPTIM_SLIP * 0.75f || !ModSettings.BrakingABS || !w.isOnGround) ? m_brake : 0.0f;

                wheelTorque -= Mathf.Sign(w.wheelRadps) * Mathf.Min(totalBrake * w.wheelBrakeForce * w.wheelRadius, Mathf.Abs(wheelTorque + w.wheelRadps * w.wheelMoment / Time.fixedDeltaTime));

                // braking Handbrake
                float handBrake = m_handbrake;

                wheelTorque -= Mathf.Sign(w.wheelRadps) * Mathf.Min(handBrake * w.wheelHandbrakeForce * w.wheelRadius, Mathf.Abs(wheelTorque + w.wheelRadps * w.wheelMoment / Time.fixedDeltaTime));

                w.AddVelocity(wheelTorque * Time.fixedDeltaTime / w.wheelMoment);
            }
        }
    }
}
