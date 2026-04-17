using AlgernonCommons;
using ColossalFramework;
using DriveIt.Settings;
using DriveIt.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DriveIt.Vehicles
{
    public class VehicleGeneric : MonoBehaviour
    {
        private const float RESET_SCAN_HEIGHT = 2.0f;
        private const float RESET_HEIGHT = 0.1f;
        private const float RESET_FREQ = 2.0f;
        private const float THROTTLE_RESP = 2.0f;
        private const float THROTTLE_REST = 2.0f;
        private const float MASS_FACTOR = 85.0f;
        private const float RADIUS_D_WHEEL = 0.2f;
        private const float STEER_RESP = 1.75f;
        private const float STEER_REST = 1.75f;
        private const float DEPEN_VELOCITY = 2.0f;
        private const float STEER_MAX = 15.0f;
        private const float STEER_DECAY = 0.0075f;
        private const float GEAR_RESP = 0.2f;
        private const float GEAR_RESP_AUTO = 1.0f;
        private const float PARK_SPEED = 0.25f;
        private const float DRAG_FACTOR = 0.25f;
        private const float DRAG_DRIVETRAIN = 0.15f;
        private const float DRAG_WHEEL_POWERED = 0.25f;
        private const float DRAG_WHEEL = 0.15f;
        private const float MOMENT_WHEEL = 1.5f;
        private const float VALID_INCLINE = 0.5f;
        private const float GRIP_HIGH_SLIP = 0.5f;
        private const float GRIP_OPTIM_SLIP = 0.2f;
        private const float GRIP_SLIP_SPEED_LOW = 8.0f;
        private const float GRIP_SLIP_SPEED_HIGH = 40.0f;
        private const float DIFF_LSD_FACTOR_LOW = 0.05f;
        private const float DIFF_LSD_FACTOR_HIGH = 0.25f;
        private const float ENGINE_PEAK_RPS = 900.0f;
        private const float ENGINE_OVER_RPS = 1100.0f;
        private const float ENGINE_IDLE_RPS = 90.0f;
        private const float ENGINE_INERTIA = 0.01f;
        private const int ENGINE_MODE_REVERSE = -1;
        private const int ENGINE_MODE_NEUTRAL = 0;
        private const int ENGINE_MODE_FORWARD = 1;
        private const float ENGINE_AUTO_SHIFT_THRESH = 0.1f;

        private static float s_engine_inertia;
        private static float s_drag_wheel_powered;
        private static float s_drag_wheel;
        private static VehicleGeneric s_primaryVehicle = null;

        protected const float ACCEL_G = 10f;

        protected DriveEffects m_effects;
        protected Rigidbody m_vehicleRigidBody;
        protected BoxCollider m_vehicleCollider;
        protected VehicleInfo m_vehicleInfo;
        protected Vehicle.Flags m_vehicleFlags;
        protected Color m_vehicleColor;
        protected Vector3 m_prevPosition;
        protected Vector3 m_prevVelocity;
        protected Vector3 m_prevPrevVelocity;
        protected bool m_isTurning = false;
        protected int m_gear = 0;
        protected int m_gearNeutral = 2;
        protected float[] m_gearRatios = { -5.0f, -10.0f, 0.0f, 10.0f, 5.0f };
        protected string[] m_gearNames = { "R2", "R1", "N", "D1", "D2" };
        protected int m_driveMode = ENGINE_MODE_NEUTRAL;
        protected int m_wheelCount = 0;
        protected int m_frontWheels = 0;
        protected int m_rightWheels = 0;
        protected float m_lastReset = 0.0f;
        protected float m_distanceTravelled = 0.0f;
        protected float m_steer = 0.0f;
        protected float m_brake = 0.0f;
        protected float m_throttle = 0.0f;
        protected float m_nextGearChange = 0.0f;
        protected float m_radps = 0.0f;
        protected float m_prevRadps = 0.0f;
        protected float m_radpsTrans = 0.0f;
        protected float m_boundMin = 0.0f;
        protected List<Wheel> m_wheelObjects = new List<Wheel>();

        public float odometer { get => m_distanceTravelled; }
        public float speedometer { get => m_vehicleRigidBody.velocity.magnitude * DriveCommon.MS_TO_KMPH / ModSettings.MaxVelocity; }
        public float tachometer { get => m_radps / ENGINE_PEAK_RPS; }
        public float steer { get => m_steer * steerMax; }
        public float brake { get => m_brake; }
        public float throttle { get => m_throttle; }
        public string gear { get => m_gearNames[m_gear]; }
        public float speed { get => m_vehicleRigidBody.velocity.magnitude; }
        public Vector3 velocity { get => m_vehicleRigidBody.velocity; }
        public Vector3 acceleration { get => (m_prevVelocity - m_prevPrevVelocity) / Time.fixedDeltaTime; }
        public float rpm { get => m_radps * DriveCommon.RPS_TO_RPM; }
        public float radps { get => m_radps; }
        public float angularAcceleration { get => (m_radps - m_prevRadps) / Time.fixedDeltaTime; }
        public VehicleType vehicleType { get => VehicleType.Generic; }
        public List<Wheel> wheels { get => m_wheelObjects; }
        public Vehicle.Flags vehicleFlags { get => m_vehicleFlags; }
        public int wheelCount { get => m_wheelCount; }
        public int frontCount { get => m_frontWheels; }
        public int rearCount { get => m_wheelCount - m_frontWheels; }
        public int rightCount { get => m_rightWheels; }
        public int leftCount { get => m_wheelCount - m_rightWheels; }
        public bool inlineWheels { get => rightCount == 0 || leftCount == 0; }
        public bool parallelWheels { get => rearCount == 0 || frontCount == 0; }
        public float springHeight { get => springOffset;  }
        
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

        public class Wheel : MonoBehaviour
        {
            public bool isOnGround { get => onGround; }
            public MapUtils.COLLISION_TYPE wheelGroundType { get => groundType; }
            public Vector3 wheelGroundNormal { get => normal; }
            public Vector3 wheelGroundTangent { get => tangent; }
            public Vector3 wheelGroundBinormal { get => binormal; }
            public Vector3 wheelContactPoint { get => contactPoint; }
            public Vector3 wheelContactVelocity { get => contactVelocity; }
            public float wheelNormalImpulse {  get => impulse.y; }
            public float wheelTangentImpulse {  get => impulse.z; }
            public float wheelBinormalImpulse {  get => impulse.x; }
            public float wheelTorqueFract { get => torqueFract; }
            public float wheelBrakeFract { get => brakeFract; }
            public float wheelRadps {  get => radps; }
            public float wheelSlip { get => slip; }
            public float wheelOptimSlip { get => slip - GRIP_OPTIM_SLIP; }
            public float wheelHighSlip { get => slip - GRIP_HIGH_SLIP; }
            public float wheelFrictionCoeffX { get => frictionCoeffX; }
            public float wheelFrictionCoeffZ { get => frictionCoeffZ; }
            public float wheelCompression { get => compression; }
            public Vector3 wheelOrigin { get => origin; }
            public float wheelRadius { get => radius; }
            public float wheelMoment { get => moment; }
            public bool isPowered { get => powered && (torqueFract > 0.0f); }
            public bool isSteerable { get => steerable; }
            public bool isInvertedSteer { get => inverted; }
            public bool isFront { get => front; }
            public bool isRight { get => right; }

            private VehicleGeneric vehicle;
            private MapUtils.COLLISION_TYPE groundType;
            private Vector3 origin;
            private Vector3 tangent;
            private Vector3 binormal;
            private Vector3 normal;
            private Vector3 heightSample;
            private Vector3 contactPoint;
            private Vector3 contactVelocity;
            private Vector3 impulse;
            private float radius;
            private float moment;
            private float radps;
            private float drag;
            private float torqueFract;
            private float brakeFract;
            private float compression;
            private float frictionCoeffX;
            private float frictionCoeffZ;
            private float slip;
            private bool onGround;
            private bool powered;
            private bool steerable;
            private bool inverted;
            private bool front;
            private bool right;

            public static Wheel InstanceWheel(VehicleGeneric parent, Vector3 localpos, float moment, float radius, bool isPowered = true, bool isSteerable = false, bool isInvertedSteer = false)
            {
                GameObject wheelObject = new GameObject("Wheel");
                Wheel w = wheelObject.AddComponent<Wheel>();
                wheelObject.transform.SetParent(parent.transform);
                wheelObject.transform.localPosition = localpos;
                w.vehicle = parent;
                w.tangent = Vector3.zero;
                w.binormal = Vector3.zero;
                w.normal = Vector3.zero;
                w.heightSample = Vector3.zero;
                w.contactPoint = Vector3.zero;
                w.contactVelocity = Vector3.zero;
                w.impulse = Vector3.zero;
                w.origin = localpos;
                w.moment = moment;
                w.radius = radius;
                w.radps = 0.0f;
                w.drag = 0.0f;
                w.torqueFract = 0.0f;
                w.brakeFract = 0.0f;
                w.compression = 0.0f;
                w.frictionCoeffX = ModSettings.GripCoeffK;
                w.frictionCoeffZ = ModSettings.GripCoeffK;
                w.slip = 0.0f;
                w.onGround = false;
                w.powered = isPowered;
                w.steerable = isSteerable;
                w.inverted = isInvertedSteer;
                w.front = localpos.z > 0.0f;
                w.right = localpos.x > 0.0f;

                parent.RegisterWheel(w);

                return w;
            }

            public void AdjustWheel(float torqueFract = 0.0f, float brakeFract = 0.0f, float drag = 0.0f)
            {
                this.torqueFract = torqueFract;
                this.brakeFract = brakeFract;
                this.drag = drag;
            }

            public void ApplyDrag()
            {
                this.radps *= 1.0f - this.drag;
            }

            public void AddVelocity(float radps)
            {
                this.radps += radps;
            }

            public void SetVelocity(float radps)
            {
                this.radps = radps;
            }

            public void OnDestroy()
            {
                this.vehicle.DeRegisterWheel(this);
            }

            // Calculate the road tbn and height at the wheel position.
            public void CalcRoadState()
            {
                Vector3 pos = this.gameObject.transform.position;
                Vector3 xdisp = pos;
                Vector3 zdisp = pos;

                xdisp.x += 0.1f;
                zdisp.z += 0.1f;
                pos.y = MapUtils.CalculateHeight(pos, 0.0f, out this.groundType);
                xdisp.y = MapUtils.CalculateHeight(xdisp, 0.0f, out var _);
                zdisp.y = MapUtils.CalculateHeight(zdisp, 0.0f, out var _);

                this.heightSample = pos;
                this.normal = Vector3.Normalize(Vector3.Cross(zdisp - pos, xdisp - pos));
                this.binormal = Vector3.Normalize(Vector3.Cross(this.gameObject.transform.TransformDirection(Vector3.forward), this.normal));
                this.tangent = Vector3.Normalize(Vector3.Cross(this.normal, this.binormal));
            }

            // Adjust current wheel speed with previous tick sim and calculate new suspension position and state.
            public void CalcWheelState(Vector3 upVec)
            {
                if (this.onGround)
                {
                    Vector3 prelimContactVel = vehicle.m_vehicleRigidBody.GetPointVelocity(this.contactPoint);
                    float radDelta = Vector3.Dot(prelimContactVel, this.tangent) / this.radius - this.radps;
                    this.radps += Mathf.Sign(radDelta) * Mathf.Min(Mathf.Abs(radDelta), this.impulse.y * this.radius * this.frictionCoeffZ / this.moment);
                }

                // calculate fist pass normal impulses. Update wheel suspension position.
                this.onGround = false;
                this.impulse = Vector3.zero;
                this.slip = 1.0f;
                float normDotUp = Vector3.Dot(this.normal, upVec);
                if (normDotUp > VALID_INCLINE)
                {
                    Vector3 originWheelBottom = vehicle.m_vehicleRigidBody.transform.TransformPoint(this.origin + Vector3.down * this.radius);
                    float compression = Mathf.Max(Vector3.Dot(this.heightSample - originWheelBottom, this.normal) / normDotUp, 0.0f);
                    float springVel = (compression - this.compression) / Time.fixedDeltaTime;
                    float deltaVel = -vehicle.springDamp * Mathf.Exp(-vehicle.springDamp * Time.fixedDeltaTime) * (compression + springVel * Time.fixedDeltaTime) + springVel * Mathf.Exp(-vehicle.springDamp * Time.fixedDeltaTime) - springVel;

                    this.gameObject.transform.localPosition = new Vector3(this.origin.x, this.origin.y + compression, this.origin.z);
                    this.compression = compression;

                    if (deltaVel < 0.0f)
                    {
                        this.onGround = true;
                        this.impulse.y = (-deltaVel) * vehicle.m_vehicleRigidBody.mass / vehicle.wheelCount;
                        this.contactPoint = this.gameObject.transform.TransformPoint(new Vector3(0.0f, -this.radius, 0.0f));
                        this.contactVelocity = vehicle.m_vehicleRigidBody.GetPointVelocity(this.contactPoint);
                        Vector3 flatVel = this.contactVelocity - Vector3.Dot(this.contactVelocity, this.normal) * this.normal;
                        this.slip = Mathf.Clamp01(Vector3.Magnitude(flatVel - (this.radps * this.radius * this.tangent)) / Mathf.Clamp(flatVel.magnitude, GRIP_SLIP_SPEED_LOW, GRIP_SLIP_SPEED_HIGH));
                    }
                }
                else
                {
                    this.compression = 0.0f;
                    this.gameObject.transform.localPosition = this.origin;
                }
            }

            public void SetFriction(float coeffX, float coeffZ)
            {
                this.frictionCoeffX = coeffX;
                this.frictionCoeffZ = coeffZ;
            }

            public void AddImpulses(float impulseX = 0.0f, float impulseY = 0.0f, float impulseZ = 0.0f)
            {
                this.impulse.x += impulseX;
                this.impulse.y = Mathf.Max(this.impulse.y + impulseY, 0.0f);
                this.impulse.z += impulseZ;
            }
        }

        public bool IsPrimary() { return this == s_primaryVehicle; }

        public static VehicleGeneric InstanceVehicle(VehicleInfo info)
        {
            GameObject vgo = new GameObject("PrimaryVehicleObject");

            if (
                info.m_vehicleType == VehicleInfo.VehicleType.Plane)
            {
                return vgo.AddComponent<VehiclePlane>();
            }
            if (
                info.m_vehicleType == VehicleInfo.VehicleType.Ferry ||
                info.m_vehicleType == VehicleInfo.VehicleType.Ship)
            {
                return vgo.AddComponent<VehicleBoat>();
            }
            if (
                info.m_vehicleType == VehicleInfo.VehicleType.Balloon ||
                info.m_vehicleType == VehicleInfo.VehicleType.Helicopter ||
                info.m_vehicleType == VehicleInfo.VehicleType.Blimp ||
                info.m_vehicleType == VehicleInfo.VehicleType.Rocket)
            {
                return vgo.AddComponent<VehicleHeli>();
            }
            if (
                info.m_vehicleType == VehicleInfo.VehicleType.Train ||
                info.m_vehicleType == VehicleInfo.VehicleType.Monorail ||
                info.m_vehicleType == VehicleInfo.VehicleType.Metro ||
                info.m_vehicleType == VehicleInfo.VehicleType.Tram)
            {
                return vgo.AddComponent<VehicleTrain>();
            }
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
                info.m_vehicleType == VehicleInfo.VehicleType.Bicycle ||
                (info.m_vehicleType == VehicleInfo.VehicleType.Car && fronts == 1 && rears == 1))
            {
                return vgo.AddComponent<VehicleBike>();
            }
            if (fronts + rears > 2 &&
                (info.m_vehicleType == VehicleInfo.VehicleType.Car ||
                info.m_vehicleType == VehicleInfo.VehicleType.Trolleybus))
            {
                return vgo.AddComponent<VehicleCar>();
            }
            else
            {
                return vgo.AddComponent<VehicleGeneric>();
            }
        }

        public Rigidbody GetRigidbody()
        {
            return m_vehicleRigidBody;
        }

        public BoxCollider GetBoxCollider()
        {
            return m_vehicleCollider;
        }

        public void Initialize(Vector3 position, Quaternion rotation, VehicleInfo vehicleInfo, Vehicle.Flags vehicleFlags, Color vehicleColor, bool setColor, bool setPrimary = false)
        {
            if (setPrimary) s_primaryVehicle = this;

            m_vehicleColor = vehicleColor;
            m_vehicleColor.a = 0; // Make sure blinking is not set.
            m_prevPosition = position;
            m_prevVelocity = Vector3.zero;
            m_prevPrevVelocity = Vector3.zero;
            m_vehicleInfo = vehicleInfo;
            m_vehicleFlags = vehicleFlags;
            m_gear = m_gearNeutral;
            m_driveMode = ENGINE_MODE_NEUTRAL;
            m_distanceTravelled = 0.0f;
            m_steer = 0.0f;
            m_brake = 0.0f;
            m_throttle = 0.0f;
            m_nextGearChange = 0.0f;

            Mesh vehicleMesh = m_vehicleInfo.m_mesh;
            RigidbodyConstraints constraints = RigidbodyConstraints.None;
            Vector3 adjustedBounds = m_vehicleInfo.m_lodMesh.bounds.size;
            float adjustedY = m_vehicleInfo.m_lodMesh.bounds.min.y;
            float adjustedZ = m_vehicleInfo.m_lodMesh.bounds.min.z;
            float frontTorque = 0.0f;
            float rearTorque = 0.0f;
            float frontBraking = 0.0f;
            float rearBraking = 0.0f;

            InitializeInternal(ref adjustedBounds, ref adjustedY, ref adjustedZ, ref constraints);

            m_boundMin = Mathf.Min(0.0f, adjustedY);

            if (rearCount == 0 && frontCount > 0)
            {
                frontTorque = 1.0f / frontCount;
                frontBraking = brakingForce * DriveCommon.KN_TO_N / frontCount;
            }
            else if (frontCount == 0 && rearCount > 0)
            {
                rearTorque = 1.0f / rearCount;
                rearBraking = brakingForce * DriveCommon.KN_TO_N / rearCount;
            }
            else if (frontCount > 0 && rearCount > 0)
            {
                frontTorque = driveBias / frontCount;
                rearTorque = (1.0f - driveBias) / rearCount;
                frontBraking = brakeBias * brakingForce * DriveCommon.KN_TO_N / frontCount;
                rearBraking = (1.0f - brakeBias) * brakingForce * DriveCommon.KN_TO_N / rearCount;
            }

            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isFront)
                {
                    w.AdjustWheel(frontTorque, frontBraking, frontTorque > 0.0f ? s_drag_wheel_powered : s_drag_wheel);
                }
                else
                {
                    w.AdjustWheel(rearTorque, rearBraking, rearTorque > 0.0f ? s_drag_wheel_powered : s_drag_wheel);
                }
            }

            float halfSA = (adjustedBounds.x * adjustedBounds.y + adjustedBounds.x * adjustedBounds.z + adjustedBounds.y * adjustedBounds.z);
            m_vehicleRigidBody.drag = vehicleDrag * adjustedBounds.x * adjustedBounds.y / halfSA;
            m_vehicleRigidBody.angularDrag = vehicleDrag * adjustedBounds.y * adjustedBounds.z / halfSA;
            m_vehicleRigidBody.mass = halfSA * MASS_FACTOR;
            m_vehicleRigidBody.transform.position = position;
            m_vehicleRigidBody.transform.rotation = rotation;
            m_vehicleRigidBody.centerOfMass = new Vector3(0.0f, adjustedY + adjustedBounds.y * massCenterHeight, adjustedZ + massCenterBias * adjustedBounds.z);
            m_vehicleRigidBody.velocity = Vector3.zero;
            m_vehicleRigidBody.maxDepenetrationVelocity = DEPEN_VELOCITY;
            m_vehicleRigidBody.constraints = constraints;

            m_vehicleCollider.size = adjustedBounds;
            m_vehicleCollider.center = new Vector3(0.0f, adjustedY + 0.5f * adjustedBounds.y, adjustedZ + adjustedBounds.z * 0.5f);

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
            m_vehicleInfo = null;
            m_prevPosition = Vector3.zero;
            m_prevVelocity = Vector3.zero;
            m_prevPrevVelocity = Vector3.zero;
            m_isTurning = false;
            m_gear = m_gearNeutral;
            m_driveMode = ENGINE_MODE_NEUTRAL;
            m_lastReset = 0.0f;
            m_distanceTravelled = 0.0f;
            m_steer = 0.0f;
            m_brake = 0.0f;
            m_throttle = 0.0f;
            m_nextGearChange = 0.0f;
            m_radps = 0.0f;
            m_prevRadps = 0.0f;
            m_radpsTrans = 0.0f;
            m_boundMin = 0.0f;
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
        protected virtual float vehicleDrag { get => DRAG_FACTOR; }
        protected virtual float steerMax { get => STEER_MAX; }
        protected virtual float momentWheel { get => MOMENT_WHEEL; }
        protected virtual float parkSpeed { get => PARK_SPEED; }

        protected void InitializeFallbackWheels(float y, float width, float length)
        {

        }

        // Initialize the vehicle wheel configuration, calculate hitbox parameters, and configure constriants
        protected virtual void InitializeInternal(ref Vector3 adjustedBounds, ref float adjustedY, ref float adjustedZ, ref RigidbodyConstraints constraints)
        {
            float width = adjustedBounds.x;
            float length = adjustedBounds.z;
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(width * 0.5f, adjustedY + springOffset + RADIUS_D_WHEEL, length * 0.5f), momentWheel, RADIUS_D_WHEEL, true, true));
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(-width * 0.5f, adjustedY + springOffset + RADIUS_D_WHEEL, length * 0.5f), momentWheel, RADIUS_D_WHEEL, true, true));
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(0.0f, adjustedY + springOffset, 0.0f), momentWheel, RADIUS_D_WHEEL, true, false));
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(width * 0.5f, adjustedY + springOffset + RADIUS_D_WHEEL, -length * 0.5f), momentWheel, RADIUS_D_WHEEL, true, true, true));
            m_wheelObjects.Add(Wheel.InstanceWheel(this, new Vector3(-width * 0.5f, adjustedY + springOffset + RADIUS_D_WHEEL, -length * 0.5f), momentWheel, RADIUS_D_WHEEL, true, true, true));
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
                    coeffZ = Mathf.Lerp(ModSettings.GripCoeffS, ModSettings.GripCoeffK, Mathf.Max((wheel.wheelSlip - GRIP_OPTIM_SLIP) / (1.0f - GRIP_OPTIM_SLIP), 0.0f));

                    // boost in lateral friction on lower TCS levels or steerables so the cars feel less slidy (unrealistic)
                    float lateralCoeffK = ModSettings.GripCoeffK;
                    lateralCoeffK = (ModSettings.TCSLevel <= (int)DriveCommon.TRACTIONCTL_LEVEL.SPORT) ? (ModSettings.GripCoeffK * 0.75f + ModSettings.GripCoeffS * 0.25f) : lateralCoeffK;
                    lateralCoeffK = wheel.isSteerable ? ModSettings.GripCoeffS : lateralCoeffK;

                    coeffX = Mathf.Lerp(ModSettings.GripCoeffS, lateralCoeffK, Mathf.Max((wheel.wheelSlip - GRIP_OPTIM_SLIP) / (1.0f - GRIP_OPTIM_SLIP), 0.0f));
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
                m_distanceTravelled += w.wheelRadps * w.wheelTorqueFract * w.wheelRadius * Time.fixedDeltaTime;

                // apply wheel drag from previous tick
                w.ApplyDrag();

                engineRps += w.wheelRadps * w.wheelTorqueFract;
            }

            m_radpsTrans = engineRps;

            if (m_gear == m_gearNeutral)
            {
                engineRps = m_throttle * ENGINE_PEAK_RPS;
            }
            else
            {
                engineRps *= m_gearRatios[m_gear];
            }

            m_prevRadps = m_radps;
            engineRps = Mathf.Clamp(engineRps, ENGINE_IDLE_RPS, ENGINE_OVER_RPS);
            m_radps = Mathf.Lerp(engineRps, m_radps, s_engine_inertia);
        }

        // Torque curve.
        protected virtual float GetTorque(float radps) // Torque curve 27x(k-x)/(4k^3)+max(3(k/2-x)^3/k^4,0)
        {
            // Check https://www.desmos.com/calculator/fp0csjaazj for formulation.
            float k = ENGINE_PEAK_RPS;
            float x = Mathf.Max(radps, ENGINE_IDLE_RPS);
            float rawval = 27.0f * x * (k - x) / (4.0f * k * k * k) + Mathf.Max(3 * Mathf.Pow(k * 0.5f - x, 3.0f) / (k * k * k * k), 0.0f);
            return enginePower * DriveCommon.KW_TO_W * (1.0f - DRAG_DRIVETRAIN) * rawval;
        }

        // Power curve. Should not be modified. Derived from torque curve.
        protected float GetPower(float radps) // Power curve 27x^2(k-x)/(4k^3)
        {
            return GetTorque(radps) * Mathf.Max(radps, ENGINE_IDLE_RPS);
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
                                wheelTorque *= 1.0f - Mathf.Min(w.wheelSlip / GRIP_OPTIM_SLIP, 1.0f);
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
                float totalBrake = (w.wheelSlip < GRIP_OPTIM_SLIP * 0.75f || !ModSettings.BrakingABS || !w.isOnGround) ? m_brake : 0.0f;

                wheelTorque -= Mathf.Sign(w.wheelRadps) * Mathf.Min(totalBrake * w.wheelBrakeFract * w.wheelRadius, Mathf.Abs(w.wheelRadps) * w.wheelMoment / Time.fixedDeltaTime);

                w.AddVelocity(wheelTorque * Time.fixedDeltaTime / w.wheelMoment);
            }
        }

        // PostProcess function that runs after all physics calculations.
        protected virtual void PhysicsPostProcess(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel, Vector3 upVec, Vector3 forwardVec)
        {

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

            if (s_primaryVehicle == null)
            {
                s_engine_inertia = (float)System.Math.Pow(ENGINE_INERTIA, Time.fixedDeltaTime);
                s_drag_wheel_powered = (float)(1.0 - System.Math.Pow(1.0 - DRAG_WHEEL_POWERED, Time.fixedDeltaTime));
                s_drag_wheel = (float)(1.0 - System.Math.Pow(1.0 - DRAG_WHEEL, Time.fixedDeltaTime));
                s_primaryVehicle = this;
            }
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

            HandleInputOnFixedUpdate(invert);

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
                string uiString = "v: " + this +
                                  "\ndm: " + m_driveMode +
                                  "\ng: " + (m_gear - 1) +
                                  "\nt: " + m_throttle +
                                  "\nb: " + m_brake +
                                  "\nrps: " + m_radps + "\trpst: " + m_radpsTrans +
                                  "\nwct: " + frontCount + " " + rightCount + " " + wheelCount;
                for (int index = 0; index < m_wheelObjects.Count; index++)
                {
                    uiString += "\nw" + index + ": " + m_wheelObjects[index].wheelOrigin + "\t " + m_wheelObjects[index].wheelCompression + "\t " + m_wheelObjects[index].wheelRadps;
                }

                GUIStyle m_style = new GUIStyle(GUI.skin.label);
                m_style.fontSize = 20;
                m_style.normal.textColor = Color.white;

                GUI.Label(new Rect(100f, 100f, 700f, 700f), uiString, m_style);
            }
        }

        private void WheelPhysics(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel)
        {
            Vector3 upVec = m_vehicleRigidBody.transform.TransformDirection(Vector3.up).normalized;
            Vector3 forwardVec = m_vehicleRigidBody.transform.TransformDirection(Vector3.forward).normalized;

            PhysicsPreProcess(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);

            m_vehicleRigidBody.AddForce(Vector3.down * (ACCEL_G * m_vehicleRigidBody.mass) - upVec * downForce * Mathf.Abs(Vector3.Dot(vehicleVel, forwardVec)), ForceMode.Force);

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
            PhysicsFeedbackWheelAndEngine(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel, upVec, forwardVec);
            if (ModSettings.AutoTrans)
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

                    Vector2 flatImpulses = Vector2.zero;

                    float longSpeed = Vector3.Dot(w.wheelContactVelocity, w.wheelGroundTangent);
                    float lateralSpeed = Vector3.Dot(w.wheelContactVelocity, w.wheelGroundBinormal);

                    float longComponent = w.wheelMoment * (w.wheelRadps - longSpeed / w.wheelRadius) / w.wheelRadius;
                    if (Mathf.Abs(longSpeed) < 1.0f || Mathf.Abs(w.wheelRadps) < 0.1f) // exaggerated torque at low speeds for better stop and start
                    {
                        longComponent = normalContribution * m_vehicleRigidBody.mass * (w.wheelRadps * w.wheelRadius - longSpeed);
                    }

                    flatImpulses.y = longComponent;

                    float lateralComponent = -normalContribution * m_vehicleRigidBody.mass * lateralSpeed;

                    flatImpulses.x = lateralComponent;

                    if (w.wheelOptimSlip > 0.0f)
                    {
                        DebugHelper.DrawDebugMarker(2.0f, w.wheelContactPoint, Quaternion.LookRotation(w.wheelGroundTangent, w.wheelGroundNormal), Color.yellow);
                    }

                    float frictionScaleLong;
                    float frictionScaleLat;

                    float flatMagniutde = flatImpulses.magnitude;
                    frictionScaleLong = Mathf.Min(w.wheelNormalImpulse * w.wheelFrictionCoeffZ, flatMagniutde) / Mathf.Max(flatMagniutde, DriveCommon.FLOAT_ERROR);
                    frictionScaleLat = Mathf.Min(w.wheelNormalImpulse * w.wheelFrictionCoeffX, flatMagniutde) / Mathf.Max(flatMagniutde, DriveCommon.FLOAT_ERROR);

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

        private void HandleInputOnFixedUpdate(int invert)
        {
            bool throttling = false;
            bool braking = false;
            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyMoveForward.Key))
            {
                if (ModSettings.AutoTrans)
                {
                    if (invert < 0)
                    {
                        if (m_driveMode <= ENGINE_MODE_NEUTRAL)
                        {
                            m_throttle = 0.0f;
                            m_brake = Mathf.Clamp01(m_brake + Time.fixedDeltaTime * THROTTLE_RESP);
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
                        throttling = true;
                    }
                }
                else // ManualTrans
                {
                    m_throttle = Mathf.Clamp01(m_throttle + Time.fixedDeltaTime * THROTTLE_RESP);
                    throttling = true;
                }

            }
            if (Input.GetKey((KeyCode)Settings.ModSettings.KeyMoveBackward.Key))
            {
                if (ModSettings.AutoTrans)
                {
                    if (invert > 0)
                    {
                        if (m_driveMode >= ENGINE_MODE_NEUTRAL)
                        {
                            m_throttle = 0.0f;
                            m_brake = Mathf.Clamp01(m_brake + Time.fixedDeltaTime * THROTTLE_RESP);
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
                        throttling = true;
                    }
                }
                else // ManualTrans
                {
                    m_brake = Mathf.Clamp01(m_brake + Time.fixedDeltaTime * THROTTLE_RESP);
                    braking = true;
                }
            }
            if (ModSettings.AutoTrans && (invert == 0 || m_gear == m_gearNeutral) && m_throttle == 0.0f)
            {
                if (Time.time > m_nextGearChange)
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
        }

        private void HandleInputOnUpdate()
        {
            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyResetVehicle.Key) && m_lastReset + RESET_FREQ < Time.time)
            {
                Quaternion rot = Quaternion.LookRotation(m_vehicleRigidBody.transform.TransformDirection(Vector3.forward));
                Vector3 pos = m_vehicleRigidBody.transform.position;
                pos.y = MapUtils.CalculateHeight(pos, RESET_SCAN_HEIGHT, out var _) - m_boundMin + RESET_HEIGHT;
                m_vehicleRigidBody.transform.SetPositionAndRotation(pos, rot);
                m_lastReset = Time.time;
            }
            if (!ModSettings.AutoTrans)
            {
                if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyGearUp.Key) && m_nextGearChange < Time.time && m_gear < m_gearRatios.Length - 1)
                {
                    m_gear += 1;
                    m_nextGearChange = Time.time + GEAR_RESP;
                }
                if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyGearDown.Key) && m_nextGearChange < Time.time && m_gear > 0)
                {
                    m_gear -= 1;
                    m_nextGearChange = Time.time + GEAR_RESP;
                }
            }
        }
        private void RegisterWheel(Wheel w)
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

        private void DeRegisterWheel(Wheel w)
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
    }
}
