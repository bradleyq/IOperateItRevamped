using AlgernonCommons.Translation;
using ColossalFramework;
using UnityEngine;

namespace DriveIt.Utils
{
    public class DriveSelectTool : ToolBase
    {
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);
            if (!m_toolController.IsInsideUI && Cursor.visible)
            {
                if (MapUtils.RaycastRoad(out var raycastOutput))
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
            if (!m_toolController.IsInsideUI && Cursor.visible && MapUtils.RaycastRoad(out var raycastOutput))
            {
                ushort netSegmentId = raycastOutput.m_netSegment;

                if (netSegmentId != 0)
                {
                    NetSegment netSegment = Singleton<NetManager>.instance.m_segments.m_buffer[netSegmentId];

                    if (netSegment.m_flags.IsFlagSet(NetSegment.Flags.Created))
                    {
                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            netSegment.GetClosestPositionAndDirection(raycastOutput.m_hitPos, out _, out var dir);
                            netSegment.GetClosestLanePosition(raycastOutput.m_hitPos, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All, out Vector3 lanePos, out _, out _, out _);
                            dir = Vector3.Dot(Camera.main.transform.TransformDirection(Vector3.forward), dir) > 0.0f ? dir : -dir;
                            Quaternion rotation = Quaternion.LookRotation(dir);
                            DriveController.instance.StartDriving(lanePos, rotation);
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
                        position.y = MapUtils.TerrainHeight(position, out _);
                        Vector3 normal = MapUtils.TerrainNormal(position, out _);
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
    }
}
