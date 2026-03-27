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
        private const float FLOAT_ERROR = 0.01f;
        private const float RESET_SCAN_HEIGHT = 2.0f;
        private const float RESET_HEIGHT = 0.5f;
        private const float RESET_FREQ = 2.0f;
        private const float THROTTLE_RESP = 2.0f;
        private const float THROTTLE_REST = 2.0f;
        private const float STEER_RESP = 1.75f;
        private const float STEER_REST = 1.75f;
        private const float STEER_TILT_INERTIA = 0.1f;
        private const float STEER_TILT_STRENGTH = 4.5f;
        private const float GEAR_RESP = 0.25f;
        private const float PARK_SPEED = 0.5f;
        private const float STEER_MAX = 37.0f;
        private const float STEER_DECAY = 0.0075f;
        private const float LIGHT_HEADLIGHT_INTENSITY = 3.0f;
        private const float LIGHT_HEADLIGHT_RANGE = 125.0f;
        private const float LIGHT_HEADLIGHT_ANGLE = 60.0f;
        private const float LIGHT_TAILLIGHT_INTENSITY = 1.0f;
        private const float LIGHT_TAILLIGHT_IDLE_INTENSITY = 0.5f;
        private const float LIGHT_TAILLIGHT_RANGE = 15.0f;
        private const float LIGHT_TEXTURE_INTENSITY = 5.0f;
        private const float LIGHT_TEXTURE_IDLE_INTENSITY = 0.5f;
        private const float DRAG_FACTOR = 0.25f;
        private const float DRAG_DRIVETRAIN = 0.15f;
        private const float DRAG_FREEZE = 0.9f;
        private const float DRAG_WHEEL_POWERED = 0.05f;
        private const float DRAG_WHEEL = 0.01f;
        private const float MOMENT_WHEEL = 1.5f;
        private const float VALID_INCLINE = 0.5f;
        private const float GRIP_MAX_SLIP = 1.0f;
        private const float GRIP_OPTIM_SLIP = 0.2f;
        private const float DIFF_BIAS_RATIO = 2.5f;
        private const float ENGINE_PEAK_RPS = 900.0f;
        private const float ENGINE_OVER_RPS = 1200.0f;
        private const float ENGINE_IDLE_RPS = 90.0f;
        private const float ENGINE_INERTIA = 0.005f;
        private const float ENGINE_PITCH = 0.15f;
        private static readonly float[] ENGINE_GEAR_RATIOS = { -25.0f, 0.0f, 25.0f, 12.5f, 8.333f, 6.25f, 5.0f, 4.167f, 3.571f, 3.125f };
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
        const float MS_TO_KMPH = 3.6f;
        const float UNIT_TO_M = 25.0f / 54.0f;
        const float M_TO_UNIT = 54.0f / 25.0f;
        const float UNIT_TO_MPH = UNIT_TO_M * 2.23694f;
        const float KN_TO_N = 1000f;
        const float KW_TO_W = 1000f;

        private static float s_engine_inertia;
        private static float s_steer_tilt_inertia;
        private static float s_drag_wheel_powered;
        private static float s_drag_wheel;

        private struct NetInfoBackup
        {
            public NetInfoBackup(NetInfo.Node[] nodes, NetInfo.Segment[] segments)
            {
                this.nodes = nodes;
                this.segments = segments;
            }
            
            public NetInfo.Node[]      nodes;
            public NetInfo.Segment[]   segments;
        }

        private class Wheel : MonoBehaviour
        {
            //public TrailRenderer skidTrail;
            public static int wheelCount { get => wheels; private set => wheels = value; }
            public static int frontCount { get => fronts; private set => fronts = value; }
            public static int rearCount { get => wheels - fronts; }

            private static int wheels = 0;
            private static int fronts = 0;

            public Vector3 tangent;
            public Vector3 binormal;
            public Vector3 normal;
            public Vector3 heightSample;
            public Vector3 contactPoint;
            public Vector3 contactVelocity;
            public Vector3 origin;
            public float moment;
            public float radius;
            public float torqueFract;
            public float radps;
            public float brakeForce;
            public float normalFract;
            public float normalImpulse;
            public float binormalImpulse;
            public float tangentImpulse;
            public float compression;
            public float frictionCoeff;
            public float slip;
            public bool onGround;
            public MapUtils.COLLISION_TYPE groundType;
            public bool isSimulated     { get => simulated; private set => simulated = value; }
            public bool isPowered       { get => powered && (torqueFract > 0.0f); private set => powered = value; }
            public bool isSteerable     { get => steerable; private set => steerable = value; }
            public bool isInvertedSteer { get => inverted; private set => inverted = value; }
            public bool isFront         { get => front; private set => front = value; }

            private bool simulated;
            private bool powered;
            private bool steerable;
            private bool inverted;
            private bool front;
            private bool registered;
            public static Wheel InstanceWheel(Transform parent, Vector3 localpos, float moment, float radius, bool isSimulated = true, bool isPowered = true, float torque = 0.0f, float brakeForce = 0.0f, bool isSteerable = false, bool isInvertedSteer = false)
            {
                GameObject go = new GameObject("Wheel");
                Wheel w = go.AddComponent<Wheel>();
                go.transform.SetParent(parent);
                go.transform.localPosition = localpos;
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
                w.frictionCoeff = ModSettings.GripCoeffK;
                w.slip = 0.0f;
                w.onGround = false;
                w.isSimulated = isSimulated;
                w.isPowered = isPowered;
                w.isSteerable = isSteerable;
                w.isInvertedSteer = isInvertedSteer;
                w.isFront = localpos.z > 0.0f;
                w.registered = false;

                w.Register();

                return w;
            }

            public void OnEnable()
            {
                Register();
            }

            public void OnDisable()
            {
                DeRegister();
            }

            public void CalcRoadState(float heightThresh)
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
                    if (isSimulated)
                    {
                        wheelCount++;
                    }

                    if (isFront)
                    {
                        fronts++;
                    }
                    registered = true;
                }
            }

            private void DeRegister()
            {
                if (registered)
                {
                    if (isSimulated)
                    {
                        wheelCount--;
                    }

                    if (isFront)
                    {
                        fronts--;
                    }
                    registered = false;
                }
            }
        }

        public static DriveController instance { get; private set; }

        private Rigidbody       m_vehicleRigidBody;
        private BoxCollider     m_vehicleCollider;
        private Color           m_vehicleColor;
        private bool            m_setColor;
        private VehicleInfo     m_vehicleInfo;
        private GameObject      m_headlightObject;
        private GameObject      m_taillightObject;
        private Light           m_headlight;
        private Light           m_taillight;
        private GUIStyle        m_speedoStyle;
        private GUIStyle        m_gearStyle;

        private List<Wheel>             m_wheelObjects          = new List<Wheel>();

        private List<LightEffect>       m_lightEffects          = new List<LightEffect>();
        private List<EngineSoundEffect> m_engineSoundEffects    = new List<EngineSoundEffect>();
        private List<EffectInfo>        m_regularEffects        = new List<EffectInfo>();
        private List<EffectInfo>        m_dustEffects           = new List<EffectInfo>();
        private List<EffectInfo>        m_specialEffects        = new List<EffectInfo>();

        private Dictionary<string, string> m_customUndergroundMappings = new Dictionary<string, string>();
        private Dictionary<NetInfo, NetInfoBackup> m_backupPrefabData = new Dictionary<NetInfo, NetInfoBackup>();
        private Material m_backupUndergroundMaterial = null;
        private Material m_glMaterial = null;

        private DriveColliders m_collidersManager = new DriveColliders();
        private Vector3 m_prevPosition;
        private Vector3 m_prevVelocity;
        private Vector3 m_tangent;
        private Vector3 m_binormal;
        private Vector3 m_normal;
        private Vector4 m_lightState;
        private bool m_isSirenEnabled = false;
        private bool m_isLightEnabled = false;
        private bool m_isDusty = false;
        private bool m_isTurning = false;
        private bool m_isConstrainedX = false;
        private bool m_isConstrainedZ = false;

        private int m_gear = ENGINE_GEAR_NEUTRAL;
        private int m_driveMode = ENGINE_MODE_NEUTRAL;
        private float m_lastReset = 0.0f;
        private float m_terrainHeight = 0.0f;
        private float m_distanceTravelled = 0.0f;
        private float m_steer = 0.0f;
        private float m_brake = 0.0f;
        private float m_throttle = 0.0f;
        private float m_rideHeight = 0.0f;
        private float m_roofHeight = 0.0f;
        private float m_compression = 0.0f;
        private float m_prevGearChange = 0.0f;
        private float m_normalImpulse = 0.0f;
        private float m_radps = 0.0f;
        private float m_prevRadps = 0.0f;
        private float m_radpsTrans = 0.0f;
        private float m_tilt = 0.0f;

        private void Awake()
        {
            instance = this;

            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
            gameObject.GetComponent<MeshRenderer>().enabled = true;

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

            m_headlightObject = new GameObject();
            m_headlight = m_headlightObject.AddComponent<Light>();
            m_headlight.type = LightType.Spot;
            m_headlight.intensity = LIGHT_HEADLIGHT_INTENSITY;
            m_headlight.range = LIGHT_HEADLIGHT_RANGE;
            m_headlight.spotAngle = LIGHT_HEADLIGHT_ANGLE;
            m_headlight.transform.parent = m_vehicleRigidBody.transform;
            m_headlight.color = Color.white;
            m_headlight.enabled = false;

            m_taillightObject = new GameObject();
            m_taillight = m_taillightObject.AddComponent<Light>();
            m_taillight.type = LightType.Point;
            m_taillight.intensity = LIGHT_TAILLIGHT_IDLE_INTENSITY;
            m_taillight.range = LIGHT_TAILLIGHT_RANGE;
            m_taillight.transform.parent = m_vehicleRigidBody.transform;
            m_taillight.color = Color.red;
            m_taillight.enabled = false;

            StartCoroutine(m_collidersManager.InitializeColliders());
            gameObject.SetActive(false);
            enabled = false;

            // Some tunnel names are atypical and need to be manually mapped.
            m_customUndergroundMappings["HighwayRamp Tunnel"]                        = "HighwayRampElevated";
            m_customUndergroundMappings["Metro Track"]                               = "Metro Track Elevated 01";
            m_customUndergroundMappings["Metro Station Track"]                       = "Metro Station Track Elevated 01";
            m_customUndergroundMappings["Large Oneway Road Tunnel"]                  = "Large Oneway Elevated";
            m_customUndergroundMappings["Metro Station Below Ground Bypass"]         = "Metro Station Track Elevated Bypass";
            m_customUndergroundMappings["Metro Station Below Ground Dual Island"]    = "Metro Station Track Elevated Dual Island";
            m_customUndergroundMappings["Metro Station Below Ground Island"]         = "Metro Station Track Elevated Island Platform";

            m_speedoStyle = new GUIStyle();
            m_speedoStyle.fontSize = (int) (Screen.height * UI_SIZE * 0.3f);
            m_speedoStyle.normal.textColor = Color.white;
            m_speedoStyle.alignment = TextAnchor.LowerCenter;
            m_speedoStyle.fontStyle = FontStyle.Italic;
            m_speedoStyle.padding.right = m_speedoStyle.fontSize / 8;
            m_gearStyle = new GUIStyle();
            m_gearStyle.fontSize = (int)(Screen.height * UI_SIZE * 0.2f);
            m_gearStyle.normal.textColor = Color.white;
            m_gearStyle.alignment = TextAnchor.MiddleCenter;
            m_gearStyle.fontStyle = FontStyle.Italic;
            m_gearStyle.padding.right = m_gearStyle.fontSize / 8;


            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_glMaterial = new Material(shader);
            m_glMaterial.hideFlags = HideFlags.HideAndDontSave;
            m_glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m_glMaterial.SetInt("_ZWrite", 0);
        }
        private void Update()
        {
            HandleInputOnUpdate();
            PlayEffects();

            MaterialPropertyBlock materialBlock = Singleton<VehicleManager>.instance.m_materialBlock;
            materialBlock.Clear();
            //materialBlock.SetMatrix(Singleton<VehicleManager>.instance.ID_TyreMatrix, value);
            Vector4 tyrePosition = default;
            tyrePosition.x = m_steer * STEER_MAX / 180.0f * Mathf.PI;
            tyrePosition.y = m_distanceTravelled;
            tyrePosition.z = 0f;
            tyrePosition.w = 0f;
            materialBlock.SetVector(Singleton<VehicleManager>.instance.ID_TyrePosition, tyrePosition);

            m_lightState.x = m_isLightEnabled ? LIGHT_TEXTURE_INTENSITY : 0.0f;
            m_lightState.y = m_brake > 0.0f ? LIGHT_TEXTURE_INTENSITY : (m_isLightEnabled ? LIGHT_TEXTURE_IDLE_INTENSITY : 0.0f);
            materialBlock.SetVector(Singleton<VehicleManager>.instance.ID_LightState, m_lightState);
            if (m_setColor)
            {
                materialBlock.SetColor(Singleton<VehicleManager>.instance.ID_Color, m_vehicleColor);
            }

            m_headlight.enabled = m_isLightEnabled;

            float tailIntensity = m_isLightEnabled ? LIGHT_TEXTURE_IDLE_INTENSITY : 0.0f;
            tailIntensity = m_brake > 0.0f ? LIGHT_TAILLIGHT_INTENSITY : tailIntensity;
            if (tailIntensity > 0.0f)
            {
                m_taillight.intensity = tailIntensity;
                m_taillight.enabled = true;
            }
            else
            {
                m_taillight.enabled = false;
            }

            float avgCompression = 0.0f;
            foreach(Wheel w in m_wheelObjects)
            {
                avgCompression += w.compression;
            }
            avgCompression = ModSettings.SpringOffset + avgCompression / Wheel.wheelCount;
            materialBlock.SetMatrix(Singleton < VehicleManager >.instance.ID_TyreMatrix, Matrix4x4.TRS(new Vector3(0.0f, Mathf.Clamp(avgCompression, ModSettings.SpringOffset, 0.0f), 0.0f), Quaternion.identity, Vector3.one));

            gameObject.GetComponent<MeshRenderer>().SetPropertyBlock(materialBlock);

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

        public bool OnEsc()
        {
            if (enabled)
            {
                StopDriving();
                return true;
            }
            return false;
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
            if (Event.current.type == EventType.Repaint)
            {
                if (Logging.DetailLogging)
                {
                    string uiString = "dm: " + m_driveMode +
                                      "\ng: " + (m_gear - 1) +
                                      "\nt: " + m_throttle +
                                      "\nb: " + m_brake +
                                      "\ns: " + m_vehicleRigidBody.velocity.magnitude * MS_TO_KMPH +
                                      "\nrps: " + m_radps +
                                      "\nrpst: " + m_radpsTrans;
                    for (int index = 0; index < Wheel.wheelCount; index++)
                    {
                        uiString += "\nw" + index + ": " + m_wheelObjects[index].origin + " " + m_wheelObjects[index].slip + " " + m_wheelObjects[index].radps;
                    }

                    GUIStyle m_style = new GUIStyle(GUI.skin.label);
                    m_style.fontSize = 20;
                    m_style.normal.textColor = Color.white;

                    GUI.Label(new Rect(100f, 100f, 700f, 700f), uiString, m_style);
                }

                float sizePx = UI_SIZE * Screen.height;
                float offsetPx = UI_OFFSET * Screen.height;
                float fillPx = sizePx * UI_FILL;
                float borderPx = (sizePx - fillPx) / 2;

                float x = Screen.width - (sizePx + offsetPx);
                float y = Screen.height - (sizePx + offsetPx);


                Rect area = new Rect(x + borderPx, y + borderPx, fillPx, fillPx);
                Rect bgarea = new Rect(x, y, sizePx, sizePx);

                // Background
                GUI.color = UI_BG_COLOR;
                GUI.DrawTexture(bgarea, DriveCommon.driveGaugeCluster);
                GUI.color = UI_FG_COLOR;

                // text
                GUI.Label(area, ENGINE_GEAR_NAMES[m_gear], m_gearStyle);
                GUI.Label(area, Mathf.RoundToInt(m_vehicleRigidBody.velocity.magnitude * MS_TO_KMPH).ToString(), m_speedoStyle);

                // Draw tach arc
                DriveCommon.DrawRingSegment(new Vector2(x + sizePx * 0.5f, y + sizePx * 0.5f),
                    fillPx * 0.5f, fillPx * 0.5f * 0.9f,
                    Mathf.PI * 5.0f / 4.0f, -Mathf.PI / 4.0f,
                    m_radps / ENGINE_PEAK_RPS,
                    UI_FG_COLOR, m_glMaterial);
            }
        }

        private void FeedbackWheelAndEngine()
        {
            float engineRps = 0.0f;

            foreach (Wheel w in m_wheelObjects)
            {
                // record distance travelled from previous tick
                m_distanceTravelled += w.radps * w.torqueFract * w.radius * Time.fixedDeltaTime;

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

            if (m_radps > ENGINE_PEAK_RPS)
            {
                m_throttle = 1.0f;
            }
        }

        private void SelectGear()
        {
            if (m_driveMode == ENGINE_MODE_FORWARD)
            {
                int chosenGear = Mathf.Max(m_gear, ENGINE_GEAR_FORWARD_START);
                float currTorque = ENGINE_GEAR_RATIOS[chosenGear] * GetTorque(m_radpsTrans * ENGINE_GEAR_RATIOS[chosenGear]);

                for (int gear = ENGINE_GEAR_FORWARD_START; gear <= ENGINE_GEAR_FORWARD_END; gear++)
                {
                    float tmpTorque = ENGINE_GEAR_RATIOS[gear] * GetTorque(m_radpsTrans * ENGINE_GEAR_RATIOS[gear]);
                    if (tmpTorque > currTorque * (1.0f + ENGINE_AUTO_SHIFT_THRESH))
                    {
                        chosenGear = gear;
                        currTorque = tmpTorque;
                    }
                }

                if (m_gear != chosenGear && Time.time > m_prevGearChange + GEAR_RESP)
                {
                    m_gear = chosenGear;
                    m_prevGearChange = Time.time;
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

        private float GetTransmissionTorque()
        {
            return ENGINE_GEAR_RATIOS[m_gear] * GetTorque(m_radps);
        }

        private static float GetTorque(float radps) // Torque curve 27x(k-x)/(4k^3)
        {
            // Check https://www.desmos.com/calculator/fp0csjaazj for formulation.
            float k = ENGINE_PEAK_RPS;
            float x = Mathf.Max(radps, ENGINE_IDLE_RPS);
            return ModSettings.EnginePower * KW_TO_W * (1.0f - DRAG_DRIVETRAIN) * 27.0f * x * (k - x) / (4.0f * k * k * k);
        }

        private static float GetPower(float radps) // Power curve 27x^2(k-x)/(4k^3)
        {
            return GetTorque(radps) * Mathf.Max(radps, ENGINE_IDLE_RPS);
        }

        private void WheelPhysics(ref Vector3 vehiclePos, ref Vector3 vehicleVel, ref Vector3 vehicleAngularVel)
        {
            Vector3 upVec = m_vehicleRigidBody.transform.TransformDirection(Vector3.up);
            Vector3 forwardVec = m_vehicleRigidBody.transform.TransformDirection(Vector3.forward);

            // Steering assist
            //if (!m_isTurning)
            //{
            //    float rot = Vector3.Dot(vehicleAngularVel, upVec);
            //    float dir = Vector3.Dot(vehicleVel, forwardVec);

            //    if (Mathf.Abs(rot) >= 0.001 && Mathf.Abs(m_steer) < 0.05f)
            //    {
            //        m_steer -= Mathf.Sign(rot * dir) * Mathf.Min(Mathf.Abs(rot) * 20.0f * Time.fixedDeltaTime, 0.05f);
            //    }
            //}

            m_vehicleRigidBody.AddForce(Vector3.down * (ACCEL_G * m_vehicleRigidBody.mass) - upVec * ModSettings.DownForce * Mathf.Abs(Vector3.Dot(vehicleVel, forwardVec)), ForceMode.Force);

            foreach (Wheel w in m_wheelObjects)
            {
                // first calculate the heights and tbn at each wheel.
                Vector3 wheelPos = w.gameObject.transform.position;

                w.CalcRoadState(m_roofHeight);

                if (wheelPos.y + DriveCommon.ROAD_WALL_HEIGHT < w.heightSample.y)
                {
                    vehiclePos = m_prevPosition;
                    vehicleVel = -vehicleVel * 0.1f;
                    vehicleAngularVel = Vector3.zero;
                    m_vehicleRigidBody.angularVelocity = vehicleAngularVel;
                    m_vehicleRigidBody.velocity = vehicleVel;
                    m_vehicleRigidBody.transform.position = vehiclePos;
                    return;
                }
            }

            foreach (Wheel w in m_wheelObjects) 
            {
                // apply angular friction from previous tick. 
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
                    Vector2 flatImpulses = new Vector2(w.binormalImpulse, w.tangentImpulse);
                    float radDelta = Vector3.Dot(prelimContactVel, w.tangent) / w.radius - w.radps;
                    w.radps += Mathf.Sign(radDelta) * Mathf.Min(Mathf.Abs(radDelta), w.normalImpulse * w.radius * w.frictionCoeff / w.moment);
                }

                // calculate fist pass normal impulses. Update wheel suspension position.
                w.onGround = false;
                w.normalImpulse = 0.0f;
                w.slip = 1.0f;
                w.frictionCoeff = ModSettings.GripCoeffK;
                float normDotUp = Vector3.Dot(w.normal, upVec);
                if (normDotUp > VALID_INCLINE)
                {
                    Vector3 originWheelBottom = m_vehicleRigidBody.transform.TransformPoint(w.origin + Vector3.down * w.radius);
                    float compression = Mathf.Max(Vector3.Dot(w.heightSample - originWheelBottom, w.normal) / normDotUp, 0.0f);
                    float springVel = (compression - w.compression) / Time.fixedDeltaTime;
                    float deltaVel = -ModSettings.SpringDamp * Mathf.Exp(-ModSettings.SpringDamp * Time.fixedDeltaTime) * (compression + springVel * Time.fixedDeltaTime) + springVel * Mathf.Exp(-ModSettings.SpringDamp * Time.fixedDeltaTime) - springVel;

                    w.gameObject.transform.localPosition = new Vector3(w.origin.x, w.origin.y + compression, w.origin.z);
                    w.compression = compression;

                    if (deltaVel < 0.0f)
                    {
                        w.onGround = true;
                        w.normalImpulse = (-deltaVel) * m_vehicleRigidBody.mass / Wheel.wheelCount;
                        w.contactPoint = w.gameObject.transform.TransformPoint(new Vector3(0.0f, -w.radius, 0.0f));
                        w.contactVelocity = m_vehicleRigidBody.GetPointVelocity(w.contactPoint);
                        w.slip = Mathf.Clamp(Vector3.Magnitude(w.contactVelocity - (w.radps * w.radius * w.tangent)) / Mathf.Max(w.contactVelocity.magnitude, 1.0f) / GRIP_MAX_SLIP, 0.0f, 1.0f);
                        w.frictionCoeff = Mathf.Lerp(ModSettings.GripCoeffS, ModSettings.GripCoeffK, Mathf.Max((w.slip - GRIP_OPTIM_SLIP) / (1.0f - GRIP_OPTIM_SLIP), 0.0f));
                    }
                }
                else
                {
                    w.compression = 0.0f;
                    w.gameObject.transform.localPosition = w.origin;
                }
            }

            // calculate new engine angular velocity
            FeedbackWheelAndEngine();
            if (ModSettings.AutoTrans)
            {
                SelectGear();
            }
            float engineTorque = GetTransmissionTorque();


            float maxFrontTorque = Mathf.Abs(engineTorque);
            float maxRearTorque = Mathf.Abs(engineTorque);

            foreach (Wheel w in m_wheelObjects) 
            {
                // Find the maximum torque applicable to a single wheel on the axle
                if (w.isFront)
                {
                    maxFrontTorque = Mathf.Min(DIFF_BIAS_RATIO * w.normalImpulse * w.frictionCoeff * w.radius / Time.fixedDeltaTime, maxFrontTorque);
                }
                else
                {
                    maxRearTorque = Mathf.Min(DIFF_BIAS_RATIO * w.normalImpulse * w.frictionCoeff * w.radius / Time.fixedDeltaTime, maxRearTorque);
                }
            }
            foreach (Wheel w in m_wheelObjects)
            {
                // calcuate wheel angular velocity along with any assists.
                float wheelTorque = Mathf.Abs(engineTorque);
                float wheelTorqueSign = Mathf.Sign(engineTorque);

                if (w.isFront)
                {
                    wheelTorque = Mathf.Min(m_throttle * w.torqueFract * wheelTorque, maxFrontTorque);
                }
                else
                {
                    wheelTorque = Mathf.Min(m_throttle * w.torqueFract * wheelTorque, maxRearTorque);
                }
                wheelTorque *= wheelTorqueSign;

                // TCS cut power when slipping
                wheelTorque *= 1.0f - Mathf.Min(w.slip, 1.0f);

                // braking ABS
                float totalBrake = (w.slip < GRIP_OPTIM_SLIP * 0.75f || !ModSettings.BrakingABS || !w.onGround) ? m_brake : 0.0f;

                wheelTorque -= Mathf.Sign(w.radps) * Mathf.Min(totalBrake * w.brakeForce * w.radius, Mathf.Abs(w.radps) * w.moment / Time.fixedDeltaTime);

                w.radps += wheelTorque * Time.fixedDeltaTime / w.moment;
            }

            Vector3 netNetImpulse = Vector3.zero;
            int dustyWheels = 0;

            foreach (Wheel w in m_wheelObjects)
            {
                // calculate the lateral and longitudinal forces. Apply all forces.
                if (w.onGround)
                {
                    Vector3 netImpulse = Vector3.zero;             

                    float normalContribution = 0.0f;
                    foreach(Wheel wAlt in m_wheelObjects)
                    {
                        normalContribution += Vector3.Dot(w.normal, wAlt.normal) * wAlt.normalImpulse;
                    }
                    normalContribution = w.normalImpulse / normalContribution;

                    Vector2 flatImpulses = Vector2.zero;

                    float longSpeed = Vector3.Dot(w.contactVelocity, w.tangent);
                    float lateralSpeed = Vector3.Dot(w.contactVelocity, w.binormal);

                    float longComponent = w.moment * (w.radps - longSpeed / w.radius) / w.radius;
                    if (longSpeed < 1.0f || w.radps <0.01f) // exaggerated torque at very low speeds for better stop and start
                    {
                        longComponent = Mathf.Lerp(normalContribution * m_vehicleRigidBody.mass * (w.radps * w.radius - longSpeed), longComponent, longSpeed);
                    }

                    flatImpulses.y = longComponent;

                    float lateralComponent = -normalContribution * m_vehicleRigidBody.mass * lateralSpeed;

                    flatImpulses.x = lateralComponent;

                    if (w.slip > GRIP_OPTIM_SLIP)
                    {
                        DebugHelper.DrawDebugMarker(2.0f, w.contactPoint, Quaternion.LookRotation(w.tangent, w.normal), Color.yellow);
                    }

                    // boost in lateral friction so the cars feel less slidy (unrealistic)
                    float frictionScaleLong = Mathf.Min(w.normalImpulse * w.frictionCoeff, flatImpulses.magnitude) / Mathf.Max(flatImpulses.magnitude, FLOAT_ERROR);
                    float frictionScaleLat = Mathf.Min(w.normalImpulse * (w.frictionCoeff + ModSettings.GripCoeffS) * 0.5f, flatImpulses.magnitude) / Mathf.Max(flatImpulses.magnitude, FLOAT_ERROR);

                    w.binormalImpulse = lateralComponent * frictionScaleLat;
                    w.tangentImpulse = longComponent * frictionScaleLong;

                    netImpulse += w.normalImpulse * w.normal;
                    netImpulse += w.binormalImpulse * w.binormal;
                    netImpulse += w.tangentImpulse * w.tangent;

                    netNetImpulse += netImpulse;

                    m_vehicleRigidBody.AddForceAtPosition(netImpulse, w.contactPoint, ForceMode.Impulse);

                    // check for effects
                    if (w.groundType == MapUtils.COLLISION_TYPE.GROUND)
                    {
                        dustyWheels++;
                    }
                }
            }

            m_isDusty = dustyWheels >= Wheel.wheelCount / 2;

            if (m_isConstrainedZ)
            {
                Vector3 sideVec = Vector3.Cross(m_vehicleRigidBody.transform.forward, Vector3.up).normalized;
                Vector3 targetNorm = Vector3.Cross(sideVec, m_vehicleRigidBody.transform.forward).normalized;
                m_tilt = s_steer_tilt_inertia * m_tilt + Mathf.Atan(Vector3.Dot(netNetImpulse / m_vehicleRigidBody.mass, sideVec) / ACCEL_G) * 180.0f / Mathf.PI;
                Quaternion rot = Quaternion.AngleAxis(m_tilt, m_vehicleRigidBody.transform.forward) * Quaternion.LookRotation(m_vehicleRigidBody.transform.forward);
                m_vehicleRigidBody.transform.rotation = rot;
            }
        }

        private void LimitVelocity()
        {
            if (m_vehicleRigidBody.velocity.magnitude > Settings.ModSettings.MaxVelocity / MS_TO_KMPH)
            {
                m_vehicleRigidBody.AddForce(m_vehicleRigidBody.velocity.normalized * Settings.ModSettings.MaxVelocity / MS_TO_KMPH - m_vehicleRigidBody.velocity, ForceMode.VelocityChange);
            }
        }

        public void updateColor(Color color, bool enable)
        {
            m_vehicleColor = color; 
            m_setColor = enable;
        }

        public void updateVehicleInfo(VehicleInfo info)
        {
            m_vehicleInfo = info; 
        }

        public bool isVehicleInfoSet()
        {
            return m_vehicleInfo != null;
        }

        public void StartDriving(Vector3 position, Quaternion rotation) => StartDriving(position, rotation, m_vehicleInfo, m_vehicleColor, m_setColor);
        public void StartDriving(Vector3 position, Quaternion rotation, VehicleInfo vehicleInfo, Color vehicleColor, bool setColor)
        {
            enabled = true;
            SpawnVehicle(position + new Vector3(0.0f, -ModSettings.SpringOffset, 0.0f), rotation, vehicleInfo, vehicleColor, setColor);
            OverridePrefabs();
            DriveCam.instance.EnableCam(m_vehicleRigidBody, 2.0f * m_vehicleCollider.size.z);
            DriveButtons.instance.SetDisable();
        }
        public void StopDriving()
        {
            StartCoroutine(m_collidersManager.DisableColliders());
            DriveCam.instance.DisableCam();
            DriveButtons.instance.SetEnable();
            RestorePrefabs();
            DestroyVehicle();
            enabled = false;
        }
        private void SpawnVehicle(Vector3 position, Quaternion rotation, VehicleInfo vehicleInfo, Color vehicleColor, bool setColor)
        {
            s_engine_inertia = (float) System.Math.Pow(ENGINE_INERTIA, Time.fixedDeltaTime);
            s_steer_tilt_inertia = (float) System.Math.Pow(STEER_TILT_INERTIA, Time.fixedDeltaTime);
            s_drag_wheel_powered = (float) (1.0 - System.Math.Pow(1.0 - DRAG_WHEEL_POWERED, Time.fixedDeltaTime));
            s_drag_wheel = (float) (1.0 - System.Math.Pow(1.0 - DRAG_WHEEL, Time.fixedDeltaTime));

            m_setColor = setColor;
            m_vehicleColor = vehicleColor;
            m_vehicleColor.a = 0; // Make sure blinking is not set.
            m_prevPosition = position;
            m_prevVelocity = Vector3.zero;
            m_lightState = Vector4.zero;
            m_vehicleInfo = vehicleInfo;
            m_driveMode = ENGINE_MODE_NEUTRAL;
            m_terrainHeight = 0.0f;
            m_distanceTravelled = 0.0f;
            m_steer = 0.0f;
            m_brake = 0.0f;
            m_throttle = 0.0f;
            m_compression = 0.0f;
            m_normalImpulse = 0.0f;
            m_prevGearChange = 0.0f;

            m_vehicleInfo.CalculateGeneratedInfo();

            if (m_vehicleInfo.m_generatedInfo.m_tyres?.Length > 0)
            {
                m_rideHeight = m_vehicleInfo.m_generatedInfo.m_tyres[0].y;

                foreach (Vector4 tirepos in m_vehicleInfo.m_generatedInfo.m_tyres)
                {
                    m_wheelObjects.Add(Wheel.InstanceWheel(gameObject.transform, new Vector3(tirepos.x, tirepos.y + ModSettings.SpringOffset, tirepos.z), MOMENT_WHEEL, tirepos.w, 
                        true, true, 0, 0, tirepos.z > 0.0f));
                }

                m_isConstrainedX = Wheel.rearCount == 0 || Wheel.frontCount == 0;
                m_isConstrainedZ = (Wheel.rearCount <= 1) && (Wheel.frontCount <= 1);

                float frontTorque = 0.0f;
                float rearTorque = 0.0f;
                float frontBraking = 0.0f;
                float rearBraking = 0.0f;
                if (Wheel.rearCount == 0 && Wheel.frontCount > 0)
                {
                    frontTorque = 1.0f / Wheel.frontCount;
                    frontBraking = Settings.ModSettings.BrakingForce * KN_TO_N / Wheel.frontCount;
                }
                else if (Wheel.frontCount == 0 && Wheel.rearCount > 0)
                {
                    rearTorque = 1.0f / Wheel.rearCount;
                    rearBraking = Settings.ModSettings.BrakingForce * KN_TO_N / Wheel.rearCount;
                }
                else if (Wheel.frontCount > 0 && Wheel.rearCount > 0)
                {
                    frontTorque = ModSettings.DriveBias / Wheel.frontCount;
                    rearTorque = (1.0f - ModSettings.DriveBias) / Wheel.rearCount;
                    frontBraking = ModSettings.BrakeBias * Settings.ModSettings.BrakingForce * KN_TO_N / Wheel.frontCount;
                    rearBraking = (1.0f - ModSettings.BrakeBias) * Settings.ModSettings.BrakingForce * KN_TO_N / Wheel.rearCount;
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
            }
            else
            {
                m_rideHeight = 0;
            }

            Mesh vehicleMesh = m_vehicleInfo.m_mesh;
            Vector3 fullBounds = vehicleMesh.bounds.size;
            m_headlight.transform.localPosition = new Vector3(0.0f, fullBounds.y * 0.5f, fullBounds.z * 0.5f + FLOAT_ERROR);
            m_taillight.transform.localPosition = new Vector3(0.0f, fullBounds.y * 0.5f, -fullBounds.z * 0.5f - FLOAT_ERROR);

            Vector3 adjustedBounds = m_vehicleInfo.m_lodMesh.bounds.size;
            adjustedBounds.y = adjustedBounds.y - m_rideHeight;
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

            RigidbodyConstraints constraints = RigidbodyConstraints.None;
            if (m_isConstrainedX)
            {
                constraints |= RigidbodyConstraints.FreezePositionX;
            }
            if (m_isConstrainedZ)
            {
                constraints |= RigidbodyConstraints.FreezeRotationZ;
            }

            m_vehicleRigidBody.constraints = constraints;

            m_vehicleCollider.size = adjustedBounds;
            m_vehicleCollider.center = new Vector3(0.0f, 0.5f * adjustedBounds.y + m_rideHeight, 0.0f);

            gameObject.GetComponent<MeshFilter>().mesh = gameObject.GetComponent<MeshFilter>().sharedMesh = vehicleMesh;
            gameObject.GetComponent<MeshRenderer>().material = gameObject.GetComponent<MeshRenderer>().sharedMaterial = m_vehicleInfo.m_material;

            if (m_setColor)
            {
                MaterialPropertyBlock materialBlock = Singleton<VehicleManager>.instance.m_materialBlock;
                materialBlock.Clear();
                materialBlock.SetColor(Singleton<VehicleManager>.instance.ID_Color, m_vehicleColor);
                gameObject.GetComponent<MeshRenderer>().SetPropertyBlock(materialBlock);
            }

            m_tangent = m_vehicleRigidBody.transform.TransformDirection(Vector3.forward);
            m_normal = m_vehicleRigidBody.transform.TransformDirection(Vector3.up);
            m_binormal = m_vehicleRigidBody.transform.TransformDirection(Vector3.right);

            gameObject.SetActive(true);

            AddEffects();
        }
        private void DestroyVehicle()
        {
            RemoveEffects();
            foreach (Wheel w in m_wheelObjects)
            {
                Object.DestroyImmediate(w.gameObject);
            }
            m_wheelObjects.Clear();
            gameObject.SetActive(false);
            m_vehicleRigidBody.velocity = Vector3.zero;
            m_vehicleRigidBody.angularVelocity = Vector3.zero;

            m_setColor = false;
            m_vehicleColor = default;
            m_vehicleInfo = null;
            m_prevPosition = Vector3.zero;
            m_prevVelocity = Vector3.zero;
            m_tangent = Vector3.zero;
            m_binormal = Vector3.zero;
            m_normal = Vector3.zero;
            m_lightState = Vector4.zero;
            m_isSirenEnabled = false;
            m_isLightEnabled = false;
            m_isDusty = false;
            m_isTurning = false;
            m_isConstrainedX = false;
            m_isConstrainedZ = false;
            m_gear = ENGINE_GEAR_NEUTRAL;
            m_driveMode = ENGINE_MODE_NEUTRAL;
            m_lastReset = 0.0f;
            m_terrainHeight = 0.0f;
            m_distanceTravelled = 0.0f;
            m_steer = 0.0f;
            m_brake = 0.0f;
            m_throttle = 0.0f;
            m_rideHeight = 0.0f;
            m_roofHeight = 0.0f;
            m_compression = 0.0f;
            m_prevGearChange = 0.0f;
            m_normalImpulse = 0.0f;
            m_radps = 0.0f;
            m_prevRadps = 0.0f;
            m_radpsTrans = 0.0f;
            m_tilt = 0.0f;
        }

        private void OverridePrefabs()
        {
            // override the underground material for all vehicles.
            for (uint prefabIndex = 0; prefabIndex < PrefabCollection<VehicleInfo>.PrefabCount(); prefabIndex++)
            {
                VehicleInfo prefabVehicleInfo = PrefabCollection<VehicleInfo>.GetPrefab(prefabIndex);
                if (prefabVehicleInfo == null) continue;
                prefabVehicleInfo.m_undergroundMaterial = prefabVehicleInfo.m_material;
                prefabVehicleInfo.m_undergroundLodMaterial = prefabVehicleInfo.m_lodMaterialCombined;
                foreach (VehicleInfo.MeshInfo submesh in prefabVehicleInfo.m_subMeshes)
                {
                    if (submesh.m_subInfo)
                    {
                        VehicleInfoSub subVehicleInfo = (VehicleInfoSub)submesh.m_subInfo;
                        subVehicleInfo.m_undergroundMaterial = subVehicleInfo.m_material;
                        subVehicleInfo.m_undergroundLodMaterial = subVehicleInfo.m_lodMaterialCombined;
                    }
                }
            }

            int prefabCount = PrefabCollection<NetInfo>.PrefabCount();

            // only modify prefabs with MetroTunnels item layer or underground render layer.
            for (uint prefabIndex = 0; prefabIndex < prefabCount; prefabIndex++)
            {
                NetInfo prefabNetInfo = PrefabCollection<NetInfo>.GetPrefab(prefabIndex);
                if (prefabNetInfo == null) continue;
                NetInfo prefabReplaceInfo = prefabNetInfo;
                bool bHasUnderground = false;
                bool bForceUnderground = false;

                for (int index = 0; index < prefabReplaceInfo.m_segments.Length; index++)
                {
                    bHasUnderground |= prefabReplaceInfo.m_segments[index].m_layer == MapUtils.LAYER_UNDERGROUND;
                }

                for (int index = 0; index < prefabReplaceInfo.m_nodes.Length; index++)
                {
                    bHasUnderground |= prefabReplaceInfo.m_nodes[index].m_layer == MapUtils.LAYER_UNDERGROUND;
                }

                if (prefabNetInfo.m_class.m_layer == ItemClass.Layer.MetroTunnels)
                {
                    bool bCanReplace = true;

                    foreach (NetInfo.Segment s in prefabNetInfo.m_segments)
                    {
                        if (s.m_material && (!s.m_material.shader || s.m_material.shader.name != "Custom/Net/Metro"))
                        {
                            bCanReplace = false;
                        }
                    }

                    // replace the prefab with elevated variant if no visible meshes are found.
                    if (bCanReplace)
                    {
                        string replaceName = "";
                    
                        // get underground to elvated mapping.
                        if (!m_customUndergroundMappings.TryGetValue(prefabNetInfo.name, out replaceName))
                        {
                            replaceName = prefabNetInfo.name.Replace(" Tunnel", " Elevated");
                        }

                        // find the elevated counterpart prefab to be used as a reference.
                        for (uint otherPrefabIndex = 0; otherPrefabIndex < prefabCount; otherPrefabIndex++)
                        {
                            NetInfo tmpInfo = PrefabCollection<NetInfo>.GetPrefab(otherPrefabIndex);
                            if (tmpInfo.m_class.m_layer == ItemClass.Layer.Default && tmpInfo.name == replaceName)
                            {
                                prefabReplaceInfo = tmpInfo;
                                bForceUnderground = true;
                                break;
                            }
                        }
                    }
                }
                
                
                if (bHasUnderground)
                {
                    m_backupPrefabData[prefabNetInfo] = new NetInfoBackup(prefabNetInfo.m_nodes, prefabNetInfo.m_segments);
                    NetInfo.Segment[] segments = new NetInfo.Segment[prefabReplaceInfo.m_segments.Length];
                    NetInfo.Node[] nodes = new NetInfo.Node[prefabReplaceInfo.m_nodes.Length];

                    for (int index = 0; index < prefabReplaceInfo.m_segments.Length; index++)
                    {
                        NetInfo.Segment newSegment = CopySegment(prefabReplaceInfo.m_segments[index]);

                        if (newSegment.m_layer == MapUtils.LAYER_UNDERGROUND)
                        {
                            // disable segment underground xray component from rendering.
                            if (newSegment.m_material && newSegment.m_material.shader && newSegment.m_material.shader.name == "Custom/Net/Metro")
                            {
                                newSegment.m_forwardForbidden = NetSegment.Flags.All;
                                newSegment.m_forwardRequired = NetSegment.Flags.None;
                                newSegment.m_backwardForbidden = NetSegment.Flags.All;
                                newSegment.m_backwardRequired = NetSegment.Flags.None;
                            }
                        }
                        // apply underground layer if the segment is being replaced from an overground component.
                        else if (bForceUnderground)
                        {
                            newSegment.m_layer = MapUtils.LAYER_UNDERGROUND;
                        }

                        segments[index] = newSegment;
                    }

                    for (int index = 0; index < prefabReplaceInfo.m_nodes.Length; index++)
                    {
                        NetInfo.Node newNode = CopyNode(prefabReplaceInfo.m_nodes[index]);

                        if (newNode.m_layer == MapUtils.LAYER_UNDERGROUND)
                        {
                            // disable node underground xray component from rendering.
                            if (newNode.m_material && newNode.m_material.shader && newNode.m_material.shader.name == "Custom/Net/Metro")
                            {
                                newNode.m_flagsForbidden = NetNode.Flags.All;
                                newNode.m_flagsRequired = NetNode.Flags.None;
                            }
                        }
                        // apply underground layer if the node is being replaced from an overground component.
                        else if (bForceUnderground)
                        {
                            newNode.m_layer = MapUtils.LAYER_UNDERGROUND;
                        }
                        
                        newNode.m_flagsForbidden = newNode.m_flagsForbidden & ~NetNode.Flags.Underground;

                        nodes[index] = newNode;
                    }

                    prefabNetInfo.m_segments = segments;
                    prefabNetInfo.m_nodes = nodes;
                }
            }

            // replace the distant LOD material for underground and update LOD render groups.
            RenderManager rm = Singleton<RenderManager>.instance;
            m_backupUndergroundMaterial = rm.m_groupLayerMaterials[MapUtils.LAYER_UNDERGROUND];
            rm.m_groupLayerMaterials[MapUtils.LAYER_UNDERGROUND] = rm.m_groupLayerMaterials[MapUtils.LAYER_ROAD];
            rm.UpdateGroups(MapUtils.LAYER_UNDERGROUND);
        }

        private void RestorePrefabs()
        {
            // delete all underground vehicle materials. Cities will auto generate new ones.
            for (uint iter = 0; iter < PrefabCollection<VehicleInfo>.PrefabCount(); iter++)
            {
                VehicleInfo prefabVehicleInfo = PrefabCollection<VehicleInfo>.GetPrefab(iter);
                if (prefabVehicleInfo == null) continue;
                prefabVehicleInfo.m_undergroundMaterial = null;
                prefabVehicleInfo.m_undergroundLodMaterial = null;
                foreach (VehicleInfo.MeshInfo submesh in prefabVehicleInfo.m_subMeshes)
                {
                    if (submesh.m_subInfo)
                    {
                        VehicleInfoSub subVehicleInfo = (VehicleInfoSub)submesh.m_subInfo;
                        subVehicleInfo.m_undergroundMaterial = null;
                        subVehicleInfo.m_undergroundLodMaterial = null;
                    }
                }
            }

            // restore road prefab segment and node data to before driving.
            int prefabCount = PrefabCollection<NetInfo>.PrefabCount();
            for (uint prefabIndex = 0; prefabIndex < prefabCount; prefabIndex++)
            {
                NetInfo prefabNetInfo = PrefabCollection<NetInfo>.GetPrefab(prefabIndex);
                if (prefabNetInfo == null) continue;

                if (m_backupPrefabData.ContainsKey(prefabNetInfo))
                {
                    if (m_backupPrefabData.TryGetValue(prefabNetInfo, out NetInfoBackup backupData))
                    {
                        prefabNetInfo.m_segments = backupData.segments;
                        prefabNetInfo.m_nodes = backupData.nodes;

                        m_backupPrefabData.Remove(prefabNetInfo);
                    }
                }
            }

            m_backupPrefabData.Clear();

            // restore the distant LOD material for underground and update LOD render groups.
            RenderManager rm = Singleton<RenderManager>.instance;
            rm.m_groupLayerMaterials[MapUtils.LAYER_UNDERGROUND] = m_backupUndergroundMaterial;
            m_backupUndergroundMaterial = null;
            rm.UpdateGroups(MapUtils.LAYER_UNDERGROUND);
        }

        private static NetInfo.Node CopyNode(NetInfo.Node node)
        {
            NetInfo.Node retval = new NetInfo.Node();
            retval.m_mesh = node.m_mesh;
            retval.m_lodMesh = node.m_lodMesh;
            retval.m_material = node.m_material;
            retval.m_lodMaterial = node.m_lodMaterial;
            retval.m_flagsRequired = node.m_flagsRequired;
            retval.m_flagsRequired2 = node.m_flagsRequired2;
            retval.m_flagsForbidden = node.m_flagsForbidden;
            retval.m_flagsForbidden2 = node.m_flagsForbidden2;
            retval.m_connectGroup = node.m_connectGroup;
            retval.m_directConnect = node.m_directConnect;
            retval.m_emptyTransparent = node.m_emptyTransparent;
            retval.m_tagsRequired = node.m_tagsRequired;
            retval.m_nodeTagsRequired = node.m_nodeTagsRequired;
            retval.m_tagsForbidden = node.m_tagsForbidden;
            retval.m_nodeTagsForbidden = node.m_nodeTagsForbidden;
            retval.m_forbidAnyTags = node.m_forbidAnyTags;
            retval.m_minSameTags = node.m_minSameTags;
            retval.m_maxSameTags = node.m_maxSameTags;
            retval.m_minOtherTags = node.m_minOtherTags;
            retval.m_maxOtherTags = node.m_maxOtherTags;
            retval.m_nodeMesh = node.m_nodeMesh;
            retval.m_nodeMaterial = node.m_nodeMaterial;
            retval.m_combinedLod = node.m_combinedLod;
            retval.m_lodRenderDistance = node.m_lodRenderDistance;
            retval.m_requireSurfaceMaps = node.m_requireSurfaceMaps;
            retval.m_requireWindSpeed = node.m_requireWindSpeed;
            retval.m_preserveUVs = node.m_preserveUVs;
            retval.m_generateTangents = node.m_generateTangents;
            retval.m_layer = node.m_layer;

            return retval;
        }

        private static NetInfo.Segment CopySegment(NetInfo.Segment segment)
        {
            NetInfo.Segment retval = new NetInfo.Segment();
            retval.m_mesh = segment.m_mesh;
            retval.m_lodMesh = segment.m_lodMesh;
            retval.m_material = segment.m_material;
            retval.m_lodMaterial = segment.m_lodMaterial;
            retval.m_forwardRequired = segment.m_forwardRequired;
            retval.m_forwardForbidden = segment.m_forwardForbidden;
            retval.m_backwardRequired = segment.m_backwardRequired;
            retval.m_backwardForbidden = segment.m_backwardForbidden;
            retval.m_emptyTransparent = segment.m_emptyTransparent;
            retval.m_disableBendNodes = segment.m_disableBendNodes;
            retval.m_segmentMesh = segment.m_segmentMesh;
            retval.m_segmentMaterial = segment.m_segmentMaterial;
            retval.m_combinedLod = segment.m_combinedLod;
            retval.m_lodRenderDistance = segment.m_lodRenderDistance;
            retval.m_requireSurfaceMaps = segment.m_requireSurfaceMaps;
            retval.m_requireHeightMap = segment.m_requireHeightMap;
            retval.m_requireWindSpeed = segment.m_requireWindSpeed;
            retval.m_preserveUVs = segment.m_preserveUVs;
            retval.m_generateTangents = segment.m_generateTangents;
            retval.m_layer = segment.m_layer;

            return retval;
        }

        // there are problems with replacing LodValue. It needs to be registered with NodeManager and/or RenderManager or there will be null reference errors.
        //private static NetInfo.LodValue CopyLodValue(NetInfo.LodValue value)
        //{
        //    NetInfo.LodValue retval = new NetInfo.LodValue();
        //    retval.m_key = value.m_key;
        //    retval.m_material = value.m_material;
        //    retval.m_lodMin = value.m_lodMin;
        //    retval.m_lodMax = value.m_lodMax;
        //    retval.m_surfaceTexA = value.m_surfaceTexA;
        //    retval.m_surfaceTexB = value.m_surfaceTexB;
        //    retval.m_surfaceMapping = value.m_surfaceMapping;
        //    retval.m_heightMap = value.m_heightMap;
        //    retval.m_heightMapping = value.m_heightMapping;

        //    return retval;
        //}

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
                            m_brake = Mathf.Clamp(m_brake + Time.fixedDeltaTime * THROTTLE_RESP, 0.0f, 1.0f);
                            braking = true;
                        }
                    }
                    else if (m_throttle == 0.0f && Time.time > m_prevGearChange + GEAR_RESP && m_driveMode <= ENGINE_MODE_NEUTRAL)
                    {
                        m_driveMode++;
                        m_prevGearChange = Time.time;
                    }
                    if (m_driveMode > ENGINE_MODE_NEUTRAL)
                    {
                        m_brake = 0.0f;
                        m_throttle = Mathf.Clamp(m_throttle + Time.fixedDeltaTime * THROTTLE_RESP, 0.0f, 1.0f);
                        throttling = true;
                    }
                }
                else // ManualTrans
                {
                    m_throttle = Mathf.Clamp(m_throttle + Time.fixedDeltaTime * THROTTLE_RESP, 0.0f, 1.0f);
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
                            m_brake = Mathf.Clamp(m_brake + Time.fixedDeltaTime * THROTTLE_RESP, 0.0f, 1.0f);
                            braking = true;
                        }
                    }
                    else if (m_throttle == 0.0f && Time.time > m_prevGearChange + GEAR_RESP && m_driveMode >= ENGINE_MODE_NEUTRAL)
                    {
                        m_driveMode--;
                        m_prevGearChange = Time.time;
                    }
                    if (m_driveMode < ENGINE_MODE_NEUTRAL)
                    {
                        m_brake = 0.0f;
                        m_throttle = Mathf.Clamp(m_throttle + Time.fixedDeltaTime * THROTTLE_RESP, 0.0f, 1.0f);
                        throttling = true;
                    }
                }
                else // ManualTrans
                {
                    m_brake = Mathf.Clamp(m_brake + Time.fixedDeltaTime * THROTTLE_RESP, 0.0f, 1.0f);
                    braking = true;
                }
            }
            if (ModSettings.AutoTrans && invert == 0 && m_throttle == 0.0f)
            {
                if (Time.time > m_prevGearChange + GEAR_RESP)
                {
                    m_driveMode = ENGINE_MODE_NEUTRAL;
                    m_prevGearChange = Time.time;
                }
                m_brake = 1.0f;
                braking = true;
            }
            if (!throttling)
            {
                m_throttle = Mathf.Clamp(m_throttle - Time.fixedDeltaTime * THROTTLE_REST, 0.0f, 1.0f);
            }
            if (!braking)
            {
                m_brake = Mathf.Clamp(m_brake - Time.fixedDeltaTime * THROTTLE_REST, 0.0f, 1.0f);
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
            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyLightToggle.Key))
                m_isLightEnabled = !m_isLightEnabled;

            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeySirenToggle.Key))
                m_isSirenEnabled = !m_isSirenEnabled;

            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyResetVehicle.Key) && m_lastReset + RESET_FREQ < Time.time)
            {
                Quaternion rot = Quaternion.LookRotation(m_vehicleRigidBody.transform.TransformDirection(Vector3.forward));
                Vector3 pos = m_vehicleRigidBody.transform.position;
                pos.y = MapUtils.CalculateHeight(pos, RESET_SCAN_HEIGHT, out var _) + RESET_HEIGHT;
                m_vehicleRigidBody.transform.SetPositionAndRotation(pos, rot);
                m_lastReset = Time.time;
            }
            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyGearUp.Key) && m_prevGearChange + GEAR_RESP < Time.time && m_gear < ENGINE_GEAR_FORWARD_END)
            {
                m_gear += 1;
                m_prevGearChange = Time.time;
            }
            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyGearDown.Key) && m_prevGearChange + GEAR_RESP < Time.time && m_gear > 0)
            {
                m_gear -= 1;
                m_prevGearChange = Time.time;
            }
        }
        private void AddEffects()
        {
            if (m_vehicleInfo.m_effects != null)
            {
                foreach (var effect in m_vehicleInfo.m_effects)
                {
                    {
                        if (effect.m_effect != null)
                        {
                            if (effect.m_vehicleFlagsRequired.IsFlagSet(Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2))
                                m_specialEffects.Add(effect.m_effect);
                            else if (effect.m_vehicleFlagsRequired.IsFlagSet(Vehicle.Flags.OnGravel))
                                {
                                m_dustEffects.Add(effect.m_effect);
                            }
                            else if (effect.m_effect is EngineSoundEffect esEffect0)
                            {
                                m_engineSoundEffects.Add(esEffect0);
                            }
                            else if (effect.m_effect is MultiEffect multiEffect)
                            {
                                foreach (var sub in multiEffect.m_effects)
                                {
                                    if (sub.m_effect is LightEffect lightEffect)
                                    {
                                        m_lightEffects.Add(lightEffect);
                                    }
                                    else if (sub.m_effect is EngineSoundEffect esEffect1)
                                    {
                                        m_engineSoundEffects.Add(esEffect1);
                                    }
                                    else
                                    {
                                        m_regularEffects.Add(effect.m_effect);
                                    }
                                }
                            }
                            else
                            {
                                m_regularEffects.Add(effect.m_effect);
                            }
                        }
                    }
                }
            }
        }
        private void PlayEffects()
        {
            var position = m_vehicleRigidBody.transform.position;
            var rotation = m_vehicleRigidBody.transform.rotation;
            var velocity = m_vehicleRigidBody.velocity;
            var acceleration = ((velocity - m_prevVelocity) / Time.fixedDeltaTime).magnitude;
            var swayPosition = Vector3.zero;
            var scale = Vector3.one;
            var matrix = m_vehicleInfo.m_vehicleAI.CalculateBodyMatrix(Vehicle.Flags.Created | Vehicle.Flags.Spawned, ref position, ref rotation, ref scale, ref swayPosition);
            var area = new EffectInfo.SpawnArea(matrix, m_vehicleInfo.m_lodMeshData);
            var listenerInfo = Singleton<AudioManager>.instance.CurrentListenerInfo;
            var audioGroup = Singleton<VehicleManager>.instance.m_audioGroup;
            RenderGroup.MeshData effectMeshData = m_vehicleInfo.m_vehicleAI.GetEffectMeshData();
            var area2 = new EffectInfo.SpawnArea(matrix, effectMeshData, m_vehicleInfo.m_generatedInfo.m_tyres, m_vehicleInfo.m_lightPositions);

            foreach (var regularEffect in m_regularEffects)
            {
                regularEffect.PlayEffect(default, area, velocity, acceleration, 1f, listenerInfo, audioGroup);
            }
            foreach (var engineEffect in m_engineSoundEffects)
            {
                engineEffect.PlayEffect(default, area, ENGINE_PITCH * m_radps * Vector3.up, ENGINE_PITCH * (m_radps - m_prevRadps) / Time.fixedDeltaTime, 1.0f + 0.25f * m_radps / ENGINE_PEAK_RPS + 0.25f * m_throttle, listenerInfo, audioGroup);
            }

            if (m_isLightEnabled)
            {
                foreach (var light in m_lightEffects)
                {
                    light.RenderEffect(default, area2, velocity, acceleration, 1f, -1f, Singleton<SimulationManager>.instance.m_simulationTimeDelta, Singleton<RenderManager>.instance.CurrentCameraInfo);
                }
            }

            if (m_isSirenEnabled)
            {
                foreach (var specialEffect in m_specialEffects)
                {
                    specialEffect.RenderEffect(default, area2, velocity, acceleration, 1f, -1f, Singleton<SimulationManager>.instance.m_simulationTimeDelta, Singleton<RenderManager>.instance.CurrentCameraInfo);
                    specialEffect.PlayEffect(default, area, velocity, acceleration, 1f, listenerInfo, audioGroup);
                }
            }
            if (m_isDusty)
            {
                foreach (var dustEffect in m_dustEffects)
                {
                    dustEffect.RenderEffect(default, area2, velocity, acceleration, 1f, -1f, Singleton<SimulationManager>.instance.m_simulationTimeDelta, Singleton<RenderManager>.instance.CurrentCameraInfo);
                }
            }
        }

        private void RemoveEffects()
        {
            m_lightEffects.Clear();
            m_engineSoundEffects.Clear();
            m_regularEffects.Clear();
            m_dustEffects.Clear();
            m_specialEffects.Clear();
        }
    }
}