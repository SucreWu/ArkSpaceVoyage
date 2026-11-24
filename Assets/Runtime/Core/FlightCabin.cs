using UnityEngine;
using UnityEngine.Rendering;

namespace ArkWalk
{
    internal static class FlightCabin
    {
        internal static GameObject Create(Transform eye)
        {
            var root = new GameObject("穿梭机前舱");
            root.transform.SetParent(eye, false);
            var shell = Finish("消光舱壳", new Color(.15f,.19f,.21f), .36f);
            var rim = Finish("银灰喷砂", new Color(.45f,.52f,.52f), .51f);
            var dark = Finish("仪表玻璃", new Color(.025f,.075f,.084f), .64f);
            var aqua = Finish("仪表背光", new Color(.36f,.84f,.79f), .3f, true);
            var gold = Finish("航向指示灯", new Color(.98f,.68f,.28f), .23f, true);
            for (int i = -1; i <= 1; i += 2)
            {
                var a = new Vector3(i * .68f, -.52f, 1.0f);
                var b = new Vector3(i * 1.23f, .74f, 1.28f);
                Beam(root.transform, a, b, .055f, .095f, shell);
                Beam(root.transform, a + Vector3.left * i * .03f, b + Vector3.left * i * .03f, .008f, .08f, rim);
                var side = Part(root.transform, "侧仪表", new Vector3(i * .47f,-.52f,.97f), new Vector3(.52f,.08f,.35f), shell);
                side.localRotation = Quaternion.Euler(-12, -i * 15, 0);
                for (int n = 0; n < 4; n++)
                    Part(side, "航灯", new Vector3((n - 1.5f) * .16f,.52f,.04f), new Vector3(.082f,.05f,.30f), n == 0 ? gold : aqua);
            }
            Part(root.transform, "前窗下沿", new Vector3(0,-.555f,1.05f),new Vector3(1.56f,.11f,.14f), shell);
            Part(root.transform, "仪表嵌座", new Vector3(0,-.51f,.99f),new Vector3(.53f,.09f,.19f), rim);
            Part(root.transform, "电子姿态仪", new Vector3(0,-.499f,.89f),new Vector3(.49f,.076f,.025f), dark);
            for (int n = -6; n <= 6; n++)
                Part(root.transform, "航向刻度", new Vector3(n*.030f,-.494f,.875f), new Vector3(.011f,n % 3 == 0 ? .026f : .012f,.004f), n == 0 ? gold : aqua);
            root.SetActive(false);
            return root;
        }

        private static Material Finish(string name, Color tone, float smooth, bool luminous = false)
        {
            var material = new Material(Shader.Find("Standard")) { name = name, color = tone };
            material.SetFloat("_Glossiness", smooth);
            if (luminous)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", tone * .72f);
            }
            return material;
        }

        private static Transform Part(Transform parent, string name, Vector3 point, Vector3 size, Material finish)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.layer = WalkPilot.PlayerLayer;
            Object.Destroy(obj.GetComponent<Collider>());
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = point;
            obj.transform.localScale = size;
            var surface = obj.GetComponent<Renderer>();
            surface.sharedMaterial = finish;
            surface.shadowCastingMode = ShadowCastingMode.Off;
            return obj.transform;
        }

        private static void Beam(Transform parent, Vector3 start, Vector3 end, float width, float depth, Material finish)
        {
            var tr = Part(parent, "侧窗框", (start+end)*.5f, new Vector3(width,(end-start).magnitude,depth), finish);
            tr.localRotation = Quaternion.FromToRotation(Vector3.up, end-start);
        }
    }
}
