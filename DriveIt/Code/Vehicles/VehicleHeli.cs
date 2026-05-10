using AlgernonCommons;
using DriveIt.Settings;
using DriveIt.Utils;
using System.Text;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleHeli : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS = { 0.0f, 1.0f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "N", "D" };
        private const int ENGINE_GEAR_NEUTRAL = 0;
        private const float ENGINE_PEAK_RPS = 300.0f;
        private const float ENGINE_OVER_RPS = 315.0f;
        private const float ENGINE_IDLE_RPS = 3.0f;
        private const float ENGINE_INERTIA = 0.9f;
        private const float PITCH_RESP = 1.5f;
        private const float PITCH_REST = 1.5f;
        private const float DRAG_FACTOR_ROT = 4.0f;
        private const float DRAG_FACTOR = 0.5f;
        private const float STEER_MAX = 0.0f;
        private const float GEAR_RESP = 0.1f;
        private const float MIN_POWER_VEL = 1.0f;
        private const float COEFF_ROT = 52.0f;
        private const float COEFF_STAB = 1.0f;
        private const float STAB_COMPV = 0.75f;
        private const float STAB_COMPH = 0.1f;
        private const float AIR_DENSITY_SEA = 1.225f;
        private const float AIR_DENSITY_DECAY = -0.00011856f;
        private const float GRAVITY_STD = 10.0f;
        private const float MASS_FACTOR = 5.0f;
        private const float MASS_BIAS = 1500.0f;

        private static bool s_first_create = true;
        private static float s_engine_inertia;

        private float m_wingLever = 0.0f;
        private float m_tailLever = 0.0f;
        private float m_rudderLever = 0.0f;
        private float m_rollCoeff = 0.0f;
        private float m_pitchCoeff = 0.0f;
        private float m_yawCoeff = 0.0f;
        private float m_vstabCoeff = 0.0f;
        private float m_hstabCoeff = 0.0f;
        private float m_pitch = 0.0f;
        private float m_flying = 0.0f;

        protected override float enginePower { get => ModSettings.HeliEnginePower; }
        protected override float brakingForce { get => 500.0f; }
        protected override float downForce { get => 0.0f; }
        protected override float driveBias { get => 0.5f; }
        protected override float brakeBias { get => 0.5f; }
        protected override float springDamp { get => ModSettings.HeliSpringDamp; }
        protected override float springOffset { get => ModSettings.HeliSpringOffset; }
        protected override float springSwayBar { get => 0.0f; }
        protected override float massCenterHeight { get => ModSettings.HeliMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.HeliMassCenterBias; }
        protected override float steerMax { get => STEER_MAX; }
        protected override float linearDrag { get => DRAG_FACTOR; }
        protected override float angularDrag { get => DRAG_FACTOR_ROT; }
        protected override float enginePeakRPS { get => ENGINE_PEAK_RPS; }
        protected override float engineOverRPS { get => ENGINE_OVER_RPS; }
        protected override float engineIdleRPS { get => ENGINE_IDLE_RPS; }
        protected override float massFactor { get => MASS_FACTOR; }
        protected override float massBias { get => MASS_BIAS; }

        protected override void AwakeExt()
        {
            if (s_first_create)
            {
                s_first_create = false;
                s_engine_inertia = (float)System.Math.Pow(ENGINE_INERTIA, Time.fixedDeltaTime);
            }
        }

        protected override void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref float groundY, ref RigidbodyConstraints constraints)
        {
            /* bound assumptions:
             * - contact height at min of lowest wheel and springOffset height
             * - ride height fixed rideHeight from contact height
             * - ground at springOffset from contact height
             */
            float contactHeight = -springOffset;
            if (m_vehicleInfo.m_generatedInfo.m_tyres?.Length > 0)
            {
                foreach (Vector4 tirepos in m_vehicleInfo.m_generatedInfo.m_tyres)
                {
                    if (tirepos.y > 0.0f && tirepos.w <= 2.0f)
                    {
                        contactHeight = Mathf.Min(contactHeight, tirepos.y - tirepos.w);
                    }
                }
            }

            float height = contactHeight + rideHeight;
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
            m_wingLever = adjustedBounds.x * 0.5f;
            m_rudderLever = adjustedBounds.y * 0.5f;
            m_tailLever = adjustedBounds.z * 0.5f;
            float controlScale = enginePower / ModSettings.HELIENGINEPOWER;
            m_rollCoeff = controlScale * COEFF_ROT * adjustedBounds.x * adjustedBounds.z;
            m_pitchCoeff = controlScale * COEFF_ROT * adjustedBounds.x * adjustedBounds.z;
            m_yawCoeff = controlScale * COEFF_ROT * adjustedBounds.y * adjustedBounds.z;
            m_vstabCoeff = controlScale * COEFF_STAB * adjustedBounds.y * adjustedBounds.z;
            m_hstabCoeff = controlScale * COEFF_STAB * adjustedBounds.x * adjustedBounds.z;
            m_vehicleFlags &= ~(Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.TakingOff);
        }
        protected override void InitializeAdjust(ref float frontTorque, ref float rearTorque, ref float frontBraking, ref float rearBraking, ref float frontEBraking, ref float rearEBraking)
        {
            frontTorque = 0.0f;
            rearTorque = 0.0f;
        }

        protected override void PhysicsPreProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            if (m_effects.IsExtrasEnabled())
            {
                m_vehicleFlags |= Vehicle.Flags.Flying;
            }
            else
            {
                m_vehicleFlags &= ~Vehicle.Flags.Flying;
            }
        }

        protected override void PhysicsFeedbackWheelAndEngine(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            float radpsTrans = 0.0f;
            m_flying = 0.0f;

            foreach (Wheel w in m_wheelObjects)
            {
                // add to flying fract per wheel
                if (!w.isOnGround)
                {
                    m_flying += 1.0f / wheelCount;
                }

                // record distance travelled from previous tick
                m_distanceTravelled += w.wheelRadps * w.wheelRadius * Time.fixedDeltaTime / wheelCount;

                // apply wheel drag from previous tick
                w.ApplyDrag();

                radpsTrans += w.wheelRadps;
            }

            // distance travelled is engine rps instead
            m_distanceTravelled += m_flying * m_radps * Time.fixedDeltaTime;

            m_radpsTrans = radpsTrans / wheelCount;

            float engineRps = (m_throttle - m_brake) * m_gearRatios[m_gear] * enginePeakRPS;

            m_prevRadps = m_radps;
            m_radps = Mathf.Lerp(engineRps, m_radps, s_engine_inertia);
            m_radps = Mathf.Clamp(m_radps, engineIdleRPS, engineOverRPS);
        }

        protected override float GetTorque(float radps)
        {
            return enginePower * DriveCommon.KW_TO_W / enginePeakRPS;
        }

        // Function runs immediately after PhysicsFeedbackWheelAndEngine with auto transmissions. Selects a new gear based on engine state.
        protected override void PhysicsSelectGear(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            if (m_driveMode == ENGINE_MODE_REVERSE || m_driveMode == ENGINE_MODE_NEUTRAL)
            {
                m_driveMode = ENGINE_MODE_NEUTRAL;
                m_gear = m_gearNeutral;
            }
            else
            {
                if (m_gear != m_gearNeutral + 1 && Time.time > m_nextGearChange)
                {
                    m_gear = m_gearNeutral + 1;
                    m_nextGearChange = Time.time + GEAR_RESP;
                }
            }
        }

        protected override void PhysicsFeedForwardWheelAndEngine(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            foreach (Wheel w in m_wheelObjects)
            {
                float wheelTorque = 0.0f;

                // helis always braking
                wheelTorque -= Mathf.Sign(w.wheelRadps) * Mathf.Min(w.wheelBrakeForce * w.wheelRadius, Mathf.Abs(wheelTorque + w.wheelRadps * w.wheelMoment / Time.fixedDeltaTime));

                w.AddVelocity(wheelTorque * Time.fixedDeltaTime / w.wheelMoment);
            }

            Vector3 sideVec = Vector3.Cross(upVec, forwardVec).normalized;
            float densitySeaLevel = AIR_DENSITY_SEA * (ModSettings.Gravity / GRAVITY_STD);
            float density = densitySeaLevel * Mathf.Exp(AIR_DENSITY_DECAY * vehiclePos.y);
            float dir = 0.0f;
            float fc = Vector3.Dot(vehicleVel, forwardVec);
            float sc = Vector3.Dot(vehicleVel, sideVec);
            float sct = Vector3.Dot(m_vehicleRigidBody.GetPointVelocity(m_vehicleRigidBody.transform.TransformPoint(new Vector3(0.0f, m_rudderLever, -m_tailLever))), sideVec);
            float uc = Vector3.Dot(vehicleVel, upVec);

            Vector3 tmp = Vector3.zero;
            Vector3 netForce = Vector3.zero;
            Vector3 netStab = Vector3.zero;

            // vertical stabilizers
            dir = Mathf.Sign(sc);
            tmp = -dir * m_vstabCoeff * sc * sc * density * sideVec;
            netForce += (1.0f - STAB_COMPV) * tmp;
            dir = Mathf.Sign(sct);
            tmp = -dir * m_vstabCoeff * sct * sct * density * sideVec;
            netStab += STAB_COMPV * tmp;

            // horizontal stabilizers (wings)
            dir = Mathf.Sign(uc);
            tmp = -dir * m_hstabCoeff * uc * uc * density * upVec;
            netForce += (1.0f - STAB_COMPH) * tmp;
            netStab += STAB_COMPH * tmp;

            // engine thrust
            float thrust = GetPower(m_radps) * ENGINE_GEAR_RATIOS[m_gear] / Mathf.Max(uc, MIN_POWER_VEL) * (density / densitySeaLevel);
            float hoverScale = Vector3.Dot(upVec, Vector3.up);
            float hoverThrust = Mathf.Min(thrust, m_vehicleRigidBody.mass * ModSettings.Gravity / Vector3.Dot(upVec, Vector3.up) - netForce.y - netStab.y);
            netForce += Mathf.Lerp(thrust, hoverThrust, m_handbrake) * upVec;

            m_vehicleRigidBody.AddForceAtPosition(netForce, m_vehicleRigidBody.transform.TransformPoint(new Vector3(0.0f, 2.0f * m_rudderLever, m_vehicleRigidBody.centerOfMass.z)),ForceMode.Force);
            m_vehicleRigidBody.AddForceAtPosition(netStab, m_vehicleRigidBody.transform.TransformPoint(new Vector3(0.0f, m_rudderLever, -m_tailLever)), ForceMode.Force);

            Vector3 netTorque = Vector3.zero;



            // ailerons
            netTorque += -m_steer * m_rollCoeff * density * m_wingLever * forwardVec;

            // elevators
            netTorque += m_pitch * m_pitchCoeff * density * m_tailLever * sideVec;

            // tailrotor
            netTorque += m_handbrake * m_steer * m_yawCoeff * density * m_tailLever * upVec;

            m_vehicleRigidBody.AddTorque(netTorque);
        }

        protected override void PhysicsPostProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
        }

        protected override void HandleInputOnFixedUpdate(int invert)
        {
            invert = m_flying >= 1.0f - DriveCommon.FLOAT_ERROR ? 1 : Mathf.Abs(invert); 
            base.HandleInputOnFixedUpdate(invert);

            float pitch = Input.GetAxisRaw(DriveCommon.AXIS_PITCH);
            bool pitching = false;

            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyPitchDown.Key))
            {
                float factor = (m_pitch < 0.0f) ? PITCH_RESP + PITCH_REST : PITCH_RESP;
                m_pitch = Mathf.Clamp(m_pitch + Time.fixedDeltaTime * factor, -1.0f, 1.0f);
                pitching = true;
            }
            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyPitchUp.Key))
            {
                float factor = (m_pitch > 0.0f) ? PITCH_RESP + PITCH_REST : PITCH_RESP;
                m_pitch = Mathf.Clamp(m_pitch - Time.fixedDeltaTime * factor, -1.0f, 1.0f);
                pitching = true;
            }
            if (pitch != 0.0f)
            {
                pitching = true;
                m_pitch = pitch;
            }
            if (!pitching)
            {
                if (m_pitch > 0.0f)
                {
                    m_pitch = Mathf.Clamp(m_pitch - Time.fixedDeltaTime * PITCH_REST, 0.0f, 1.0f);
                }
                if (m_pitch < 0.0f)
                {
                    m_pitch = Mathf.Clamp(m_pitch + Time.fixedDeltaTime * PITCH_REST, -1.0f, 0.0f);
                }
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint && Logging.DetailLogging)
            {
                StringBuilder debugString = new StringBuilder();
                debugString.AppendFormat("v: {0}\n", this);
                debugString.AppendFormat("s: {0} p: {1}\n", m_steer, m_pitch);
                debugString.AppendFormat("m: {0} a: {1}\n", m_vehicleRigidBody.mass, m_vehicleRigidBody.transform.position.y);

                GUIStyle m_style = new GUIStyle(GUI.skin.label);
                m_style.fontSize = 20;
                m_style.normal.textColor = Color.white;

                GUI.Label(new Rect(100f, 100f + (m_isTrailer ? 350f : 0f), 700f, 350f), debugString.ToString(), m_style);
            }
        }
    }
}
