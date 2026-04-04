using AlgernonCommons;
using ColossalFramework;
using DriveIt.Settings;
using DriveIt.UI;
using DriveIt.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DriveIt
{
    public class DriveController : MonoBehaviour
    {
        private const float RESET_SCAN_HEIGHT = 2.0f;
        private const float RESET_HEIGHT = 0.5f;
        private const float RESET_FREQ = 2.0f;
        private const float THROTTLE_RESP = 2.0f;
        private const float THROTTLE_REST = 2.0f;
        private const float STEER_RESP = 1.75f;
        private const float STEER_REST = 1.75f;
        private const float STEER_TILT_INERTIA = 0.1f;
        private const float STEER_TILT_STRENGTH = 4.5f;
        private const float STEER_SWAY_BAR_K = 60000.0f;
        private const float GEAR_RESP = 0.2f;
        private const float GEAR_RESP_AUTO = 1.0f;
        private const float PARK_SPEED = 0.25f;
        private const float STEER_MAX = 37.0f;
        private const float STEER_DECAY = 0.0075f;
        private const float DRAG_FACTOR = 0.25f;
        private const float DRAG_DRIVETRAIN = 0.15f;
        private const float DRAG_FREEZE = 0.9f;
        private const float DRAG_WHEEL_POWERED = 0.25f;
        private const float DRAG_WHEEL = 0.15f;
        private const float MOMENT_WHEEL = 1.5f;
        private const float RADIUS_D_WHEEL = 0.2f;
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
        private static readonly float[] ENGINE_GEAR_RATIOS = { -20.0f, 0.0f, 27.0f, 15.0f, 8.5f, 5.67f, 4.25f, 3.4f, 2.83f, 2.43f };
        private static readonly string[] ENGINE_GEAR_NAMES = { "R", "N", "1", "2", "3", "4", "5", "6", "7", "8" };
        private const int ENGINE_GEAR_REVERSE = 0;
        private const int ENGINE_GEAR_NEUTRAL = 1;
        private const int ENGINE_GEAR_FORWARD_START = 2;
        private const int ENGINE_GEAR_FORWARD_END = 9;
        private const int ENGINE_MODE_REVERSE = -1;
        private const int ENGINE_MODE_NEUTRAL = 0;
        private const int ENGINE_MODE_FORWARD = 1;
        private const float ENGINE_AUTO_SHIFT_THRESH = 0.1f;
        private const float ACCEL_G = 10f;
        private const float UI_SIZE = 1.0f / 4.0f;
        private const float UI_OFFSET = 1.0f / 12.0f;
        private const float UI_FILL = 1.0f;
        private static readonly Color UI_BG_COLOR = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        private static readonly Color UI_FG_COLOR = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        private static float s_engine_inertia;
        private static float s_steer_tilt_inertia;
        private static float s_drag_wheel_powered;
        private static float s_drag_wheel;

        public class Wheel : MonoBehaviour
        {
            public static int wheelCount { get => wheels; }
            public static int frontCount { get => fronts; }
            public static int rearCount { get => wheels - fronts; }
            public static int rightCount { get => rights; }
            public static int leftCount { get => wheels - rights; }

            private static int wheels = 0;
            private static int fronts = 0;
            private static int rights = 0;

            public bool isOnGround { get => onGround; }
            public MapUtils.COLLISION_TYPE wheelGroundType { get => groundType; }
            public Vector3 wheelGroundNormal { get => normal; }
            public Vector3 wheelGroundTangent { get => tangent; }
            public Vector3 wheelGroundBinormal { get => binormal; }
            public Vector3 wheelContactPoint { get => contactPoint; }
            public Vector3 wheelContactVelocity { get => contactVelocity; }
            public float wheelSlip { get => slip; }
            public float wheelOptimSlip { get => slip - GRIP_OPTIM_SLIP; }
            public float wheelHighSlip { get => slip - GRIP_HIGH_SLIP; }
            public float wheelCompression { get => compression; }
            public Vector3 wheelOrigin { get => origin; }
            public float wheelRadius { get => radius; }
            public float wheelMoment { get => moment; }
            public bool isPowered { get => powered && (torqueFract > 0.0f); }
            public bool isSteerable { get => steerable; }
            public bool isInvertedSteer { get => inverted; }
            public bool isFront { get => front; }
            public bool isRight { get => right; }

            internal Vector3 tangent;
            internal Vector3 binormal;
            internal Vector3 normal;
            internal Vector3 heightSample;
            internal Vector3 contactPoint;
            internal Vector3 contactVelocity;
            internal float torqueFract;
            internal float radps;
            internal float brakeForce;
            internal float normalFract;
            internal float normalImpulse;
            internal float binormalImpulse;
            internal float tangentImpulse;
            internal float compression;
            internal float frictionCoeffX;
            internal float frictionCoeffZ;
            internal float slip;
            internal bool onGround;
            internal MapUtils.COLLISION_TYPE groundType;

            private Vector3 origin;
            private float radius;
            private float moment;
            private bool powered;
            private bool steerable;
            private bool inverted;
            private bool front;
            private bool right;
            private bool registered;
            public static Wheel InstanceWheel(Transform parent, Vector3 localpos, float moment, float radius, bool isPowered = true, float torque = 0.0f, float brakeForce = 0.0f, bool isSteerable = false, bool isInvertedSteer = false)
            {
                GameObject wheelObject = new GameObject("Wheel");
                Wheel w = wheelObject.AddComponent<Wheel>();
                wheelObject.transform.SetParent(parent);
                wheelObject.transform.localPosition = localpos;
                w.tangent = Vector3.zero;
                w.binormal = Vector3.zero;
                w.normal = Vector3.zero;
                w.heightSample = Vector3.zero;
                w.contactPoint = Vector3.zero;
                w.contactVelocity = Vector3.zero;
                w.origin = localpos;
                w.moment = moment;
                w.radius = radius;
                w.torqueFract = torque;
                w.radps = 0.0f;
                w.brakeForce = brakeForce;
                w.normalFract = 0.0f;
                w.normalImpulse = 0.0f;
                w.binormalImpulse = 0.0f;
                w.tangentImpulse = 0.0f;
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
                w.registered = false;

                w.Register();

                return w;
            }

            public void OnEnable() // Register is called manually in InstanceWheel
            {
            }

            public void OnDisable()
            {
                DeRegister();
            }

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

            private void Register()
            {
                if (!registered)
                {
                    wheels++;

                    if (isFront)
                    {
                        fronts++;
                    }
                    if (isRight)
                    {
                        rights++;
                    }
                    registered = true;
                }
            }

            private void DeRegister()
            {
                if (registered)
                {
                    wheels--;

                    if (isFront)
                    {
                        fronts--;
                    }
                    if (isRight)
                    {
                        rights--;
                    }
                    registered = false;
                }
            }
        }

        public static DriveController instance { get; private set; }

        private Rigidbody m_vehicleRigidBody;
        private RigidbodyConstraints m_vehicleConstraints;
        private BoxCollider m_vehicleCollider;
        private Color m_vehicleColor;
        private bool m_setColor;
        private VehicleInfo m_vehicleInfo;

        private List<Wheel> m_wheelObjects = new List<Wheel>();

        private DriveColliders m_collidersManager = new DriveColliders();
        private Vector3 m_prevPosition;
        private Vector3 m_prevVelocity;
        private Vector3 m_prevPrevVelocity;
        private Vector3 m_tangent;
        private Vector3 m_binormal;
        private Vector3 m_normal;
        private bool m_isTurning = false;
        private bool m_isConstrainedX = false;
        private bool m_isConstrainedZ = false;
        private bool m_isFallbackSuspension = false;

        private int m_gear = ENGINE_GEAR_NEUTRAL;
        private int m_driveMode = ENGINE_MODE_NEUTRAL;
        private float m_lastReset = 0.0f;
        private float m_distanceTravelled = 0.0f;
        private float m_steer = 0.0f;
        private float m_brake = 0.0f;
        private float m_throttle = 0.0f;
        private float m_rideHeight = 0.0f;
        private float m_roofHeight = 0.0f;
        private float m_nextGearChange = 0.0f;
        private float m_radps = 0.0f;
        private float m_prevRadps = 0.0f;
        private float m_radpsTrans = 0.0f;
        private float m_tilt = 0.0f;

        public float odometer { get => m_distanceTravelled; }
        public float speedometer { get => m_vehicleRigidBody.velocity.magnitude * DriveCommon.MS_TO_KMPH / ModSettings.MaxVelocity; }
        public float tachometer { get => m_radps / ENGINE_PEAK_RPS; }
        public float steer { get => m_steer * STEER_MAX; }
        public float brake { get => m_brake; }
        public float throttle { get => m_throttle; }
        public string gear { get => ENGINE_GEAR_NAMES[m_gear]; }
        public float speed { get => m_vehicleRigidBody.velocity.magnitude; }
        public Vector3 velocity { get => m_vehicleRigidBody.velocity; }
        public Vector3 acceleration { get => (m_prevVelocity - m_prevPrevVelocity) / Time.fixedDeltaTime; }
        public float rpm { get => m_radps * DriveCommon.RPS_TO_RPM; }
        public float radps { get => m_radps; }
        public float angularAcceleration { get => (m_radps - m_prevRadps) / Time.fixedDeltaTime; }
        public bool inlineWheels { get => m_isConstrainedZ; }
        public bool parallelWheels {  get => m_isConstrainedX; }
        public bool fallbackWheels { get => m_isFallbackSuspension; }
        public List<Wheel> wheels { get => m_wheelObjects; }

        public bool OnEsc()
        {
            if (enabled)
            {
                StopDriving();
                return true;
            }
            return false;
        }

        public void UpdateColor(Color color, bool enable)
        {
            m_vehicleColor = color;
            m_setColor = enable;
        }

        public void UpdateVehicleInfo(VehicleInfo info)
        {
            m_vehicleInfo = info;
        }

        public bool IsVehicleInfoSet()
        {
            return m_vehicleInfo != null;
        }

        public void StartDriving(Vector3 position, Quaternion rotation) => StartDriving(position, rotation, m_vehicleInfo, m_vehicleColor, m_setColor);

        public void StartDriving(Vector3 position, Quaternion rotation, VehicleInfo vehicleInfo, Color vehicleColor, bool setColor)
        {
            Vector3 spawnPosition = position;
            Quaternion spawnRotation = rotation;

            if (enabled)
            {
                spawnPosition = m_vehicleRigidBody.transform.position;
                spawnRotation = m_vehicleRigidBody.transform.rotation;

                DestroyVehicle(false);
            }

            SpawnVehicle(spawnPosition + new Vector3(0.0f, -ModSettings.SpringOffset, 0.0f), spawnRotation, vehicleInfo, vehicleColor, setColor);

            if (!enabled)
            {
                DriveCam.instance.EnableCam(m_vehicleRigidBody, 2.0f * m_vehicleCollider.size.z);
                DriveButtons.instance.SetDisable();
            }

            enabled = true;
        }

        public void StopDriving()
        {
            StartCoroutine(m_collidersManager.DisableColliders());
            DriveCam.instance.DisableCam();
            DriveButtons.instance.SetEnable();
            DestroyVehicle();
            enabled = false;
        }

        private void Awake()
        {
            if (instance)
            {
                Destroy(this);
                return;
            }
            instance = this;

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

            StartCoroutine(m_collidersManager.InitializeColliders());
            gameObject.SetActive(false);
            enabled = false;

            DriveEffects.s_controller = this;
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
            int invert = Mathf.Abs(speed) < PARK_SPEED ? 0 : (speed > 0.0f ? 1 : -1);

            HandleInputOnFixedUpdate(invert);

            WheelPhysics(ref vehiclePos, ref vehicleVel, ref vehicleAngularVel);

            LimitVelocity();

            m_collidersManager.UpdateColliders(m_vehicleRigidBody.transform);
            m_collidersManager.UpdateGroundCollider(m_vehicleCollider);

            m_prevPrevVelocity = m_prevVelocity;
            m_prevVelocity = vehicleVel;
            m_prevPosition = vehiclePos;
        }

        private void LateUpdate()
        {
        }
        private void OnDestroy()
        {
            m_collidersManager.DestroyColliders();
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
                string uiString = "dm: " + m_driveMode +
                                    "\ng: " + (m_gear - 1) +
                                    "\nt: " + m_throttle +
                                    "\nb: " + m_brake +
                                    "\ns: " + m_vehicleRigidBody.velocity.magnitude * DriveCommon.MS_TO_KMPH +
                                    "\nrps: " + m_radps +
                                    "\nrpst: " + m_radpsTrans +
                                    "\nwct: " + Wheel.wheelCount;
                for (int index = 0; index < m_wheelObjects.Count; index++)
                {
                    uiString += "\nw" + index + ": " + m_wheelObjects[index].wheelOrigin + " " + m_wheelObjects[index].slip + " " + m_wheelObjects[index].radps;
                }

                GUIStyle m_style = new GUIStyle(GUI.skin.label);
                m_style.fontSize = 20;
                m_style.normal.textColor = Color.white;

                GUI.Label(new Rect(100f, 100f, 700f, 700f), uiString, m_style);
            }
        }

        private void FeedbackWheelAndEngine()
        {
            float engineRps = 0.0f;

            foreach (Wheel w in m_wheelObjects)
            {
                // record distance travelled from previous tick
                m_distanceTravelled += w.radps * w.torqueFract * w.wheelRadius * Time.fixedDeltaTime;

                // apply wheel drag from previous tick
                w.radps *= 1.0f - (w.isPowered ? s_drag_wheel_powered : s_drag_wheel);

                engineRps += w.radps * w.torqueFract;
            }

            m_radpsTrans = engineRps;

            if (m_gear == ENGINE_GEAR_NEUTRAL)
            {
                engineRps = m_throttle * ENGINE_PEAK_RPS;
            }
            else
            {
                engineRps *= ENGINE_GEAR_RATIOS[m_gear];
            }

            m_prevRadps = m_radps;
            engineRps = Mathf.Clamp(engineRps, ENGINE_IDLE_RPS, ENGINE_OVER_RPS);
            m_radps = Mathf.Lerp(engineRps, m_radps, s_engine_inertia);
        }

        private void FeedForwardWheelAndEngine()
        {
            float engineTorque = GetTorque(m_radps);
            float avgRps = 0.0f;
            int powered = 0;

            engineTorque = (engineTorque > 0.0f ? m_throttle : 1.0f) * ENGINE_GEAR_RATIOS[m_gear] * engineTorque;

            foreach (Wheel w in m_wheelObjects)
            {
                // Find the average wheel speed per differential
                if (w.isPowered)
                {
                    avgRps += w.radps * w.torqueFract;
                    powered++;
                }
            }

            foreach (Wheel w in m_wheelObjects)
            {
                // calcuate wheel angular velocity along with any assists.
                float wheelTorque = w.torqueFract * engineTorque;

                // LSD resist more at higher TCS levels
                float LSDFactor = (ModSettings.TCSLevel >= (int)DriveCommon.TRACTIONCTL_LEVEL.SPORT) ? DIFF_LSD_FACTOR_HIGH : DIFF_LSD_FACTOR_LOW;
                wheelTorque += (avgRps - w.radps) * w.wheelMoment / Time.fixedDeltaTime * LSDFactor * w.torqueFract * powered;

                // TCS cut power when on ground, accelerating, and slipping
                switch (ModSettings.TCSLevel)
                {
                    case (int)DriveCommon.TRACTIONCTL_LEVEL.FULL:
                        {
                            if (w.radps * wheelTorque > 0.0f)
                            {
                                wheelTorque *= 1.0f - Mathf.Min(w.slip / GRIP_OPTIM_SLIP, 1.0f);
                            }
                        }
                        break;
                    case (int)DriveCommon.TRACTIONCTL_LEVEL.SPORT:
                        {
                            if (w.onGround && w.radps * wheelTorque > 0.0f)
                            {
                                wheelTorque *= 1.0f - Mathf.Min(w.slip, 1.0f);
                            }
                        }
                        break;
                    case (int)DriveCommon.TRACTIONCTL_LEVEL.TRACK:
                        {
                            if (w.onGround && w.radps * wheelTorque > 0.0f)
                            {
                                wheelTorque *= 1.0f - Mathf.Min(w.slip * 0.5f, 1.0f);
                            }
                        }
                        break;
                    case (int)DriveCommon.TRACTIONCTL_LEVEL.OFF:
                    default:
                        break;
                }

                // braking ABS
                float totalBrake = (w.slip < GRIP_OPTIM_SLIP * 0.75f || !ModSettings.BrakingABS || !w.onGround) ? m_brake : 0.0f;

                wheelTorque -= Mathf.Sign(w.radps) * Mathf.Min(totalBrake * w.brakeForce * w.wheelRadius, Mathf.Abs(w.radps) * w.wheelMoment / Time.fixedDeltaTime);

                w.radps += wheelTorque * Time.fixedDeltaTime / w.wheelMoment;
            }
        }

        private void SelectGear()
        {
            if (m_driveMode == ENGINE_MODE_FORWARD)
            {
                int chosenGear = Mathf.Max(m_gear, ENGINE_GEAR_FORWARD_START);
                float currTorque = ENGINE_GEAR_RATIOS[chosenGear] * GetTorque(m_radps);
                float predTransRps = m_radps / (m_gear == ENGINE_GEAR_NEUTRAL ? 1.0f : ENGINE_GEAR_RATIOS[m_gear]);

                for (int gear = ENGINE_GEAR_FORWARD_START; gear <= ENGINE_GEAR_FORWARD_END; gear++)
                {
                    float tmpTorque = ENGINE_GEAR_RATIOS[gear] * GetTorque(predTransRps * ENGINE_GEAR_RATIOS[gear]);
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
                m_gear = ENGINE_GEAR_REVERSE;
            }
            else
            {
                m_gear = ENGINE_GEAR_NEUTRAL;
            }
        }

        private static float GetTorque(float radps) // Torque curve 27x(k-x)/(4k^3)+max(3(k/2-x)^3/k^4,0)
        {
            // Check https://www.desmos.com/calculator/fp0csjaazj for formulation.
            float k = ENGINE_PEAK_RPS;
            float x = Mathf.Max(radps, ENGINE_IDLE_RPS);
            float rawval = 27.0f * x * (k - x) / (4.0f * k * k * k) + Mathf.Max(3 * Mathf.Pow(k * 0.5f - x, 3.0f) / (k * k * k * k), 0.0f);
            return ModSettings.EnginePower * DriveCommon.KW_TO_W * (1.0f - DRAG_DRIVETRAIN) * rawval;
        }

        private static float GetPower(float radps) // Power curve 27x^2(k-x)/(4k^3)
        {
            return GetTorque(radps) * Mathf.Max(radps, ENGINE_IDLE_RPS);
        }

        private void WheelPhysics(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel)
        {
            Vector3 upVec = m_vehicleRigidBody.transform.TransformDirection(Vector3.up);
            Vector3 forwardVec = m_vehicleRigidBody.transform.TransformDirection(Vector3.forward);

            m_vehicleRigidBody.AddForce(Vector3.down * (ACCEL_G * m_vehicleRigidBody.mass) - upVec * ModSettings.DownForce * Mathf.Abs(Vector3.Dot(vehicleVel, forwardVec)), ForceMode.Force);

            bool slipping = false;

            foreach (Wheel w in m_wheelObjects)
            {
                // first calculate the heights and tbn at each wheel.
                Vector3 wheelPos = w.gameObject.transform.position;

                w.CalcRoadState();

                slipping |= w.slip > GRIP_HIGH_SLIP;
            }

            RigidbodyConstraints constraints = m_vehicleConstraints;
            if (vehicleVel.magnitude < 3.0f && m_throttle == 0.0f && m_brake > 0.0f && !slipping)
            {
                Vector3 sideVec = Vector3.Cross(forwardVec, upVec).normalized;
                m_vehicleRigidBody.velocity = m_vehicleRigidBody.velocity - Vector3.Dot(m_vehicleRigidBody.velocity, sideVec) * sideVec;

                if (vehicleVel.magnitude < PARK_SPEED)
                {
                    constraints = constraints | RigidbodyConstraints.FreezeRotationZ;
                }
            }
            m_vehicleRigidBody.constraints = constraints;

            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isSteerable)
                {
                    w.gameObject.transform.localRotation = Quaternion.Euler(0, (w.isInvertedSteer ? -1.0f : 1.0f) * STEER_MAX * m_steer, 0);
                }
                else
                {
                    w.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }

                if (w.onGround)
                {
                    Vector3 prelimContactVel = m_vehicleRigidBody.GetPointVelocity(w.contactPoint);
                    float radDelta = Vector3.Dot(prelimContactVel, w.tangent) / w.wheelRadius - w.radps;
                    w.radps += Mathf.Sign(radDelta) * Mathf.Min(Mathf.Abs(radDelta), w.normalImpulse * w.wheelRadius * w.frictionCoeffZ / w.wheelMoment);
                }

                // calculate fist pass normal impulses. Update wheel suspension position.
                w.onGround = false;
                w.normalImpulse = 0.0f;
                w.slip = 1.0f;
                w.frictionCoeffX = ModSettings.GripCoeffS;
                w.frictionCoeffZ = ModSettings.GripCoeffK;
                float normDotUp = Vector3.Dot(w.normal, upVec);
                if (normDotUp > VALID_INCLINE)
                {
                    Vector3 wheelOrigin = w.wheelOrigin;
                    Vector3 originWheelBottom = m_vehicleRigidBody.transform.TransformPoint(wheelOrigin + Vector3.down * w.wheelRadius);
                    float compression = Mathf.Max(Vector3.Dot(w.heightSample - originWheelBottom, w.normal) / normDotUp, 0.0f);
                    float springVel = (compression - w.compression) / Time.fixedDeltaTime;
                    float deltaVel = -ModSettings.SpringDamp * Mathf.Exp(-ModSettings.SpringDamp * Time.fixedDeltaTime) * (compression + springVel * Time.fixedDeltaTime) + springVel * Mathf.Exp(-ModSettings.SpringDamp * Time.fixedDeltaTime) - springVel;

                    w.gameObject.transform.localPosition = new Vector3(wheelOrigin.x, wheelOrigin.y + compression, wheelOrigin.z);
                    w.compression = compression;

                    if (deltaVel < 0.0f)
                    {
                        w.onGround = true;
                        w.normalImpulse = (-deltaVel) * m_vehicleRigidBody.mass / Wheel.wheelCount;
                        w.contactPoint = w.gameObject.transform.TransformPoint(new Vector3(0.0f, -w.wheelRadius, 0.0f));
                        w.contactVelocity = m_vehicleRigidBody.GetPointVelocity(w.contactPoint);
                        Vector3 flatVel = w.contactVelocity - Vector3.Dot(w.contactVelocity, w.normal) * w.normal;
                        w.slip = Mathf.Clamp01(Vector3.Magnitude(flatVel - (w.radps * w.wheelRadius * w.tangent)) / Mathf.Clamp(flatVel.magnitude, GRIP_SLIP_SPEED_LOW, GRIP_SLIP_SPEED_HIGH));

                        if (w.groundType == MapUtils.COLLISION_TYPE.ROAD)
                        {
                            w.frictionCoeffZ = Mathf.Lerp(ModSettings.GripCoeffS, ModSettings.GripCoeffK, Mathf.Max((w.slip - GRIP_OPTIM_SLIP) / (1.0f - GRIP_OPTIM_SLIP), 0.0f));

                            // boost in lateral friction on lower TCS levels or steerables so the cars feel less slidy (unrealistic)
                            float lateralCoeffK = ModSettings.GripCoeffK;
                            lateralCoeffK = (ModSettings.TCSLevel <= (int)DriveCommon.TRACTIONCTL_LEVEL.SPORT) ? (ModSettings.GripCoeffK * 0.75f + ModSettings.GripCoeffS * 0.25f) : lateralCoeffK;
                            lateralCoeffK = w.isSteerable ? ModSettings.GripCoeffS : lateralCoeffK;

                            w.frictionCoeffX = Mathf.Lerp(ModSettings.GripCoeffS, lateralCoeffK, Mathf.Max((w.slip - GRIP_OPTIM_SLIP) / (1.0f - GRIP_OPTIM_SLIP), 0.0f));
                        }
                    }
                }
                else
                {
                    w.compression = 0.0f;
                    w.gameObject.transform.localPosition = w.wheelOrigin;
                }
            }

            float frontCompression = 0.0f;
            float rearCompression = 0.0f;
            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isFront)
                {
                    frontCompression += w.compression;
                }
                else
                {
                    rearCompression += w.compression;
                }
            }

            if ( Wheel.frontCount > 0)
            {
                frontCompression /= Wheel.frontCount;
            }
            if (Wheel.rearCount > 0)
            {
                rearCompression /= Wheel.rearCount;
            }

            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isFront)
                {
                    w.normalImpulse = Mathf.Max(w.normalImpulse + (w.compression - frontCompression) * STEER_SWAY_BAR_K * Time.fixedDeltaTime, 0.0f);
                }
                else
                {
                    w.normalImpulse += Mathf.Max(w.normalImpulse + (w.compression - rearCompression) * STEER_SWAY_BAR_K * Time.fixedDeltaTime, 0.0f);
                }
            }

            // calculate new engine and wheel angular velocity
            FeedbackWheelAndEngine();
            if (ModSettings.AutoTrans)
            {
                SelectGear();
            }
            FeedForwardWheelAndEngine();

            Vector3 netNetImpulse = Vector3.zero;

            foreach (Wheel w in m_wheelObjects)
            {
                // calculate the lateral and longitudinal forces. Apply all forces.
                if (w.onGround)
                {
                    Vector3 netImpulse = Vector3.zero;

                    float normalContribution = 0.0f;
                    foreach (Wheel wAlt in m_wheelObjects)
                    {
                        normalContribution += Vector3.Dot(w.normal, wAlt.normal) * wAlt.normalImpulse;
                    }
                    normalContribution = w.normalImpulse / normalContribution;

                    Vector2 flatImpulses = Vector2.zero;

                    float longSpeed = Vector3.Dot(w.contactVelocity, w.tangent);
                    float lateralSpeed = Vector3.Dot(w.contactVelocity, w.binormal);

                    float longComponent = w.wheelMoment * (w.radps - longSpeed / w.wheelRadius) / w.wheelRadius;
                    if (Mathf.Abs(longSpeed) < 1.0f || Mathf.Abs(w.radps) < 0.1f) // exaggerated torque at low speeds for better stop and start
                    {
                        longComponent = normalContribution * m_vehicleRigidBody.mass * (w.radps * w.wheelRadius - longSpeed);
                    }

                    flatImpulses.y = longComponent;

                    float lateralComponent = -normalContribution * m_vehicleRigidBody.mass * lateralSpeed;

                    flatImpulses.x = lateralComponent;

                    if (w.slip > GRIP_OPTIM_SLIP)
                    {
                        DebugHelper.DrawDebugMarker(2.0f, w.contactPoint, Quaternion.LookRotation(w.tangent, w.normal), Color.yellow);
                    }

                    float frictionScaleLong = Mathf.Min(w.normalImpulse * w.frictionCoeffZ, flatImpulses.magnitude) / Mathf.Max(flatImpulses.magnitude, DriveCommon.FLOAT_ERROR);
                    float frictionScaleLat = Mathf.Min(w.normalImpulse * w.frictionCoeffX, flatImpulses.magnitude) / Mathf.Max(flatImpulses.magnitude, DriveCommon.FLOAT_ERROR);

                    w.binormalImpulse = lateralComponent * frictionScaleLat;
                    w.tangentImpulse = longComponent * frictionScaleLong;

                    netImpulse += w.normalImpulse * w.normal;
                    netImpulse += w.binormalImpulse * w.binormal;
                    netImpulse += w.tangentImpulse * w.tangent;

                    netNetImpulse += netImpulse;

                    m_vehicleRigidBody.AddForceAtPosition(netImpulse, w.contactPoint, ForceMode.Impulse);
                }
            }

            if (m_isConstrainedZ)
            {
                Vector3 sideVec = Vector3.Cross(m_vehicleRigidBody.transform.forward, Vector3.up).normalized;
                m_tilt = s_steer_tilt_inertia * m_tilt + Mathf.Atan(Vector3.Dot(netNetImpulse / m_vehicleRigidBody.mass, sideVec) / ACCEL_G) * 180.0f / Mathf.PI;
                Quaternion rot = Quaternion.AngleAxis(m_tilt, m_vehicleRigidBody.transform.forward) * Quaternion.LookRotation(m_vehicleRigidBody.transform.forward);
                m_vehicleRigidBody.transform.rotation = rot;
            }
        }

        private void LimitVelocity()
        {
            if (m_vehicleRigidBody.velocity.magnitude > Settings.ModSettings.MaxVelocity / DriveCommon.MS_TO_KMPH)
            {
                m_vehicleRigidBody.AddForce(m_vehicleRigidBody.velocity.normalized * Settings.ModSettings.MaxVelocity / DriveCommon.MS_TO_KMPH - m_vehicleRigidBody.velocity, ForceMode.VelocityChange);
            }
        }

        private void SpawnVehicle(Vector3 position, Quaternion rotation, VehicleInfo vehicleInfo, Color vehicleColor, bool setColor)
        {
            s_engine_inertia = (float)System.Math.Pow(ENGINE_INERTIA, Time.fixedDeltaTime);
            s_steer_tilt_inertia = (float)System.Math.Pow(STEER_TILT_INERTIA, Time.fixedDeltaTime);
            s_drag_wheel_powered = (float)(1.0 - System.Math.Pow(1.0 - DRAG_WHEEL_POWERED, Time.fixedDeltaTime));
            s_drag_wheel = (float)(1.0 - System.Math.Pow(1.0 - DRAG_WHEEL, Time.fixedDeltaTime));

            m_setColor = setColor;
            m_vehicleColor = vehicleColor;
            m_vehicleColor.a = 0; // Make sure blinking is not set.
            m_prevPosition = position;
            m_prevVelocity = Vector3.zero;
            m_prevPrevVelocity = Vector3.zero;
            m_vehicleInfo = vehicleInfo;
            m_driveMode = ENGINE_MODE_NEUTRAL;
            m_distanceTravelled = 0.0f;
            m_steer = 0.0f;
            m_brake = 0.0f;
            m_throttle = 0.0f;
            m_nextGearChange = 0.0f;
            m_isFallbackSuspension = false;

            m_vehicleInfo.CalculateGeneratedInfo();

            Mesh vehicleMesh = m_vehicleInfo.m_mesh;
            Vector3 fullBounds = vehicleMesh.bounds.size;
            Vector3 adjustedBounds = m_vehicleInfo.m_lodMesh.bounds.size;
            m_rideHeight = m_vehicleInfo.m_lodMesh.bounds.min.y;
            if (m_rideHeight < 0.0f) adjustedBounds.y += m_rideHeight;
            m_rideHeight = Mathf.Max(m_rideHeight, 0.0f);

            if (m_vehicleInfo.m_generatedInfo.m_tyres?.Length > 0 && (m_vehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Car || m_vehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Bicycle || m_vehicleInfo.m_vehicleType == VehicleInfo.VehicleType.Trolleybus))
            {
                float height = m_vehicleInfo.m_generatedInfo.m_tyres[0].y;
                if (height > m_rideHeight)
                {
                    adjustedBounds.y -= height - m_rideHeight;
                }
                m_rideHeight = height;

                foreach (Vector4 tirepos in m_vehicleInfo.m_generatedInfo.m_tyres)
                {
                    m_wheelObjects.Add(Wheel.InstanceWheel(gameObject.transform, new Vector3(tirepos.x, tirepos.y + ModSettings.SpringOffset, tirepos.z), MOMENT_WHEEL, Mathf.Min(tirepos.w, 1.0f), true, 0, 0, tirepos.z > 0.0f));
                }

                m_isConstrainedX = Wheel.rearCount == 0 || Wheel.frontCount == 0;
                m_isConstrainedZ = Wheel.rightCount == 0 || Wheel.leftCount == 0;
            }
            else
            {
                m_isFallbackSuspension = true;

                m_wheelObjects.Add(Wheel.InstanceWheel(gameObject.transform, new Vector3(adjustedBounds.x * 0.5f, ModSettings.SpringOffset + RADIUS_D_WHEEL, adjustedBounds.z * 0.5f), MOMENT_WHEEL, RADIUS_D_WHEEL, true, 0, 0, true));
                m_wheelObjects.Add(Wheel.InstanceWheel(gameObject.transform, new Vector3(-adjustedBounds.x * 0.5f, ModSettings.SpringOffset + RADIUS_D_WHEEL, adjustedBounds.z * 0.5f), MOMENT_WHEEL, RADIUS_D_WHEEL, true, 0, 0, true));
                m_wheelObjects.Add(Wheel.InstanceWheel(gameObject.transform, new Vector3(0.0f, ModSettings.SpringOffset + RADIUS_D_WHEEL, 0.0f), MOMENT_WHEEL, RADIUS_D_WHEEL, true, 0, 0, false));
                m_wheelObjects.Add(Wheel.InstanceWheel(gameObject.transform, new Vector3(adjustedBounds.x * 0.5f, ModSettings.SpringOffset + RADIUS_D_WHEEL, -adjustedBounds.z * 0.5f), MOMENT_WHEEL, RADIUS_D_WHEEL, true, 0, 0, true, true));
                m_wheelObjects.Add(Wheel.InstanceWheel(gameObject.transform, new Vector3(-adjustedBounds.x * 0.5f, ModSettings.SpringOffset + RADIUS_D_WHEEL, -adjustedBounds.z * 0.5f), MOMENT_WHEEL, RADIUS_D_WHEEL, true, 0, 0, true, true));
            }

            float frontTorque = 0.0f;
            float rearTorque = 0.0f;
            float frontBraking = 0.0f;
            float rearBraking = 0.0f;
            if (Wheel.rearCount == 0 && Wheel.frontCount > 0)
            {
                frontTorque = 1.0f / Wheel.frontCount;
                frontBraking = Settings.ModSettings.BrakingForce * DriveCommon.KN_TO_N / Wheel.frontCount;
            }
            else if (Wheel.frontCount == 0 && Wheel.rearCount > 0)
            {
                rearTorque = 1.0f / Wheel.rearCount;
                rearBraking = Settings.ModSettings.BrakingForce * DriveCommon.KN_TO_N / Wheel.rearCount;
            }
            else if (Wheel.frontCount > 0 && Wheel.rearCount > 0)
            {
                frontTorque = ModSettings.DriveBias / Wheel.frontCount;
                rearTorque = (1.0f - ModSettings.DriveBias) / Wheel.rearCount;
                frontBraking = ModSettings.BrakeBias * Settings.ModSettings.BrakingForce * DriveCommon.KN_TO_N / Wheel.frontCount;
                rearBraking = (1.0f - ModSettings.BrakeBias) * Settings.ModSettings.BrakingForce * DriveCommon.KN_TO_N / Wheel.rearCount;
            }

            foreach (Wheel w in m_wheelObjects)
            {
                if (w.isFront)
                {
                    w.torqueFract = frontTorque;
                    w.brakeForce = frontBraking;
                }
                else
                {
                    w.torqueFract = rearTorque;
                    w.brakeForce = rearBraking;
                }
            }

            m_roofHeight = adjustedBounds.y;

            float halfSA = (adjustedBounds.x * adjustedBounds.y + adjustedBounds.x * adjustedBounds.z + adjustedBounds.y * adjustedBounds.z);
            m_vehicleRigidBody.drag = DRAG_FACTOR * adjustedBounds.x * adjustedBounds.y / halfSA;
            if (m_isConstrainedZ || m_isConstrainedX)
            {
                m_vehicleRigidBody.angularDrag = DRAG_FREEZE;
            }
            else
            {
                m_vehicleRigidBody.angularDrag = DRAG_FACTOR * adjustedBounds.y * adjustedBounds.z / halfSA;
            }
            m_vehicleRigidBody.mass = halfSA * ModSettings.MassFactor;
            m_vehicleRigidBody.transform.position = position;
            m_vehicleRigidBody.transform.rotation = rotation;
            m_vehicleRigidBody.centerOfMass = new Vector3(0.0f, m_rideHeight + adjustedBounds.y * ModSettings.MassCenterHeight, (ModSettings.MassCenterBias - 0.5f) * adjustedBounds.z * 0.5f);
            m_vehicleRigidBody.velocity = Vector3.zero;
            m_vehicleRigidBody.maxDepenetrationVelocity = 2.0f;

            m_vehicleConstraints = RigidbodyConstraints.None;
            if (m_isConstrainedX)
            {
                m_vehicleConstraints |= RigidbodyConstraints.FreezePositionX;
            }
            if (m_isConstrainedZ)
            {
                m_vehicleConstraints |= RigidbodyConstraints.FreezeRotationZ;
            }

            m_vehicleRigidBody.constraints = m_vehicleConstraints;

            m_vehicleCollider.size = adjustedBounds;
            m_vehicleCollider.center = new Vector3(0.0f, 0.5f * adjustedBounds.y + m_rideHeight, m_vehicleInfo.m_lodMesh.bounds.center.z);

            m_tangent = m_vehicleRigidBody.transform.TransformDirection(Vector3.forward);
            m_normal = m_vehicleRigidBody.transform.TransformDirection(Vector3.up);
            m_binormal = m_vehicleRigidBody.transform.TransformDirection(Vector3.right);

            DriveEffects.instance.UpdateVehicleInfo(vehicleInfo, vehicleColor, setColor);
            DriveEffects.instance.StartEffects();

            gameObject.SetActive(true);
        }

        private void DestroyVehicle(bool disable = true)
        {
            DriveEffects.instance.StopEffects(disable);

            foreach (Wheel w in m_wheelObjects)
            {
                Object.DestroyImmediate(w.gameObject);
            }
            m_wheelObjects.Clear();
            if (disable)
            {
                gameObject.SetActive(false);
            }
            m_vehicleRigidBody.velocity = Vector3.zero;
            m_vehicleRigidBody.angularVelocity = Vector3.zero;

            m_setColor = false;
            m_vehicleColor = default;
            m_vehicleInfo = null;
            m_prevPosition = Vector3.zero;
            m_prevVelocity = Vector3.zero;
            m_prevPrevVelocity = Vector3.zero;
            m_tangent = Vector3.zero;
            m_binormal = Vector3.zero;
            m_normal = Vector3.zero;
            m_isTurning = false;
            m_isConstrainedX = false;
            m_isConstrainedZ = false;
            m_isFallbackSuspension = false;
            m_gear = ENGINE_GEAR_NEUTRAL;
            m_driveMode = ENGINE_MODE_NEUTRAL;
            m_lastReset = 0.0f;
            m_distanceTravelled = 0.0f;
            m_steer = 0.0f;
            m_brake = 0.0f;
            m_throttle = 0.0f;
            m_rideHeight = 0.0f;
            m_roofHeight = 0.0f;
            m_nextGearChange = 0.0f;
            m_radps = 0.0f;
            m_prevRadps = 0.0f;
            m_radpsTrans = 0.0f;
            m_tilt = 0.0f;
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
            if (ModSettings.AutoTrans && (invert == 0 || m_gear == ENGINE_GEAR_NEUTRAL) && m_throttle == 0.0f)
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
                pos.y = MapUtils.CalculateHeight(pos, RESET_SCAN_HEIGHT, out var _) + RESET_HEIGHT;
                m_vehicleRigidBody.transform.SetPositionAndRotation(pos, rot);
                m_lastReset = Time.time;
            }
            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyGearUp.Key) && m_nextGearChange < Time.time && m_gear < ENGINE_GEAR_FORWARD_END)
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
}