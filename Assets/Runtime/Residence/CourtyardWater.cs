using UnityEngine;
using UnityEngine.Rendering;

namespace ArkWalk
{
    internal static class CourtyardWater
    {
        private static Mesh spillway;
        private static Material water;

        internal static void Place(Transform root, Vector3 lip, float width, float height)
        {
            if (!spillway) spillway = MakeSheet();
            if (!water) water = new Material(Resources.Load<Shader>("Residence/FallingWater"));
            var sheet = ResidentialKit.Group(root, "露台叠水", lip);
            sheet.localScale = new Vector3(width, height, 1f);
            sheet.gameObject.AddComponent<MeshFilter>().sharedMesh = spillway;
            var surface = sheet.gameObject.AddComponent<MeshRenderer>();
            surface.sharedMaterial = water;
            surface.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static Mesh MakeSheet()
        {
            const int rows = 16, columns = 21;
            var verts = new Vector3[rows * columns];
            var uv = new Vector2[verts.Length];
            var faces = new int[(rows - 1) * (columns - 1) * 6];
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                float drop = row / (float)(rows - 1);
                for (int col = 0; col < columns; col++)
                {
                    float x = col / (float)(columns - 1);
                    int v = row * columns + col;
                    uv[v] = new Vector2(x, drop);
                    float bulge = .024f * Mathf.Sin(x * 23f + drop * 3f);
                    verts[v] = new Vector3((x - .5f) * (1f + .10f * drop), -drop, -.70f * Mathf.Sqrt(drop) + bulge);
                    if (col == columns - 1 || row == rows - 1) continue;
                    faces[index++] = v;
                    faces[index++] = v + columns;
                    faces[index++] = v + 1;
                    faces[index++] = v + 1;
                    faces[index++] = v + columns;
                    faces[index++] = v + columns + 1;
                }
            }
            var mesh = new Mesh {name = "露台循环水幕", vertices = verts, uv = uv, triangles = faces};
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true);
            return mesh;
        }
    }
}
