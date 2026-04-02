using UnityEngine;

namespace DriveIt.Code.Utils
{
    public class DriveTrailRenderer : MonoBehaviour
    {
        const int MAX_MARKS = 64;

        struct MarkSection
        {
            public Vector3 pos;
            public Vector3 normal;
        }

        private MarkSection[]   marks = new MarkSection[MAX_MARKS];
        private int             markIndex = 0;
        private int             lastMarkIndex = 0;
        private Mesh            mesh;
        private Vector3[]       vertices = new Vector3[MAX_MARKS * 4];
        private Color[]         colors = new Color[MAX_MARKS * 4];
        private int[]           triangles = new int[MAX_MARKS * 6];
        public bool             drawing = false;

        void Awake()
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }

        public void AddSkidMark(Vector3 pos, Vector3 normal, float intensity, float width, bool restart)
        {
            intensity = Mathf.Clamp01(intensity);

            MarkSection cur = new MarkSection();
            cur.pos = pos + normal * 0.02f;
            cur.normal = normal;
            cur.normal = normal;

            marks[markIndex] = cur;

            if (!restart)
            {
                UpdateMesh(intensity, width);
            }

            lastMarkIndex = markIndex;
            markIndex = (markIndex + 1) % MAX_MARKS;
        }

        private void UpdateMesh(float intensity, float width)
        {
            Vector3 dir = marks[markIndex].pos - marks[lastMarkIndex].pos;
            Vector3 sideways = Vector3.Cross(dir, marks[markIndex].normal).normalized * width * 0.5f;

            int v = markIndex * 4;

            vertices[v + 0] = marks[markIndex].pos + sideways;
            vertices[v + 1] = marks[markIndex].pos - sideways;
            vertices[v + 2] = marks[lastMarkIndex].pos + sideways;
            vertices[v + 3] = marks[lastMarkIndex].pos - sideways;

            Color c = new Color(0, 0, 0, intensity);

            colors[v + 0] = c;
            colors[v + 1] = c;
            colors[v + 2] = c;
            colors[v + 3] = c;

            int t = markIndex * 6;

            triangles[t + 0] = v + 0;
            triangles[t + 1] = v + 2;
            triangles[t + 2] = v + 1;

            triangles[t + 3] = v + 2;
            triangles[t + 4] = v + 3;
            triangles[t + 5] = v + 1;

            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }
    }
}