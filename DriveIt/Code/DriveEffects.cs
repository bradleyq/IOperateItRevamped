using ColossalFramework;
using DriveIt.Settings;
using DriveIt.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DriveIt
{
    public class DriveEffects : MonoBehaviour
    {
        private const float ENGINE_PITCH = 0.15f;
        private const float LIGHT_HEADLIGHT_INTENSITY = 3.0f;
        private const float LIGHT_HEADLIGHT_RANGE = 125.0f;
        private const float LIGHT_HEADLIGHT_ANGLE = 60.0f;
        private const float LIGHT_TAILLIGHT_INTENSITY = 1.0f;
        private const float LIGHT_TAILLIGHT_IDLE_INTENSITY = 0.5f;
        private const float LIGHT_TAILLIGHT_RANGE = 15.0f;
        private const float LIGHT_TEXTURE_INTENSITY = 5.0f;
        private const float LIGHT_TEXTURE_IDLE_INTENSITY = 0.5f;
        private const float UI_SIZE = 1.0f / 4.0f;
        private const float UI_OFFSET = 1.0f / 12.0f;
        private const float UI_FILL = 1.0f;
        private const int TIRE_TRAIL_POOL = 64;
        private const float TIRE_TRAIL_END_DELAY = 0.1f;
        private static readonly Color UI_BG_COLOR = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        private static readonly Color UI_FG_COLOR = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        private struct NetInfoBackup
        {
            public NetInfoBackup(NetInfo.Node[] nodes, NetInfo.Segment[] segments)
            {
                this.nodes = nodes;
                this.segments = segments;
            }

            public NetInfo.Node[] nodes;
            public NetInfo.Segment[] segments;
        }

        public static DriveEffects instance { get; private set; }
        public static DriveController s_controller;

        private Color m_vehicleColor;
        private VehicleInfo m_vehicleInfo;
        private GameObject m_visualObject;
        private GameObject m_headlightObject;
        private GameObject m_taillightObject;
        private Light m_headlight;
        private Light m_taillight;
        private GUIStyle m_speedoStyle;
        private GUIStyle m_gearStyle;
        private GameObject m_tireTrailRefObject;
        private TrailRenderer m_tireTrailRef;
        private Material m_backupUndergroundMaterial = null;
        private Material m_uiMaterial = null;
        private Material m_renderMaterial = null;
        private Material m_basicRenderMaterial = null;

        private Queue<TrailRenderer> m_tireTrails = new Queue<TrailRenderer>();
        private List<TrailRenderer> m_usedTireTrails = new List<TrailRenderer>();
        private List<float> m_usedTireTrailStopTime = new List<float>();

        private List<LightEffect> m_lightEffects = new List<LightEffect>();
        private List<EngineSoundEffect> m_engineSoundEffects = new List<EngineSoundEffect>();
        private List<EffectInfo> m_regularEffects = new List<EffectInfo>();
        private List<EffectInfo> m_dustEffects = new List<EffectInfo>();
        private List<EffectInfo> m_specialEffects = new List<EffectInfo>();

        private Dictionary<string, string> m_customUndergroundMappings = new Dictionary<string, string>();
        private Dictionary<NetInfo, NetInfoBackup> m_backupPrefabData = new Dictionary<NetInfo, NetInfoBackup>();

        private Vector4 m_lightState;
        private bool m_isSirenEnabled = false;
        private bool m_isLightEnabled = false;
        private bool m_isDusty = false;
        private bool m_vehicleColorSet = false;
        private bool m_isStandardVehicle = false;
        private float m_spCompression;
        private Vector3 m_spTangent;
        private Vector3 m_spBinormal;
        private Vector3 m_spNormal;
        private void Awake()
        {
            if (instance)
            {
                Destroy(this);
                return;
            }
            instance = this;

            m_visualObject = new GameObject("DriveEffects");
            m_visualObject.transform.parent = gameObject.transform;
            m_visualObject.AddComponent<MeshFilter>();
            m_visualObject.AddComponent<MeshRenderer>();
            m_visualObject.GetComponent<MeshRenderer>().enabled = true;

            m_headlightObject = new GameObject();
            m_headlight = m_headlightObject.AddComponent<Light>();
            m_headlight.type = LightType.Spot;
            m_headlight.intensity = LIGHT_HEADLIGHT_INTENSITY;
            m_headlight.range = LIGHT_HEADLIGHT_RANGE;
            m_headlight.spotAngle = LIGHT_HEADLIGHT_ANGLE;
            m_headlight.transform.parent = m_visualObject.transform;
            m_headlight.color = Color.white;
            m_headlight.enabled = false;

            m_taillightObject = new GameObject();
            m_taillight = m_taillightObject.AddComponent<Light>();
            m_taillight.type = LightType.Point;
            m_taillight.intensity = LIGHT_TAILLIGHT_IDLE_INTENSITY;
            m_taillight.range = LIGHT_TAILLIGHT_RANGE;
            m_taillight.transform.parent = m_visualObject.transform;
            m_taillight.color = Color.red;
            m_taillight.enabled = false;

            m_visualObject.SetActive(false);
            enabled = false;

            // Some tunnel names are atypical and need to be manually mapped.
            m_customUndergroundMappings["HighwayRamp Tunnel"] = "HighwayRampElevated";
            m_customUndergroundMappings["Metro Track"] = "Metro Track Elevated 01";
            m_customUndergroundMappings["Metro Station Track"] = "Metro Station Track Elevated 01";
            m_customUndergroundMappings["Large Oneway Road Tunnel"] = "Large Oneway Elevated";
            m_customUndergroundMappings["Metro Station Below Ground Bypass"] = "Metro Station Track Elevated Bypass";
            m_customUndergroundMappings["Metro Station Below Ground Dual Island"] = "Metro Station Track Elevated Dual Island";
            m_customUndergroundMappings["Metro Station Below Ground Island"] = "Metro Station Track Elevated Island Platform";

            RenderManager rm = Singleton<RenderManager>.instance;
            m_backupUndergroundMaterial = rm.m_groupLayerMaterials[MapUtils.LAYER_UNDERGROUND];
            m_renderMaterial = rm.m_groupLayerMaterials[MapUtils.LAYER_ROAD];

            Shader diffuseShader = Shader.Find("Hidden/Internal-Colored");
            m_basicRenderMaterial = new Material(diffuseShader);
            m_basicRenderMaterial.hideFlags = HideFlags.HideAndDontSave;
            m_basicRenderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_basicRenderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_basicRenderMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m_basicRenderMaterial.SetInt("_ZWrite", 0);

            Shader flatShader = Shader.Find("Hidden/Internal-Colored");
            m_uiMaterial = new Material(flatShader);
            m_uiMaterial.hideFlags = HideFlags.HideAndDontSave;
            m_uiMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_uiMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_uiMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            m_uiMaterial.SetInt("_ZWrite", 0);

            m_speedoStyle = new GUIStyle();
            m_speedoStyle.fontSize = (int)(Screen.height * UI_SIZE * 0.3f);
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

            m_tireTrailRefObject = new GameObject();
            m_tireTrailRef = m_tireTrailRefObject.AddComponent<TrailRenderer>();
            m_tireTrailRef.sharedMaterial = m_basicRenderMaterial;
            m_tireTrailRef.widthMultiplier = 1.0f;
            m_tireTrailRef.startColor = new Color(0.0f, 0.0f, 0.0f, 0.4f);
            m_tireTrailRef.endColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            m_tireTrailRef.alignment = LineAlignment.Local;
            m_tireTrailRef.time = Mathf.Infinity;
            m_tireTrailRef.enabled = false;

            for (int i = 0; i < TIRE_TRAIL_POOL; i++)
            {
                m_tireTrails.Enqueue(GameObject.Instantiate(m_tireTrailRefObject).GetComponent<TrailRenderer>());
            }
        }
        private void Update()
        {
            HandleInputOnUpdate();
            PlayEffects();

            MaterialPropertyBlock materialBlock = Singleton<VehicleManager>.instance.m_materialBlock;
            materialBlock.Clear();
            Vector4 tyrePosition = default;
            tyrePosition.x = s_controller.steer / 180.0f * Mathf.PI;
            tyrePosition.y = s_controller.odometer;
            tyrePosition.z = 0f;
            tyrePosition.w = 0f;
            materialBlock.SetVector(Singleton<VehicleManager>.instance.ID_TyrePosition, tyrePosition);

            m_lightState.x = m_isLightEnabled ? LIGHT_TEXTURE_INTENSITY : 0.0f;
            m_lightState.y = s_controller.brake > 0.0f ? LIGHT_TEXTURE_INTENSITY : (m_isLightEnabled ? LIGHT_TEXTURE_IDLE_INTENSITY : 0.0f);
            materialBlock.SetVector(Singleton<VehicleManager>.instance.ID_LightState, m_lightState);
            if (m_vehicleColorSet)
            {
                materialBlock.SetColor(Singleton<VehicleManager>.instance.ID_Color, m_vehicleColor);
            }

            m_headlight.enabled = m_isLightEnabled && m_isStandardVehicle;

            float tailIntensity = m_isLightEnabled ? LIGHT_TEXTURE_IDLE_INTENSITY : 0.0f;
            tailIntensity = s_controller.brake > 0.0f ? LIGHT_TAILLIGHT_INTENSITY : tailIntensity;
            if (tailIntensity > 0.0f)
            {
                m_taillight.intensity = tailIntensity;
                m_taillight.enabled = m_isStandardVehicle;
            }
            else
            {
                m_taillight.enabled = false;
            }

            materialBlock.SetMatrix(Singleton<VehicleManager>.instance.ID_TyreMatrix, 
                Matrix4x4.TRS(Mathf.Max(m_spCompression + ModSettings.SpringOffset, ModSettings.SpringOffset) * Vector3.up, Quaternion.LookRotation(m_spTangent, m_spNormal), Vector3.one));

            m_visualObject.GetComponent<MeshRenderer>().SetPropertyBlock(materialBlock);
        }
        private void FixedUpdate()
        {
            UpdateWheelEffects();
        }

        private void LateUpdate()
        {
        }
        private void OnDestroy()
        {
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
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
                GUI.DrawTexture(bgarea, DriveCommon.s_driveTextureGaugeCluster);
                GUI.color = UI_FG_COLOR;

                // text
                GUI.Label(area, s_controller.gear, m_gearStyle);
                GUI.Label(area, Mathf.RoundToInt(s_controller.speed * DriveCommon.MS_TO_KMPH).ToString(), m_speedoStyle);

                // Draw tach arc
                DriveCommon.DrawRingSegment(new Vector2(x + sizePx * 0.5f, y + sizePx * 0.5f),
                    fillPx * 0.5f, fillPx * 0.5f * 0.9f,
                    Mathf.PI * 5.0f / 4.0f, -Mathf.PI / 4.0f,
                    s_controller.tachometer,
                    UI_FG_COLOR, m_uiMaterial);
            }
        }

        private void UpdateWheelEffects()
        {
            int dustyCount = 0;
            Vector3 frontSum = Vector3.zero;
            Vector3 rearSum = Vector3.zero;
            Vector3 rightSum = Vector3.zero;
            Vector3 leftSum = Vector3.zero;
            
            for (int i = 0; i < DriveController.Wheel.wheelCount; i++)
            {
                DriveController.Wheel w = s_controller.wheels[i];

                if (m_isStandardVehicle)
                {
                    TrailRenderer trail = m_usedTireTrails[i];
                    bool trailFound = trail;

                    if (w.isOnGround && w.wheelGroundType == MapUtils.COLLISION_TYPE.ROAD && w.wheelHighSlip > 0.0f)
                    {
                        m_usedTireTrailStopTime[i] = Time.time + TIRE_TRAIL_END_DELAY;
                    }

                    if (w.isOnGround && w.wheelGroundType == MapUtils.COLLISION_TYPE.GROUND)
                    {
                        dustyCount += 1;
                    }

                    if (m_usedTireTrailStopTime[i] > Time.time)
                    {
                        if (!trailFound)
                        {
                            trail = m_tireTrails.Dequeue();
                            trail.widthMultiplier = w.wheelRadius * 0.75f;
                            m_usedTireTrails[i] = trail;
                        }

                        trail.gameObject.transform.position = w.wheelContactPoint + 0.01f * w.wheelGroundNormal;
                        trail.gameObject.transform.rotation = Quaternion.LookRotation(-w.wheelGroundNormal);

                        if (!trailFound)
                        {
                            trail.Clear();
                        }
                    }
                    else if (trailFound)
                    {
                        m_tireTrails.Enqueue(trail);
                        m_usedTireTrails[i] = null;
                    }
                }

                Vector3 localPos = w.transform.localPosition;
                localPos.y -= w.wheelOrigin.y;
                if (w.isFront)
                {
                    frontSum += localPos;
                }
                else
                {
                    rearSum += localPos;
                }
                if (w.isRight)
                {
                    rightSum += localPos;
                }
                else
                {
                    leftSum += localPos;
                }
            }

            m_spBinormal = Vector3.right;
            m_spTangent = Vector3.forward;
            m_spCompression = ((frontSum + rearSum) / DriveController.Wheel.wheelCount).y;

            if (!s_controller.inlineWheels)
            {
                m_spBinormal = Vector3.Normalize(rightSum / DriveController.Wheel.rightCount - leftSum / DriveController.Wheel.leftCount);
            }
            if (!s_controller.parallelWheels)
            {
                Vector3 frontAvg = frontSum / DriveController.Wheel.frontCount;
                Vector3 rearAvg = rearSum / DriveController.Wheel.rearCount;
                float wblength = Mathf.Abs(frontAvg.z - rearAvg.z);
                m_spTangent = Vector3.Normalize(frontAvg - rearAvg);
                m_spCompression = frontAvg.y * Mathf.Abs(rearAvg.z) / wblength + rearAvg.y * Mathf.Abs(frontAvg.z) / wblength;
            }
            
            m_spNormal = Vector3.Normalize(Vector3.Cross(m_spTangent, m_spBinormal));

            m_isDusty = dustyCount >= DriveController.Wheel.wheelCount / 2;
        }

        public void UpdateVehicleInfo(VehicleInfo info, Color color, bool setColor = false)
        {
            m_vehicleInfo = info;
            m_vehicleColorSet = setColor;
            if (setColor)
            {
                m_vehicleColor = color;
            }
        }

        public void StartEffects()
        {
            this.enabled = true;
            m_vehicleColor.a = 0; // Make sure blinking is not set.
            m_lightState = Vector4.zero;
            m_isSirenEnabled = false;
            m_isLightEnabled = false;
            m_isDusty = false;
            m_isStandardVehicle = !s_controller.fallbackWheels;
            m_spCompression = 0.0f;
            m_spTangent = Vector3.forward;
            m_spBinormal = Vector3.right;
            m_spNormal = Vector3.up;
            m_vehicleInfo.CalculateGeneratedInfo();

            Mesh vehicleMesh = m_vehicleInfo.m_mesh;
            Vector3 fullBounds = vehicleMesh.bounds.size;
            m_headlight.transform.localPosition = new Vector3(0.0f, fullBounds.y * 0.5f, fullBounds.z * 0.5f + DriveCommon.FLOAT_ERROR);
            m_taillight.transform.localPosition = new Vector3(0.0f, fullBounds.y * 0.5f, -fullBounds.z * 0.5f - DriveCommon.FLOAT_ERROR);

            m_visualObject.GetComponent<MeshFilter>().sharedMesh = vehicleMesh;
            m_visualObject.GetComponent<MeshRenderer>().sharedMaterial = m_vehicleInfo.m_material;

            if (m_vehicleColorSet)
            {
                MaterialPropertyBlock materialBlock = Singleton<VehicleManager>.instance.m_materialBlock;
                materialBlock.Clear();
                materialBlock.SetColor(Singleton<VehicleManager>.instance.ID_Color, m_vehicleColor);
                m_visualObject.GetComponent<MeshRenderer>().SetPropertyBlock(materialBlock);
            }

            m_visualObject.SetActive(true);

            for (int i = 0; i < s_controller.wheels.Count; i++)
            {
                m_usedTireTrails.Add(null);
                m_usedTireTrailStopTime.Add(0.0f);
            }
            foreach (TrailRenderer trail in m_tireTrails)
            {
                trail.enabled = true;
            }

            OverridePrefabs();
            AddEffects();
        }

        public void StopEffects(bool disable = true)
        {
            RemoveEffects();
            RestorePrefabs();

            foreach (TrailRenderer trail in m_usedTireTrails)
            {
                if (trail)
                {
                    m_tireTrails.Enqueue(trail);
                }
            }
            m_usedTireTrails.Clear();
            m_usedTireTrailStopTime.Clear();
            foreach (TrailRenderer trail in m_tireTrails)
            {
                trail.enabled = false;
            }

            if (disable)
            {
                m_visualObject.SetActive(false);
                this.enabled = false;
            }

            m_vehicleColorSet = false;
            m_vehicleColor = default;
            m_vehicleInfo = null;
            m_lightState = Vector4.zero;
            m_isSirenEnabled = false;
            m_isLightEnabled = false;
            m_isDusty = false;
            m_spCompression = 0.0f;
            m_spTangent = Vector3.forward;
            m_spBinormal = Vector3.right;
            m_spNormal = Vector3.up;
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
            rm.m_groupLayerMaterials[MapUtils.LAYER_UNDERGROUND] = m_renderMaterial;
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

        private void HandleInputOnUpdate()
        {
            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeyLightToggle.Key))
                m_isLightEnabled = !m_isLightEnabled;

            if (Input.GetKeyDown((KeyCode)Settings.ModSettings.KeySirenToggle.Key))
                m_isSirenEnabled = !m_isSirenEnabled;
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
            Vector3 velocity = s_controller.velocity;
            Vector3 position = m_visualObject.transform.position;
            Quaternion rotation = m_visualObject.transform.rotation;
            float acceleration = s_controller.acceleration.magnitude;
            Vector3 swayPosition = Vector3.zero;
            Vector3 scale = Vector3.one;
            Matrix4x4 matrix = m_vehicleInfo.m_vehicleAI.CalculateBodyMatrix(Vehicle.Flags.Created | Vehicle.Flags.Spawned, ref position, ref rotation, ref scale, ref swayPosition);
            AudioManager.ListenerInfo listenerInfo = Singleton<AudioManager>.instance.CurrentListenerInfo;
            AudioGroup audioGroup = Singleton<AudioManager>.instance.DefaultGroup;
            AudioGroup audioGroupLow = Singleton<VehicleManager>.instance.m_audioGroup;
            RenderGroup.MeshData effectMeshData = m_vehicleInfo.m_vehicleAI.GetEffectMeshData();
            EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(matrix, m_vehicleInfo.m_lodMeshData);
            EffectInfo.SpawnArea area2 = new EffectInfo.SpawnArea(matrix, effectMeshData, m_vehicleInfo.m_generatedInfo.m_tyres, m_vehicleInfo.m_lightPositions);

            foreach (var regularEffect in m_regularEffects)
            {
                regularEffect.PlayEffect(default, area, velocity, acceleration, 1f, listenerInfo, audioGroup);
            }
            foreach (var engineEffect in m_engineSoundEffects)
            {
                engineEffect.PlayEffect(default, 
                                        area, 
                                        ENGINE_PITCH * s_controller.radps * Vector3.up, 
                                        0.0f, 
                                        4.0f * (0.75f + 0.125f * s_controller.tachometer + 0.125f * Mathf.Max(s_controller.throttle, 
                                        Mathf.Clamp01(s_controller.angularAcceleration))), 
                                        listenerInfo, 
                                        audioGroup);
            }
            foreach (DriveController.Wheel w in s_controller.wheels)
            {
                if (w.isOnGround && w.wheelGroundType == MapUtils.COLLISION_TYPE.ROAD && w.wheelOptimSlip > 0.0f && m_isStandardVehicle)
                {
                    EffectInfo.SpawnArea tireArea = new EffectInfo.SpawnArea(w.transform.localToWorldMatrix, effectMeshData);
                    DriveCommon.s_driveSoundTireSqueal.PlaySound(default, listenerInfo, audioGroup, w.wheelContactPoint, w.wheelContactVelocity, DriveCommon.SND_RANGE, w.wheelOptimSlip, 0.9f + 0.2f * w.wheelSlip);
                }
                if (w.isOnGround && w.wheelGroundType == MapUtils.COLLISION_TYPE.ROAD && Mathf.Abs(w.radps) > 10.0f)
                {
                    EffectInfo.SpawnArea tireArea = new EffectInfo.SpawnArea(w.transform.localToWorldMatrix, effectMeshData);
                    float wheelSpeed = Mathf.Abs(w.radps) * w.wheelRadius;
                    DriveCommon.s_driveSoundTirePavement.PlaySound(default, listenerInfo, audioGroupLow, w.wheelContactPoint, w.wheelContactVelocity, DriveCommon.SND_RANGE, Mathf.Clamp01(wheelSpeed * 0.005f) * (1.0f - w.wheelSlip), 1.0f + wheelSpeed * 0.002f);
                }
                if (w.isOnGround && w.wheelGroundType == MapUtils.COLLISION_TYPE.GROUND && Mathf.Abs(w.radps) > 0.0f)
                {
                    EffectInfo.SpawnArea tireArea = new EffectInfo.SpawnArea(w.transform.localToWorldMatrix, effectMeshData);
                    float wheelSpeed = Mathf.Abs(w.radps) * w.wheelRadius;
                    DriveCommon.s_driveSoundTireGravel.PlaySound(default, listenerInfo, audioGroup, w.wheelContactPoint, w.wheelContactVelocity, DriveCommon.SND_RANGE, 1.5f * (0.6f * w.wheelSlip + 0.4f * Mathf.Clamp01(wheelSpeed * 0.1f)), 0.75f + wheelSpeed * 0.002f);
                }
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