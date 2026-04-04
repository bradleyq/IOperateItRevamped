using AlgernonCommons.Translation;
using ColossalFramework;
using UnityEngine;
using static PathUnit;

namespace DriveIt.Utils
{
    public class DriveSelectTool : ToolBase
    {
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);
            if (!m_toolController.IsInsideUI && Cursor.visible)
            {
                if (RaycastRoad(out var raycastOutput))
                {
                    ushort netSegmentId = raycastOutput.m_netSegment;

                    if (netSegmentId != 0)
                    {
                        var netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[netSegmentId];

                        if (netSegment.m_flags.IsFlagSet(NetSegment.Flags.Created))
                        {
                            var color = GetToolColor(false, false);
                            NetTool.RenderOverlay(cameraInfo, ref netSegment, color, color);
                        }
                    }
                }
            }
        }

        protected override void OnToolGUI(Event e)
        {
            if (!m_toolController.IsInsideUI && Cursor.visible && RaycastRoad(out var raycastOutput))
            {
                ushort netSegmentId = raycastOutput.m_netSegment;

                if (netSegmentId != 0)
                {
                    var netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[netSegmentId];

                    if (netSegment.m_flags.IsFlagSet(NetSegment.Flags.Created))
                    {
                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            netSegment.GetClosestPositionAndDirection(netSegment.m_middlePosition, out _, out var dir);
                            var rotation = Quaternion.LookRotation(dir);
                            DriveController.instance.StartDriving(netSegment.m_middlePosition, rotation);
                            ShowToolInfo(false, null, Vector3.zero);

                            //unset self as tool
                            ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                            ToolsModifierControl.SetTool<DefaultTool>();
                            Destroy(this);
                            return;
                        }
                    }
                    ShowToolInfo(true, Translations.Translate(DriveCommon.TK_ROAD_SELECT), netSegment.m_bounds.center);
                }
                else
                {
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        Vector3 position = raycastOutput.m_hitPos;
                        position.y = TerrainHeight(position);
                        Vector3 normal = TerrainNormal(position);
                        Vector3 dir = Camera.main.transform.TransformDirection(Vector3.forward);
                        dir = Vector3.Cross(dir, normal);
                        dir = dir.magnitude > 0.0f ? dir : Vector3.Cross(Vector3.forward, normal);
                        dir = Vector3.Cross(normal, dir).normalized;
                        DriveController.instance.StartDriving(position, Quaternion.LookRotation(dir, normal));
                        ShowToolInfo(false, null, Vector3.zero);

                        //unset self as tool
                        ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                        ToolsModifierControl.SetTool<DefaultTool>();
                        Destroy(this);
                        return;
                    }
                    else
                    {
                        ShowToolInfo(true, Translations.Translate(DriveCommon.TK_TERRAIN_SELECT), raycastOutput.m_hitPos);
                    }
                }
            }
            else
            {
                ShowToolInfo(false, null, Vector3.zero);
            }
        }

        private bool RaycastRoad(out RaycastOutput raycastOutput)
        {
            RaycastInput raycastInput = new RaycastInput(Camera.main.ScreenPointToRay(Input.mousePosition), Camera.main.farClipPlane);
            raycastInput.m_netService.m_service = ItemClass.Service.Road;
            raycastInput.m_netService.m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels;
            raycastInput.m_netService2.m_service = ItemClass.Service.Beautification;
            raycastInput.m_netService2.m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels;
            raycastInput.m_ignoreSegmentFlags = NetSegment.Flags.None;
            raycastInput.m_ignoreNodeFlags = NetNode.Flags.None;
            raycastInput.m_ignoreTerrain = false;

            return RayCast(raycastInput, out raycastOutput);
        }

        private Vector3 TerrainNormal(Vector3 pos)
        {
            Vector3 pos1 = pos + Vector3.right * 0.1f;
            Vector3 pos2 = pos + Vector3.forward * 0.1f;
            pos.y = TerrainHeight(pos);
            pos1.y = TerrainHeight(pos1);
            pos2.y = TerrainHeight(pos2);
            return Vector3.Cross(pos2 - pos, pos1 - pos).normalized;
        }

        private float TerrainHeight(Vector3 pos) {
            return Mathf.Max(Singleton<TerrainManager>.instance.SampleDetailHeightSmooth(pos), Singleton<TerrainManager>.instance.WaterLevel(new Vector2(pos.x, pos.z)));
        }
    }
}
