using AlgernonCommons;
using DriveIt.Settings;
using System.Text;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleTrailer : VehicleGeneric
    {
        private static readonly float[] ENGINE_GEAR_RATIOS = { 0.0f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "" };
        private const int ENGINE_GEAR_NEUTRAL = 0;
        private const float RADIUS_WHEEL = 0.2f;
        private const float RPS_INERTIA = 0.05f;

        private static bool s_first_create = true;
        private static float s_engine_inertia;

        private bool m_constrained = false;
        protected override float enginePower { get => 0.0f; }
        protected override float brakingForce { get => ModSettings.TrailerBrakingForce; }
        protected override float downForce { get => ModSettings.TrailerDownForce; }
        protected override float driveBias { get => 0.0f; }
        protected override float brakeBias { get => 0.25f; }
        protected override float springDamp { get => ModSettings.TrailerSpringDamp; }
        protected override float springOffset { get => ModSettings.TrailerSpringOffset; }
        protected override float springSwayBar { get => ModSettings.TrailerSpringSwayBar; }
        protected override float massCenterHeight { get => ModSettings.TrailerMassCenterHeight; }
        protected override float massCenterBias { get => ModSettings.TrailerMassCenterBias; }
        protected override void AwakeExt()
        {
            if (s_first_create)
            {
                s_first_create = false; 
                s_engine_inertia = (float)System.Math.Pow(RPS_INERTIA, Time.fixedDeltaTime);
            }
        }

        protected override void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref float groundY, ref RigidbodyConstraints constraints)
        {
            bool validWheels = true;
            if (m_vehicleInfo.m_generatedInfo.m_tyres?.Length > 0)
            {
                foreach (Vector4 tirepos in m_vehicleInfo.m_generatedInfo.m_tyres)
                {
                    if (tirepos.y <= 0.0f)
                    {
                        validWheels = false;
                    }
                }
            }

            if (validWheels)
            {
                float height = Mathf.Max(m_vehicleInfo.m_generatedInfo.m_tyres[0].y, 0.0f);
                if (height > adjustedY)
                {
                    adjustedBounds.y -= height - adjustedY;
                }
                adjustedY = height;

                foreach (Vector4 tirepos in m_vehicleInfo.m_generatedInfo.m_tyres)
                {
                    m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(tirepos.x, tirepos.y + springOffset, tirepos.z), momentWheel, tirepos.w, false, false));
                }
            }
            else
            {
                if (0.0f > adjustedY)
                {
                    adjustedBounds.y += adjustedY;
                }
                adjustedY = 0.0f;

                float width = adjustedBounds.x;
                float length = adjustedBounds.z;
                m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(width * 0.5f, springOffset + RADIUS_WHEEL, -length * 0.5f), momentWheel, RADIUS_WHEEL, false, false));
                m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(-width * 0.5f, springOffset + RADIUS_WHEEL, -length * 0.5f), momentWheel, RADIUS_WHEEL, false, false));
            }


            m_gearRatios = ENGINE_GEAR_RATIOS;
            m_gearNames = ENGINE_GEAR_NAMES;
            m_gearNeutral = ENGINE_GEAR_NEUTRAL;
        }

        protected override void InitializeAdjust(ref float frontTorque, ref float rearTorque, ref float frontBraking, ref float rearBraking, ref float frontEBraking, ref float rearEBraking)
        {
            frontTorque = 0.0f;
            rearTorque = 0.0f;
            frontEBraking = 0.5f;
            rearEBraking = 0.5f;
        }

        protected override void PhysicsFeedbackWheelAndEngine(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            float radpsTrans = 0.0f;

            foreach (Wheel w in m_wheelObjects)
            {
                // record distance travelled from previous tick
                m_distanceTravelled += w.wheelRadps * w.wheelRadius * Time.fixedDeltaTime / wheelCount;

                // apply wheel drag from previous tick
                w.ApplyDrag();

                radpsTrans += w.wheelRadps;
            }

            m_radpsTrans = radpsTrans / wheelCount;

            m_prevRadps = m_radps;
            radpsTrans = Mathf.Clamp(radpsTrans, 0.0f, engineOverRPS);
            m_radps = Mathf.Lerp(radpsTrans, m_radps, s_engine_inertia);
        }

        protected override void PhysicsFrictionCalculation(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isFront)
                {
                    w.SetFriction(0.0f, ModSettings.GripCoeffS);
                }
                else
                {
                    w.SetFriction(ModSettings.GripCoeffS, ModSettings.GripCoeffS);
                }
            }
        }

        protected override void PhysicsSelectGear(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {

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

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint && Logging.DetailLogging)
            {
                StringBuilder debugString = new StringBuilder();
                debugString.AppendFormat("v: {0}\n", this);
                debugString.AppendFormat("b: {1} hb: {2}\n", m_throttle.ToString("0.000"), m_brake.ToString("0.000"), m_handbrake.ToString("0.000"));
                debugString.AppendFormat("st: {0}\n", m_steer.ToString("0.000"));
                debugString.AppendFormat("rps: {0} rpst: {1}\n", m_radps.ToString("0.000"), m_radpsTrans.ToString("0.000"));
                debugString.AppendFormat("m: {0} wct: {1} rwct: {2} fwct: {3}\n", m_vehicleRigidBody.mass, wheelCount, rightCount, frontCount);
                for (int index = 0; index < m_wheelObjects.Count; index++)
                {
                    Wheel w = m_wheelObjects[index];
                    debugString.AppendFormat("w[{0}] o:{1} g:{2} s:{3} rps:{4}\n", index, w.wheelOrigin, w.isOnGround, w.wheelSlip.ToString("0.000"), w.wheelRadps.ToString("0.000"));
                }

                GUIStyle m_style = new GUIStyle(GUI.skin.label);
                m_style.fontSize = 20;
                m_style.normal.textColor = Color.white;

                GUI.Label(new Rect(100f, 450f, 700f, 350f), debugString.ToString(), m_style);
            }
        }
    }
}
