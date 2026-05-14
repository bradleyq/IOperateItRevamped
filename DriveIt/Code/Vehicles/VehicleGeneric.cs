using AlgernonCommons;
using ColossalFramework;
using DriveIt.Settings;
using DriveIt.Utils;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleGeneric : MonoBehaviour
    {
        private static readonly float[] ENGINE_GEAR_RATIOS_T = { 0.0f };
        private static readonly string[] ENGINE_GEAR_NAMES_T = { "" };
        private const int ENGINE_GEAR_NEUTRAL_T = 0;
        private static readonly float[] ENGINE_GEAR_RATIOS = { -5.0f, -10.0f, 0.0f, 10.0f, 5.0f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R2", "R1", "N", "D1", "D2" };
        private const int ENGINE_GEAR_NEUTRAL = 2;
        private const float RESET_SCAN_HEIGHT = 2.0f;
        private const float RESET_HEIGHT = 0.25f;
        private const float RESET_FREQ = 2.0f;
        private const float MASS_FACTOR = 85.0f;
        private const float MASS_FACTOR_TRAILER = 40.0f;
        private const float MASS_BIAS = 0.0f;
        private const float RIDE_HEIGHT = 0.2f;
        private const float THROTTLE_RESP = 2.0f;
        private const float THROTTLE_REST = 2.0f;
        private const float STEER_RESP = 1.5f;
        private const float STEER_REST = 1.5f;
        private const float STEER_MAX = 15.0f;
        private const float STEER_DECAY = 0.0075f;
        private const float DEPEN_VELOCITY = 2.0f;
        private const float GEAR_RESP = 0.2f;
        private const float GEAR_RESP_AUTO = 1.0f;
        private const float PARK_SPEED = 0.25f;
        private const float DRAG_FACTOR = 0.25f;
        private const float DRAG_DRIVETRAIN = 0.15f;
        private const float RADIUS_WHEEL = 0.2f;
        private const float DRAG_WHEEL_POWERED = 0.25f;
        private const float DRAG_WHEEL = 0.15f;
        private const float MOMENT_WHEEL = 1.5f;
        private const float DIFF_LSD_FACTOR_LOW = 0.05f;
        private const float DIFF_LSD_FACTOR_HIGH = 0.25f;
        private const float ENGINE_PEAK_RPS = 900.0f;
        private const float ENGINE_OVER_RPS = 1100.0f;
        private const float ENGINE_IDLE_RPS = 90.0f;
        private const float ENGINE_INERTIA = 0.01f;
        private const float ENGINE_AUTO_SHIFT_THRESH = 0.1f;

        private static bool s_first_create = true;
        private static float s_engine_inertia;
        private static float s_drag_wheel_powered;
        private static float s_drag_wheel;
        private static VehicleGeneric s_primaryVehicle = null;

        protected const int ENGINE_MODE_REVERSE = -1;
        protected const int ENGINE_MODE_NEUTRAL = 0;
        protected const int ENGINE_MODE_FORWARD = 1;

        protected DriveEffects m_effects;
        protected Rigidbody m_vehicleRigidBody;
        protected BoxCollider m_vehicleCollider;
        protected VehicleInfo m_vehicleInfo;
        protected Vehicle.Flags m_vehicleFlags;
        protected Color m_vehicleColor;
        protected uint m_variationMaskInv;
        protected Vector3 m_prevPosition;
        protected Vector3 m_prevVelocity;
        protected Vector3 m_prevPrevVelocity;
        protected bool m_isTurning = false;
        protected bool m_isTrailer = false;
        protected int m_gear = 0;
        protected int m_gearNeutral = ENGINE_GEAR_NEUTRAL;
        protected float[] m_gearRatios = ENGINE_GEAR_RATIOS;
        protected string[] m_gearNames = ENGINE_GEAR_NAMES;
        protected int m_driveMode = ENGINE_MODE_NEUTRAL;
        protected int m_wheelCount = 0;
        protected int m_frontWheels = 0;
        protected int m_rightWheels = 0;
        protected float m_lastReset = 0.0f;
        protected float m_distanceTravelled = 0.0f;
        protected float m_steer = 0.0f;
        protected float m_brake = 0.0f;
        protected float m_handbrake = 0.0f;
        protected float m_throttle = 0.0f;
        protected float m_nextGearChange = 0.0f;
        protected float m_radps = 0.0f;
        protected float m_prevRadps = 0.0f;
        protected float m_radpsTrans = 0.0f;
        protected float m_groundY = 0.0f;
        protected List<Wheel> m_wheelObjects = new List<Wheel>();

        public float odometer { get => m_distanceTravelled; }
        public float speedometer { get => m_vehicleRigidBody.velocity.magnitude * DriveCommon.MS_TO_KMPH / ModSettings.MaxVelocity; }
        public float tachometer { get => m_radps / enginePeakRPS; }
        public float steer { get => m_steer * steerMax; }
        public float brake { get => m_brake; }
        public float handbrake { get => m_handbrake; }
        public float throttle { get => m_throttle; }
        public string gear { get => m_gearNames[m_gear]; }
        public float speed { get => m_vehicleRigidBody.velocity.magnitude; }
        public Vector3 velocity { get => m_vehicleRigidBody.velocity; }
        public Vector3 acceleration { get => (m_prevVelocity - m_prevPrevVelocity) / Time.fixedDeltaTime; }
        public float rpm { get => m_radps * DriveCommon.RPS_TO_RPM; }
        public float radps { get => m_radps; }
        public float angularAcceleration { get => (m_radps - m_prevRadps) / Time.fixedDeltaTime; }
        public List<Wheel> wheels { get => m_wheelObjects; }
        public Vehicle.Flags vehicleFlags { get => m_vehicleFlags; }
        public uint variationMaskInv { get => m_variationMaskInv; }
        public int wheelCount { get => m_wheelCount; }
        public int frontCount { get => m_frontWheels; }
        public int rearCount { get => m_wheelCount - m_frontWheels; }
        public int rightCount { get => m_rightWheels; }
        public int leftCount { get => m_wheelCount - m_rightWheels; }
        public bool inlineWheels { get => rightCount == 0 || leftCount == 0; }
        public bool parallelWheels { get => rearCount == 0 || frontCount == 0; }
        public float springHeight { get => springOffset; }
        public float springStiffness { get => springDamp; }
        
        public enum VehicleType
        {
            Generic = 0,
            Bike = 1,
            Boat = 2,
            Car = 3,
            Heli = 4,
            Plane = 5,
            Trailer = 6,
            Train = 7,
        }

        public bool IsPrimary() { return this == s_primaryVehicle; }

        public bool IsTrailer() { return m_isTrailer; }

        public bool IsNeutral() { return m_gear == m_gearNeutral; }

        public static VehicleGeneric InstanceVehicle(VehicleInfo info)
        {
            GameObject vgo = new GameObject("VehicleObject");
            VehicleGeneric vehicle;

            int fronts = 0;
            int rears = 0;
            if (info.m_generatedInfo.m_tyres?.Length > 0)
            {
                foreach (Vector4 tirepos in info.m_generatedInfo.m_tyres)
                {
                    if (tirepos.z > 0.0f)
                    {
                        fronts += 1;
                    }
                    else
                    {
                        rears += 1;
                    }
                }
            }
            
            if (
                info.m_vehicleType == VehicleInfo.VehicleType.Plane)
            {
                vehicle = vgo.AddComponent<VehiclePlane>();
            }
            else if (
                info.m_vehicleType == VehicleInfo.VehicleType.Ferry ||
                info.m_vehicleType == VehicleInfo.VehicleType.Ship)
            {
                vehicle = vgo.AddComponent<VehicleBoat>();
            }
            else if (
                info.m_vehicleType == VehicleInfo.VehicleType.Balloon ||
                info.m_vehicleType == VehicleInfo.VehicleType.Helicopter ||
                info.m_vehicleType == VehicleInfo.VehicleType.Blimp ||
                info.m_vehicleType == VehicleInfo.VehicleType.Rocket)
            {
                vehicle = vgo.AddComponent<VehicleHeli>();
            }
            else if (
                info.m_vehicleType == VehicleInfo.VehicleType.Train ||
                info.m_vehicleType == VehicleInfo.VehicleType.Monorail ||
                info.m_vehicleType == VehicleInfo.VehicleType.Metro ||
                info.m_vehicleType == VehicleInfo.VehicleType.Tram)
            {
                vehicle = vgo.AddComponent<VehicleTrain>();
            }
            else if (
                info.m_vehicleType == VehicleInfo.VehicleType.Bicycle ||
                (info.m_vehicleType == VehicleInfo.VehicleType.Car && fronts == 1 && rears == 1))
            {
                vehicle = vgo.AddComponent<VehicleBike>();
            }
            else if (fronts + rears > 2 &&
                (info.m_vehicleType == VehicleInfo.VehicleType.Car ||
                info.m_vehicleType == VehicleInfo.VehicleType.Trolleybus))
            {
                vehicle = vgo.AddComponent<VehicleCar>();
            }
            else
            {
                vehicle = vgo.AddComponent<VehicleGeneric>();
            }

            vehicle.m_vehicleInfo = info;

            return vehicle;
        }

        public Rigidbody GetRigidbody()
        {
            return m_vehicleRigidBody;
        }

        public BoxCollider GetBoxCollider()
        {
            return m_vehicleCollider;
        }

        public void Initialize(Vector3 position, Quaternion rotation, VehicleInfo vehicleInfo, Vehicle.Flags vehicleFlags, int variation, Color vehicleColor, bool setColor, bool setPrimary = false)
        {
            if (setPrimary) s_primaryVehicle = this;

            m_isTrailer = !setPrimary;
            m_vehicleColor = vehicleColor;
            m_vehicleColor.a = 0; // Make sure blinking is not set.
            m_variationMaskInv = ~(1u << variation);
            m_prevPosition = position;
            m_prevVelocity = Vector3.zero;
            m_prevPrevVelocity = Vector3.zero;
            m_vehicleInfo = vehicleInfo;
            m_vehicleFlags = vehicleFlags | Vehicle.Flags.Created;
            m_driveMode = ENGINE_MODE_NEUTRAL;
            m_distanceTravelled = 0.0f;
            m_steer = 0.0f;
            m_brake = 0.0f;
            m_handbrake = 0.0f;
            m_throttle = 0.0f;
            m_nextGearChange = 0.0f;

            Mesh vehicleMesh = m_vehicleInfo.m_mesh;
            RigidbodyConstraints constraints = RigidbodyConstraints.None;
            Vector3 adjustedBounds = m_vehicleInfo.m_lodMesh.bounds.size;
            float adjustedY = Mathf.Min(m_vehicleInfo.m_mesh.bounds.min.y, m_vehicleInfo.m_lodMesh.bounds.min.y);
            float adjustedZ = m_vehicleInfo.m_lodMesh.bounds.min.z;
            float groundY = adjustedY + springHeight;
            float frontTorque = 0.0f;
            float rearTorque = 0.0f;
            float frontBraking = 0.0f;
            float rearBraking = 0.0f;
            float frontEBraking = 0.0f;
            float rearEBraking = 0.0f;

            adjustedBounds.y = m_vehicleInfo.m_lodMesh.bounds.max.y - adjustedY;

            InitializeInternal(ref adjustedBounds, ref adjustedY, ref adjustedZ, ref groundY, ref constraints);
            
            if (rearCount == 0 && frontCount > 0)
            {
                frontTorque = 1.0f / frontCount;
                frontBraking = brakingForce * DriveCommon.KN_TO_N / frontCount;
                frontEBraking = frontBraking;
            }
            else if (frontCount == 0 && rearCount > 0)
            {
                rearTorque = 1.0f / rearCount;
                rearBraking = brakingForce * DriveCommon.KN_TO_N / rearCount;
                rearEBraking = rearBraking;
            }
            else if (frontCount > 0 && rearCount > 0)
            {
                frontTorque = driveBias / frontCount;
                rearTorque = (1.0f - driveBias) / rearCount;
                frontBraking = brakeBias * brakingForce * DriveCommon.KN_TO_N / frontCount;
                rearBraking = (1.0f - brakeBias) * brakingForce * DriveCommon.KN_TO_N / rearCount;
                frontEBraking = 0.0f;
                rearEBraking = brakingForce * DriveCommon.KN_TO_N / rearCount;
            }

            if (m_isTrailer)
            {
                m_gearRatios = ENGINE_GEAR_RATIOS_T;
                m_gearNames = ENGINE_GEAR_NAMES_T;
                m_gearNeutral = ENGINE_GEAR_NEUTRAL_T;
                frontTorque = 0.0f;
                rearTorque = 0.0f;
                frontBraking = 0.0f;
                rearBraking = 1.0f;
                frontEBraking = 0.0f;
                rearEBraking = 1.0f;
            }

            InitializeAdjust(ref frontTorque, ref rearTorque, ref frontBraking, ref rearBraking, ref frontEBraking, ref rearEBraking);

            m_groundY = Mathf.Min(groundY, adjustedY);
            m_gear = m_gearNeutral;

            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isFront)
                {
                    w.AdjustWheel(frontTorque, frontBraking, frontEBraking, frontTorque > 0.0f ? s_drag_wheel_powered : s_drag_wheel);
                }
                else
                {
                    w.AdjustWheel(rearTorque, rearBraking, rearEBraking, rearTorque > 0.0f ? s_drag_wheel_powered : s_drag_wheel);
                }
            }

            float halfSA = (adjustedBounds.x * adjustedBounds.y + adjustedBounds.x * adjustedBounds.z + adjustedBounds.y * adjustedBounds.z);
            float massFactorLocal = massFactor;
            float massBiasLocal = massBias;
            float massCenterHeightLocal = massCenterHeight;
            float massCenterBiasLocal = massCenterBias;
            if (m_isTrailer)
            {
                massFactorLocal = massFactorTrailer;
                massBiasLocal = massBiasTrailer;
                massCenterHeightLocal = massCenterHeightTrailer;
                massCenterBiasLocal = massCenterBiasTrailer;
            }

            m_vehicleRigidBody.drag = linearDrag * adjustedBounds.x * adjustedBounds.y / halfSA;
            m_vehicleRigidBody.angularDrag = angularDrag * adjustedBounds.y * adjustedBounds.z / halfSA;
            m_vehicleRigidBody.mass = halfSA * massFactorLocal + massBiasLocal;
            m_vehicleRigidBody.transform.position = position + Vector3.down * m_groundY;
            m_vehicleRigidBody.transform.rotation = rotation;
            m_vehicleRigidBody.centerOfMass = new Vector3(0.0f, adjustedY + adjustedBounds.y * massCenterHeightLocal, adjustedZ + massCenterBiasLocal * adjustedBounds.z);
            m_vehicleRigidBody.velocity = Vector3.zero;
            m_vehicleRigidBody.maxDepenetrationVelocity = DEPEN_VELOCITY;
            m_vehicleRigidBody.constraints = constraints;

            m_vehicleCollider.size = adjustedBounds;
            m_vehicleCollider.center = new Vector3(0.0f, adjustedY + 0.5f * adjustedBounds.y, adjustedZ + adjustedBounds.z * 0.5f);

            InitializeEnd();

            if (IsPrimary()) DriveEffects.SetPrimary(m_effects);
            m_effects.UpdateVehicleInfo(vehicleInfo, vehicleColor, setColor);
            m_effects.StartEffects();

            gameObject.SetActive(true);
        }

        public void Deinitialize()
        {
            m_effects.StopEffects();

            foreach (Wheel w in m_wheelObjects)
            {
                Object.DestroyImmediate(w.gameObject);
            }
            m_wheelObjects.Clear();
            gameObject.SetActive(false);
            m_vehicleRigidBody.velocity = Vector3.zero;
            m_vehicleRigidBody.angularVelocity = Vector3.zero;

            m_vehicleColor = default;
            m_variationMaskInv = ~0u;
            m_vehicleInfo = null;
            m_prevPosition = Vector3.zero;
            m_prevVelocity = Vector3.zero;
            m_prevPrevVelocity = Vector3.zero;
            m_isTurning = false;
            m_isTrailer = false;
            m_gear = m_gearNeutral;
            m_driveMode = ENGINE_MODE_NEUTRAL;
            m_lastReset = 0.0f;
            m_distanceTravelled = 0.0f;
            m_steer = 0.0f;
            m_brake = 0.0f;
            m_handbrake = 0.0f;
            m_throttle = 0.0f;
            m_nextGearChange = 0.0f;
            m_radps = 0.0f;
            m_prevRadps = 0.0f;
            m_radpsTrans = 0.0f;
            m_groundY = 0.0f;
        }

        public void RegisterWheel(Wheel w)
        {
            m_wheelCount++;

            if (w.isFront)
            {
                m_frontWheels++;
            }
            if (w.isRight)
            {
                m_rightWheels++;
            }
        }

        public void DeRegisterWheel(Wheel w)
        {
            m_wheelCount--;

            if (w.isFront)
            {
                m_frontWheels--;
            }
            if (w.isRight)
            {
                m_rightWheels--;
            }
        }

        protected virtual float enginePower { get => ModSettings.EnginePower; }
        protected virtual float brakingForce { get => ModSettings.BrakingForce; }
        protected virtual float downForce { get => ModSettings.DownForce; }
        protected virtual float driveBias { get => ModSettings.DriveBias; }
        protected virtual float brakeBias { get => ModSettings.BrakeBias; }
        protected virtual float springDamp {  get => ModSettings.SpringDamp; }
        protected virtual float springOffset { get => ModSettings.SpringOffset; }
        protected virtual float springSwayBar { get=> ModSettings.SpringSwayBar; }
        protected virtual float massCenterHeight {  get => ModSettings.MassCenterHeight; }
        protected virtual float massCenterBias {  get => ModSettings.MassCenterBias; }
        protected virtual float linearDrag { get => DRAG_FACTOR; }
        protected virtual float angularDrag { get => DRAG_FACTOR; }
        protected virtual float steerMax { get => STEER_MAX; }
        protected virtual float momentWheel { get => MOMENT_WHEEL; }
        protected virtual float parkSpeed { get => PARK_SPEED; }
        protected virtual float enginePeakRPS { get => ENGINE_PEAK_RPS; }
        protected virtual float engineOverRPS { get => ENGINE_OVER_RPS; }
        protected virtual float engineIdleRPS { get => ENGINE_IDLE_RPS; }
        protected virtual float massFactor { get => MASS_FACTOR; }
        protected virtual float massBias { get => MASS_BIAS; }
        protected virtual float rideHeight { get => RIDE_HEIGHT; }
        protected virtual float massCenterHeightTrailer { get => ModSettings.TrailerMassCenterHeight; }
        protected virtual float massCenterBiasTrailer { get => ModSettings.TrailerMassCenterBias; }
        protected virtual float massFactorTrailer { get => MASS_FACTOR_TRAILER; }
        protected virtual float massBiasTrailer { get => MASS_BIAS; }

        // Initialize the vehicle wheel configuration, calculate hitbox parameters, and configure constriants
        protected virtual void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref float groundY, ref RigidbodyConstraints constraints)
        {
            float width = adjustedBounds.x;
            float length = adjustedBounds.z;
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(width * 0.5f, groundY + RADIUS_WHEEL, adjustedZ + length), momentWheel, RADIUS_WHEEL, true, true));
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(-width * 0.5f, groundY + RADIUS_WHEEL, adjustedZ + length), momentWheel, RADIUS_WHEEL, true, true));
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(0.0f, groundY + RADIUS_WHEEL, adjustedZ + length * 0.5f), momentWheel, RADIUS_WHEEL, true, false));
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(width * 0.5f, groundY + RADIUS_WHEEL, adjustedZ), momentWheel, RADIUS_WHEEL, true, true, true));
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(-width * 0.5f, groundY + RADIUS_WHEEL, adjustedZ), momentWheel, RADIUS_WHEEL, true, true, true));
        }
        
        protected virtual void InitializeAdjust(ref float frontTorque, ref float rearTorque, ref float frontBraking, ref float rearBraking, ref float frontEBraking, ref float rearEBraking)
        {

        }

        protected virtual void InitializeEnd()
        {

        }

        // PreProcess function that runs before any physics calculation.
        protected virtual void PhysicsPreProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {

        }

        // Function that runs immediately after gravity is applied and TBN calculations are complete.
        protected virtual void PhysicsAfterTBN(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {

        }

        // Adjust suspension calculations after wheel suspension and velocity updates. Finalize normal impulses.
        protected virtual void PhysicsAdjustSuspension(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            float frontCompression = 0.0f;
            float rearCompression = 0.0f;
            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isFront)
                {
                    frontCompression += w.wheelCompression;
                }
                else
                {
                    rearCompression += w.wheelCompression;
                }
            }

            if (frontCount > 0)
            {
                frontCompression /= frontCount;
            }
            if (rearCount > 0)
            {
                rearCompression /= rearCount;
            }

            foreach (Wheel w in m_wheelObjects)
            {
                w.AddImpulses(impulseY: (w.wheelCompression - (w.isFront ? frontCompression : rearCompression)) * springSwayBar * DriveCommon.KN_TO_N * Time.fixedDeltaTime);
            }
        }

        // Function that runs immediately after PhysicsAdjustSuspension. Calculate any friction coefficients after the wheel normal state is complete.
        protected virtual void PhysicsFrictionCalculation(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            foreach (Wheel wheel in m_wheelObjects)
            {
                float coeffX = ModSettings.GripCoeffK;
                float coeffZ = ModSettings.GripCoeffK;

                if (wheel.isOnGround && wheel.wheelGroundType == MapUtils.COLLISION_TYPE.ROAD)
                {
                    coeffZ = Mathf.Lerp(ModSettings.GripCoeffS, ModSettings.GripCoeffK, Mathf.Max((wheel.wheelOptimSlip) / (1.0f - Wheel.GRIP_OPTIM_SLIP), 0.0f));

                    coeffX = wheel.isSteerable ? ModSettings.GripCoeffS : ModSettings.GripCoeffK;
                    coeffX = Mathf.Lerp(ModSettings.GripCoeffS, coeffX, Mathf.Max((wheel.wheelOptimSlip) / (1.0f - Wheel.GRIP_OPTIM_SLIP), 0.0f));
                }
                else
                {
                    coeffZ = 0.8f * Mathf.Lerp( ModSettings.GripCoeffS, ModSettings.GripCoeffK, wheel.wheelSlip);

                    coeffX = wheel.isSteerable ? ModSettings.GripCoeffS : ModSettings.GripCoeffK;
                    coeffX = 0.8f * Mathf.Lerp(ModSettings.GripCoeffS, coeffX, wheel.wheelSlip);
                }

                wheel.SetFriction(coeffX, coeffZ);
            }
        }

        // Function runs immediately after PhysicsFrictionCalculation. Feeds updated wheel velocity back to the engine.
        protected virtual void PhysicsFeedbackWheelAndEngine(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            float engineRps = 0.0f;

            foreach (Wheel w in m_wheelObjects)
            {
                // record distance travelled from previous tick
                m_distanceTravelled += w.wheelRadps * w.wheelRadius * Time.fixedDeltaTime / wheelCount;

                // apply wheel drag from previous tick
                w.ApplyDrag();

                engineRps += w.wheelRadps * w.wheelTorqueFract;
            }

            m_radpsTrans = engineRps;

            if (m_gear == m_gearNeutral)
            {
                engineRps = m_throttle * enginePeakRPS;
            }
            else
            {
                engineRps *= m_gearRatios[m_gear];
            }

            m_prevRadps = m_radps;
            engineRps = Mathf.Clamp(engineRps, engineIdleRPS, engineOverRPS);
            m_radps = Mathf.Lerp(engineRps, m_radps, s_engine_inertia);
        }

        // Function runs immediately after PhysicsFrictionCalculation if the vehicle is a trailer. Feeds updated wheel velocity back to the engine.
        protected virtual void PhysicsFeedbackWheelAndEngineTrailer(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
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

        // Torque curve.
        protected virtual float GetTorque(float radps) // Torque curve 27x(k-x)/(4k^3)+max(3(k/2-x)^3/k^4,0)
        {
            // Check https://www.desmos.com/calculator/fp0csjaazj for formulation.
            float k = enginePeakRPS;
            float x = Mathf.Max(radps, engineIdleRPS);
            float rawval = 27.0f * x * (k - x) / (4.0f * k * k * k) + Mathf.Max(3 * Mathf.Pow(k * 0.5f - x, 3.0f) / (k * k * k * k), 0.0f);
            return enginePower * DriveCommon.KW_TO_W * (1.0f - DRAG_DRIVETRAIN) * rawval;
        }

        // Power curve. Should not be modified. Derived from torque curve.
        protected float GetPower(float radps) // Power curve 27x^2(k-x)/(4k^3)
        {
            return GetTorque(radps) * radps;
        }

        // Function runs immediately after PhysicsFeedbackWheelAndEngine with auto transmissions. Selects a new gear based on engine state.
        protected virtual void PhysicsSelectGear(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            if (m_driveMode == ENGINE_MODE_FORWARD)
            {
                int chosenGear = Mathf.Max(m_gear, m_gearNeutral + 1);
                float currTorque = m_gearRatios[chosenGear] * GetTorque(m_radps);
                float predTransRps = m_radps / (m_gear == m_gearNeutral ? 1.0f : m_gearRatios[m_gear]);

                for (int gear = m_gearNeutral + 1; gear < m_gearRatios.Length; gear++)
                {
                    float tmpTorque = m_gearRatios[gear] * GetTorque(predTransRps * m_gearRatios[gear]);
                    if (tmpTorque > currTorque * (1.0f + ENGINE_AUTO_SHIFT_THRESH))
                    {
                        chosenGear = gear;
                        currTorque = tmpTorque;
                    }
                }

                if (m_gear != chosenGear && Time.time > m_nextGearChange)
                {
                    m_gear = chosenGear;
                    m_nextGearChange = Time.time + GEAR_RESP_AUTO;
                }
            }
            else if (m_driveMode == ENGINE_MODE_REVERSE)
            {
                int chosenGear = Mathf.Min(m_gear, m_gearNeutral - 1);
                float currTorque = m_gearRatios[chosenGear] * GetTorque(m_radps);
                float predTransRps = m_radps / (m_gear == m_gearNeutral ? -1.0f : m_gearRatios[m_gear]);

                for (int gear = m_gearNeutral - 1; gear >= 0; gear--)
                {
                    float tmpTorque = m_gearRatios[gear] * GetTorque(predTransRps * m_gearRatios[gear]);
                    if (tmpTorque < currTorque * (1.0f + ENGINE_AUTO_SHIFT_THRESH))
                    {
                        chosenGear = gear;
                        currTorque = tmpTorque;
                    }
                }

                if (m_gear != chosenGear && Time.time > m_nextGearChange)
                {
                    m_gear = chosenGear;
                    m_nextGearChange = Time.time + GEAR_RESP_AUTO;
                }
            }
            else
            {
                m_gear = m_gearNeutral;
            }
        }

        // Function runs immediately after PhysicsSelectGear. Feeds updated engine state back to the wheels. This includes any transmission effects and nannies.
        protected virtual void PhysicsFeedForwardWheelAndEngine(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {
            float engineTorque = GetTorque(m_radps);
            float avgRps = 0.0f;
            int powered = 0;

            engineTorque = (engineTorque > 0.0f ? m_throttle : 1.0f) * m_gearRatios[m_gear] * engineTorque;

            foreach (Wheel w in m_wheelObjects)
            {
                // Find the average wheel speed per differential
                if (w.isPowered)
                {
                    avgRps += w.wheelRadps * w.wheelTorqueFract;
                    powered++;
                }
            }

            foreach (Wheel w in m_wheelObjects)
            {
                // calcuate wheel angular velocity along with any assists.
                float wheelTorque = w.wheelTorqueFract * engineTorque;

                //// LSD resist more at higher TCS levels
                float LSDFactor = (ModSettings.TCSLevel >= (int)DriveCommon.TRACTIONCTL_LEVEL.SPORT) ? DIFF_LSD_FACTOR_HIGH : DIFF_LSD_FACTOR_LOW;
                wheelTorque += (avgRps - w.wheelRadps) * w.wheelMoment / Time.fixedDeltaTime * LSDFactor * w.wheelTorqueFract * powered;

                // TCS cut power when on ground, accelerating, and slipping
                switch (ModSettings.TCSLevel)
                {
                    case (int)DriveCommon.TRACTIONCTL_LEVEL.FULL:
                        {
                            if (w.wheelRadps * wheelTorque > 0.0f)
                            {
                                wheelTorque *= 1.0f - Mathf.Min(w.wheelSlip / Wheel.GRIP_OPTIM_SLIP, 1.0f);
                            }
                        }
                        break;
                    case (int)DriveCommon.TRACTIONCTL_LEVEL.SPORT:
                        {
                            if (w.isOnGround && w.wheelRadps * wheelTorque > 0.0f)
                            {
                                wheelTorque *= 1.0f - Mathf.Min(w.wheelSlip, 1.0f);
                            }
                        }
                        break;
                    case (int)DriveCommon.TRACTIONCTL_LEVEL.TRACK:
                        {
                            if (w.isOnGround && w.wheelRadps * wheelTorque > 0.0f)
                            {
                                wheelTorque *= 1.0f - Mathf.Min(w.wheelSlip * 0.5f, 1.0f);
                            }
                        }
                        break;
                    case (int)DriveCommon.TRACTIONCTL_LEVEL.OFF:
                    default:
                        break;
                }

                // braking ABS
                float totalBrake = (w.wheelSlip < Wheel.GRIP_OPTIM_SLIP * 0.75f || !ModSettings.BrakingABS || !w.isOnGround) ? m_brake : 0.0f;

                wheelTorque -= Mathf.Sign(w.wheelRadps) * Mathf.Min(totalBrake * w.wheelBrakeForce * w.wheelRadius, Mathf.Abs(wheelTorque + w.wheelRadps * w.wheelMoment / Time.fixedDeltaTime));

                // braking Handbrake
                float handBrake = m_handbrake;

                wheelTorque -= Mathf.Sign(w.wheelRadps) * Mathf.Min(handBrake * w.wheelHandbrakeForce * w.wheelRadius, Mathf.Abs(wheelTorque + w.wheelRadps * w.wheelMoment / Time.fixedDeltaTime));

                w.AddVelocity(wheelTorque * Time.fixedDeltaTime / w.wheelMoment);
            }
        }

        // PostProcess function that runs after all physics calculations.
        protected virtual void PhysicsPostProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {

        }

        protected virtual void HandleInputOnFixedUpdate(int invert)
        {
            // Throttle and Brake
            bool throttling = false;
            bool braking = false;
            float st = Input.GetAxisRaw(DriveCommon.AXIS_STEER);
            float tb = -Input.GetAxisRaw(DriveCommon.AXIS_THROTTLEBRAKE);
            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyMoveForward.Key) || tb > 0.0f)
            {
                if (ModSettings.AutoTrans)
                {
                    if (invert < 0)
                    {
                        if (m_driveMode <= ENGINE_MODE_NEUTRAL)
                        {
                            m_throttle = 0.0f;
                            m_brake = Mathf.Clamp01(m_brake + Time.fixedDeltaTime * THROTTLE_RESP);
                            if (tb > 0.0f) // override with controller value if present
                            {
                                m_brake = tb;
                            }
                            braking = true;
                        }
                    }
                    else if (m_throttle == 0.0f && Time.time > m_nextGearChange && m_driveMode <= ENGINE_MODE_NEUTRAL)
                    {
                        m_driveMode++;
                        m_nextGearChange = Time.time + GEAR_RESP;
                    }
                    if (m_driveMode > ENGINE_MODE_NEUTRAL)
                    {
                        m_brake = 0.0f;
                        m_throttle = Mathf.Clamp01(m_throttle + Time.fixedDeltaTime * THROTTLE_RESP);
                        if (tb > 0.0f) // override with controller value if present
                        {
                            m_throttle = tb;
                        }
                        throttling = true;
                    }
                }
                else // ManualTrans
                {
                    m_throttle = Mathf.Clamp01(m_throttle + Time.fixedDeltaTime * THROTTLE_RESP);
                    if (tb > 0.0f) // override with controller value if present
                    {
                        m_throttle = tb;
                    }
                    throttling = true;
                }
            }
            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyMoveBackward.Key) || tb < 0.0f)
            {
                if (ModSettings.AutoTrans)
                {
                    if (invert > 0)
                    {
                        if (m_driveMode >= ENGINE_MODE_NEUTRAL)
                        {
                            m_throttle = 0.0f;
                            m_brake = Mathf.Clamp01(m_brake + Time.fixedDeltaTime * THROTTLE_RESP);
                            if (tb < 0.0f) // override with controller value if present
                            {
                                m_brake = -tb;
                            }
                            braking = true;
                        }
                    }
                    else if (m_throttle == 0.0f && Time.time > m_nextGearChange && m_driveMode >= ENGINE_MODE_NEUTRAL)
                    {
                        m_driveMode--;
                        m_nextGearChange = Time.time + GEAR_RESP;
                    }
                    if (m_driveMode < ENGINE_MODE_NEUTRAL)
                    {
                        m_brake = 0.0f;
                        m_throttle = Mathf.Clamp01(m_throttle + Time.fixedDeltaTime * THROTTLE_RESP);
                        if (tb < 0.0f) // override with controller value if present
                        {
                            m_throttle = -tb;
                        }
                        throttling = true;
                    }
                }
                else // ManualTrans
                {
                    m_brake = Mathf.Clamp01(m_brake + Time.fixedDeltaTime * THROTTLE_RESP);
                    if (tb < 0.0f) // override with controller value if present
                    {
                        m_brake = -tb;
                    }
                    braking = true;
                }
            }
            if (ModSettings.AutoTrans && invert == 0 && m_throttle == 0.0f)
            {
                if (m_driveMode != ENGINE_MODE_NEUTRAL && Time.time > m_nextGearChange)
                {
                    m_driveMode = ENGINE_MODE_NEUTRAL;
                    m_nextGearChange = Time.time + GEAR_RESP;
                }
                m_brake = 1.0f;
                braking = true;
            }
            if (!throttling)
            {
                m_throttle = Mathf.Clamp01(m_throttle - Time.fixedDeltaTime * THROTTLE_REST);
            }
            if (!braking)
            {
                m_brake = Mathf.Clamp01(m_brake - Time.fixedDeltaTime * THROTTLE_REST);
            }

            // Steering
            m_isTurning = false;
            float steerLimit = Mathf.Clamp(1.0f - STEER_DECAY * Vector3.Magnitude(m_vehicleRigidBody.velocity), 0.01f, 1.0f);
            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyMoveRight.Key))
            {
                float factor = (m_steer < 0.0f) ? STEER_RESP + STEER_REST : STEER_RESP;
                m_steer = Mathf.Clamp(m_steer + Time.fixedDeltaTime * factor, -steerLimit, steerLimit);
                m_isTurning = true;
            }
            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyMoveLeft.Key))
            {
                float factor = (m_steer > 0.0f) ? STEER_RESP + STEER_REST : STEER_RESP;
                m_steer = Mathf.Clamp(m_steer - Time.fixedDeltaTime * factor, -steerLimit, steerLimit);
                m_isTurning = true;
            }
            if (st != 0.0f)
            {
                m_isTurning = true;
                m_steer = st;
            }
            if (!m_isTurning)
            {
                if (m_steer > 0.0f)
                {
                    m_steer = Mathf.Clamp(m_steer - Time.fixedDeltaTime * STEER_REST, 0.0f, steerLimit);
                }
                if (m_steer < 0.0f)
                {
                    m_steer = Mathf.Clamp(m_steer + Time.fixedDeltaTime * STEER_REST, -steerLimit, 0.0f);
                }
            }

            // Handbrake
            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyHandbrake.Key) || Input.GetKey(DriveCommon.s_handbrakeController1) || Input.GetKey(DriveCommon.s_handbrakeController2))
            {
                m_handbrake = Mathf.Clamp01(m_handbrake + Time.fixedDeltaTime * THROTTLE_RESP);
            }
            else
            {
                m_handbrake = Mathf.Clamp01(m_handbrake - Time.fixedDeltaTime * THROTTLE_REST);
            }
        }

        protected virtual void AwakeExt()
        {

        }

        protected virtual void HandleInputOnUpdate()
        {
            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyResetVehicle.Key) && m_lastReset + RESET_FREQ < Time.time)
            {
                Vector3 dir = m_vehicleRigidBody.transform.TransformDirection(Vector3.forward);
                Vector3 terrainNorm = MapUtils.TerrainNormal(m_vehicleRigidBody.transform.position, out _);
                if (Mathf.Abs(Vector3.Dot(dir, terrainNorm)) > 1.0f - DriveCommon.FLOAT_ERROR)
                {
                    dir += Vector3.forward;
                }
                dir = dir - Vector3.Dot(dir, terrainNorm) * terrainNorm;
                dir = dir.normalized;
                Quaternion rot = Quaternion.LookRotation(dir, terrainNorm);
                Vector3 pos = m_vehicleRigidBody.transform.position;
                pos.y = MapUtils.CalculateHeight(pos, RESET_SCAN_HEIGHT, out var _) - m_groundY + RESET_HEIGHT;
                m_vehicleRigidBody.transform.SetPositionAndRotation(pos, rot);
                m_lastReset = Time.time;
            }
            if (!ModSettings.AutoTrans)
            {
                if ((Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyGearUp.Key) || Input.GetKeyDown(DriveCommon.s_upshiftController1) || Input.GetKeyDown(DriveCommon.s_upshiftController2))
                    && m_nextGearChange < Time.time
                    && m_gear < m_gearRatios.Length - 1)
                {
                    m_gear += 1;
                    m_nextGearChange = Time.time + GEAR_RESP;
                }
                if ((Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyGearDown.Key) || Input.GetKeyDown(DriveCommon.s_downshiftController1) || Input.GetKeyDown(DriveCommon.s_downshiftController2))
                    && m_nextGearChange < Time.time
                    && m_gear > 0)
                {
                    m_gear -= 1;
                    m_nextGearChange = Time.time + GEAR_RESP;
                }
            }
        }

        private void Awake()
        {
            m_vehicleRigidBody = gameObject.AddComponent<Rigidbody>();
            m_vehicleRigidBody.isKinematic = false;
            m_vehicleRigidBody.useGravity = false;
            m_vehicleRigidBody.freezeRotation = false;
            m_vehicleRigidBody.interpolation = RigidbodyInterpolation.Interpolate;

            PhysicMaterial material = new PhysicMaterial();
            material.bounciness = 0.05f;
            material.staticFriction = 0.1f;

            m_vehicleCollider = gameObject.AddComponent<BoxCollider>();
            m_vehicleCollider.material = material;

            m_effects = gameObject.AddComponent<DriveEffects>();
            m_effects.Initialize(this);

            if (s_first_create)
            {
                s_first_create = false;
                s_engine_inertia = (float)System.Math.Pow(ENGINE_INERTIA, Time.fixedDeltaTime);
                s_drag_wheel_powered = (float)(1.0 - System.Math.Pow(1.0 - DRAG_WHEEL_POWERED, Time.fixedDeltaTime));
                s_drag_wheel = (float)(1.0 - System.Math.Pow(1.0 - DRAG_WHEEL, Time.fixedDeltaTime));
                s_primaryVehicle = this;
            }

            AwakeExt();
        }

        private void Update()
        {
            HandleInputOnUpdate();

            DebugHelper.DrawDebugBox(m_vehicleCollider.size, m_vehicleCollider.transform.TransformPoint(m_vehicleCollider.center), m_vehicleCollider.transform.rotation, Color.magenta);
        }
        private void FixedUpdate()
        {
            Vector3 vehiclePos = m_vehicleRigidBody.transform.position;
            Vector3 vehicleVel = m_vehicleRigidBody.velocity;
            Vector3 vehicleAngularVel = m_vehicleRigidBody.angularVelocity;
            float speed = Vector3.Dot(Vector3.forward, m_vehicleRigidBody.transform.InverseTransformDirection(vehicleVel));
            int invert = Mathf.Abs(speed) < parkSpeed ? 0 : (speed > 0.0f ? 1 : -1);

            if (m_isTrailer)
            {
                m_brake = s_primaryVehicle.m_brake;
                m_handbrake = s_primaryVehicle.m_handbrake;
            }
            else
            {
                HandleInputOnFixedUpdate(invert);
            }

            WheelPhysics(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel);

            LimitVelocity();

            m_prevPrevVelocity = m_prevVelocity;
            m_prevVelocity = vehicleVel;
            m_prevPosition = vehiclePos;
        }
        private void OnCollisionEnter(Collision collision)
        {
            LimitVelocity();

            ColliderContainer container = collision.collider.gameObject.GetComponent<ColliderContainer>();
            if (container?.Type == ColliderContainer.ContainerType.TYPE_VEHICLE)
            {
                ref Vehicle otherVehicle = ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer[container.ID];

                ref Vector3 otherVelocity = ref (otherVehicle.m_lastFrame <= 1 ?
                    ref (otherVehicle.m_lastFrame == 0 ? ref otherVehicle.m_frame0.m_velocity : ref otherVehicle.m_frame1.m_velocity) :
                    ref (otherVehicle.m_lastFrame == 2 ? ref otherVehicle.m_frame2.m_velocity : ref otherVehicle.m_frame3.m_velocity));

                float collisionOrientation = Vector3.Dot(Vector3.Normalize(m_vehicleRigidBody.position - collision.collider.transform.position), Vector3.Normalize(otherVelocity));

                otherVelocity = otherVelocity * 0.8f * (0.5f + (-collisionOrientation + 1.0f) * 0.25f);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            LimitVelocity();
        }

        private void OnCollisionExit(Collision collision)
        {
            LimitVelocity();
        }
        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint && Logging.DetailLogging)
            {
                StringBuilder debugString = new StringBuilder();
                debugString.AppendFormat("v: {0}\n", this);
                debugString.AppendFormat("dm: {0} g: {1}\n", m_driveMode, m_gear);
                debugString.AppendFormat("t: {0} b: {1} hb: {2}\n", m_throttle.ToString("0.000"), m_brake.ToString("0.000"), m_handbrake.ToString("0.000"));
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

                GUI.Label(new Rect(100f, 100f + (m_isTrailer ? 350f : 0f), 700f, 350f), debugString.ToString(), m_style);
            }
        }

        private void WheelPhysics(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel)
        {
            Vector3 upVec = m_vehicleRigidBody.transform.TransformDirection(Vector3.up).normalized;
            Vector3 forwardVec = m_vehicleRigidBody.transform.TransformDirection(Vector3.forward).normalized;

            PhysicsPreProcess(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);

            m_vehicleRigidBody.AddForce(Vector3.down * (ModSettings.Gravity * m_vehicleRigidBody.mass) - upVec * downForce * Mathf.Abs(Vector3.Dot(vehicleVel, forwardVec)), ForceMode.Force);

            foreach (Wheel w in m_wheelObjects)
            {
                // first calculate the heights and tbn at each wheel.
                w.CalcRoadState();
            }

            PhysicsAfterTBN(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);

            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isSteerable)
                {
                    w.gameObject.transform.localRotation = Quaternion.Euler(0, (w.isInvertedSteer ? -1.0f : 1.0f) * steerMax * m_steer, 0);
                }
                else
                {
                    w.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }

                // calculate fist pass normal impulses. Update wheel suspension position.
                w.CalcWheelState(upVec);

            }

            PhysicsAdjustSuspension(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);

            PhysicsFrictionCalculation(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);

            // calculate new engine and wheel angular velocity
            if (!m_isTrailer)
            {
                PhysicsFeedbackWheelAndEngine(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);
            }
            else
            {
                PhysicsFeedbackWheelAndEngineTrailer(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);
            }

            if (ModSettings.AutoTrans && !m_isTrailer)
            {
                PhysicsSelectGear(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);
            }

            PhysicsFeedForwardWheelAndEngine(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);

            foreach (Wheel w in m_wheelObjects)
            {
                // calculate the lateral and longitudinal forces. Apply all forces.
                if (w.isOnGround)
                {
                    Vector3 netImpulse = Vector3.zero;

                    float normalContribution = 0.0f;
                    foreach (Wheel wAlt in m_wheelObjects)
                    {
                        normalContribution += Vector3.Dot(w.wheelGroundNormal, wAlt.wheelGroundNormal) * wAlt.wheelNormalImpulse;
                    }
                    normalContribution = w.wheelNormalImpulse / normalContribution;

                    float longSpeed = Vector3.Dot(w.wheelContactVelocity, w.wheelGroundTangent);
                    float lateralSpeed = Vector3.Dot(w.wheelContactVelocity, w.wheelGroundBinormal);

                    float longComponent = w.wheelMoment * (w.wheelRadps - longSpeed / w.wheelRadius) / w.wheelRadius;
                    if (Mathf.Abs(longSpeed) < 1.0f || Mathf.Abs(w.wheelRadps) < 0.1f) // exaggerated torque at low speeds for better stop and start
                    {
                        longComponent = normalContribution * m_vehicleRigidBody.mass * (w.wheelRadps * w.wheelRadius - longSpeed);
                    }

                    float lateralComponent = -normalContribution * m_vehicleRigidBody.mass * lateralSpeed;

                    if (w.wheelOptimSlip > 0.0f)
                    {
                        DebugHelper.DrawDebugMarker(2.0f, w.wheelContactPoint, Quaternion.LookRotation(w.wheelGroundTangent, w.wheelGroundNormal), Color.yellow);
                    }
                    else
                    {
                        DebugHelper.DrawDebugMarker(2.0f, w.wheelContactPoint, Quaternion.LookRotation(w.wheelGroundTangent, w.wheelGroundNormal), Color.green);
                    }

                    float frictionScaleLong = Mathf.Min(w.wheelNormalImpulse * w.wheelFrictionCoeffZ, Mathf.Abs(longComponent)) / Mathf.Max(Mathf.Abs(longComponent), DriveCommon.FLOAT_ERROR);
                    float frictionScaleLat = Mathf.Min(w.wheelNormalImpulse * w.wheelFrictionCoeffX, Mathf.Abs(lateralComponent)) / Mathf.Max(Mathf.Abs(lateralComponent), DriveCommon.FLOAT_ERROR);

                    w.AddImpulses(impulseX: lateralComponent * frictionScaleLat, impulseZ: longComponent * frictionScaleLong);

                    netImpulse += w.wheelNormalImpulse * w.wheelGroundNormal;
                    netImpulse += w.wheelBinormalImpulse * w.wheelGroundBinormal;
                    netImpulse += w.wheelTangentImpulse * w.wheelGroundTangent;

                    m_vehicleRigidBody.AddForceAtPosition(netImpulse, w.wheelContactPoint, ForceMode.Impulse);
                }
            }

            PhysicsPostProcess(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);
        }

        private void LimitVelocity()
        {
            if (m_vehicleRigidBody.velocity.magnitude > Settings.ModSettings.MaxVelocity / DriveCommon.MS_TO_KMPH)
            {
                m_vehicleRigidBody.AddForce(m_vehicleRigidBody.velocity.normalized * Settings.ModSettings.MaxVelocity / DriveCommon.MS_TO_KMPH - m_vehicleRigidBody.velocity, ForceMode.VelocityChange);
            }
        }
    }
}
