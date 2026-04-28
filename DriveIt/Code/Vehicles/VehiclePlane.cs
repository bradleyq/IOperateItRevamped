using DriveIt.Settings;
using DriveIt.Utils;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehiclePlane : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS = { 0.05f, 0.0f, 1.0f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R", "N", "F" };
        private const int ENGINE_GEAR_NEUTRAL = 1;
        private const float DRAG_FACTOR = 0.75f;
        private const float ENGINE_PEAK_RPS = 300.0f;
        private const float ENGINE_OVER_RPS = 315.0f;
        private const float ENGINE_IDLE_RPS = 10.0f;
        private const float ENGINE_INERTIA = 0.8f;
        private const float GEAR_RESP = 0.1f;
        private const float MIN_POWER_VEL = 1.0f;
        private const float COEFF_LIFT = 0.07f;
        private const float COEFF_ROT = 0.0125f;
        private const float COEFF_STAB = 0.3f;
        private const float STAB_COMP = 0.25f;
        private const float AIR_DENSITY_SEA = 1.225f;
        private const float AIR_DENSITY_DECAY = -0.00011856f;
        private const float MASS_FACTOR = 20.0f;

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
        protected override float angularDrag { get => DRAG_FACTOR; }
        protected override float enginePeakRPS { get => ENGINE_PEAK_RPS; }
        protected override float engineOverRPS { get => ENGINE_OVER_RPS;  }
        protected override float engineIdleRPS {  get => ENGINE_IDLE_RPS; }
        protected override float massFactor { get => MASS_FACTOR; }

        private float m_wingLever = 0.0f;
        private float m_tailLever = 0.0f;
        private float m_rudderLever = 0.0f;
        private float m_liftCoeff = 0.0f;
        private float m_rollCoeff = 0.0f;
        private float m_pitchCoeff = 0.0f;
        private float m_vstabCoeff = 0.0f;
        private float m_hstabCoeff = 0.0f;
        private float m_pitch = 0.0f;

        protected override void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref RigidbodyConstraints constraints)
        {
            float height = Mathf.Max(m_vehicleInfo.m_generatedInfo.m_tyres[0].y, 0.0f);
            if (height > adjustedY)
            {
                adjustedBounds.y -= height - adjustedY;
            }
            adjustedY = height;

            base.InitializeInternal(ref adjustedBounds, ref adjustedY, ref adjustedZ, ref constraints);

            adjustedBounds.y += springOffset;
            adjustedY -= springOffset;
            m_gearRatios = ENGINE_GEAR_RATIOS;
            m_gearNames = ENGINE_GEAR_NAMES;
            m_gearNeutral = ENGINE_GEAR_NEUTRAL;
            s_engine_inertia = (float)System.Math.Pow(ENGINE_INERTIA, Time.fixedDeltaTime);
            m_wingLever = adjustedBounds.x * 0.5f;
            m_rudderLever = adjustedBounds.y * 0.5f;
            m_tailLever = adjustedBounds.z * 0.5f;
            m_liftCoeff = COEFF_LIFT * adjustedBounds.x * adjustedBounds.z;
            m_rollCoeff = COEFF_ROT * adjustedBounds.x * adjustedBounds.z;
            m_pitchCoeff = COEFF_ROT * adjustedBounds.x * adjustedBounds.z;
            m_vstabCoeff = COEFF_STAB * adjustedBounds.y * adjustedBounds.z;
            m_hstabCoeff = COEFF_STAB * adjustedBounds.x * adjustedBounds.z;
        }
        protected override void InitializeAdjust(ref float frontTorque, ref float rearTorque, ref float frontBraking, ref float rearBraking, ref float frontEBraking, ref float rearEBraking)
        {
            frontTorque = 0.0f;
            rearTorque = 0.0f;
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

            float engineRps = m_throttle * m_gearRatios[m_gear] * enginePeakRPS;

            m_prevRadps = m_radps;
            engineRps = Mathf.Clamp(engineRps, engineIdleRPS, engineOverRPS);
            m_radps = Mathf.Lerp(engineRps, m_radps, s_engine_inertia);
        }

        protected override float GetTorque(float radps)
        {
            return  enginePower * DriveCommon.KW_TO_W / enginePeakRPS;
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

            Vector3 sideVec = Vector3.Cross(upVec, forwardVec).normalized;
            float density = AIR_DENSITY_SEA * Mathf.Exp(AIR_DENSITY_DECAY * vehiclePos.y);
            float dir = 0.0f;
            float fc = Vector3.Dot(vehicleVel, forwardVec);
            float sc = Vector3.Dot(vehicleVel, sideVec);
            float uc = Vector3.Dot(vehicleVel, upVec);

            Vector3 tmp = Vector3.zero;
            Vector3 netForce = Vector3.zero;
            Vector3 netStab = Vector3.zero;

            // engine thrust
            dir = (m_gear - 1.0f) * (m_brake > 0.5f ? 0.0f : 1.0f);
            netForce += dir * GetPower(m_radps) / Mathf.Max(dir * fc, MIN_POWER_VEL) * (density / AIR_DENSITY_SEA) * forwardVec;

            // lift
            netForce += m_liftCoeff * fc * fc * density * upVec;

            // vertical stabilizers
            dir = Mathf.Sign(sc);
            tmp = -dir * m_vstabCoeff * sc * sc * density * sideVec;
            netForce += (1.0f - STAB_COMP) * tmp;
            netStab += STAB_COMP * tmp;

            // horizontal stabilizers (wings)
            dir = Mathf.Sign(uc);
            tmp = -dir * m_hstabCoeff * uc * uc * density * upVec;
            netForce += (1.0f - STAB_COMP) * tmp;
            netStab += STAB_COMP * tmp;

            m_vehicleRigidBody.AddForce(netForce, ForceMode.Force);
            m_vehicleRigidBody.AddForceAtPosition(netStab, m_vehicleRigidBody.transform.TransformPoint(new Vector3(0.0f, m_rudderLever, -m_tailLever)), ForceMode.Force);

            Vector3 netTorque = Vector3.zero;
            dir = Mathf.Sign(fc);

            // ailerons
            netTorque += -m_steer * dir * m_rollCoeff * fc * fc * density * m_wingLever * forwardVec;

            // elevators
            netTorque += m_pitch * dir * m_pitchCoeff * fc * fc * density * m_tailLever * sideVec;

            m_vehicleRigidBody.AddTorque(netTorque);
        }

        protected override void PhysicsPostProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {

        }

        protected override void HandleInputOnFixedUpdate(int invert)
        {
            base.HandleInputOnFixedUpdate(invert);

            float pitch = Input.GetAxisRaw(DriveCommon.AXIS_PITCH);
            m_pitch = pitch;
        }
    }
}
