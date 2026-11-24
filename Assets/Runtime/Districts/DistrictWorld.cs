using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArkWalk
{
    public static class DistrictWorld
    {
        public static void Build(WalkSite site)
        {
            var room = site.gameObject.AddComponent<DistrictSet>();
            room.Begin(site);
            switch (site.Id)
            {
                case "ecology": DistrictRooms.Ecology(room); break;
                case "clinic": DistrictRooms.Clinic(room); break;
                case "dock": DistrictDock.Build(room); break;
                case "bridge": DistrictRooms.Bridge(room); break;
                default: DistrictSpace.Build(room, site.Id); break;
            }
            room.Finish();
        }
    }

    internal sealed class DistrictSet : MonoBehaviour
    {
        internal WalkSite Site { get; private set; }
        internal readonly System.Random Seed = new System.Random(67123);
        readonly Dictionary<Material, List<CombineInstance>> batches = new Dictionary<Material, List<CombineInstance>>();
        readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        static readonly Dictionary<PrimitiveType, Mesh> primitives = new Dictionary<PrimitiveType, Mesh>();
        internal static Vector3 V(float x, float y, float z) { return new Vector3(x,y,z); }
        internal static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var result); return result; }
        internal float Random(float min, float max) { return Mathf.Lerp(min, max, (float)Seed.NextDouble()); }
        internal void Begin(WalkSite site) { Site = site; }
        internal Transform Group(string group, Transform parent = null)
        {
            var t = new GameObject(group).transform;
            t.SetParent(parent ? parent : transform, false);
            return t;
        }
        internal static Mesh Primitive(PrimitiveType kind)
        {
            if (primitives.TryGetValue(kind, out var known) && known) return known;
            var temporary = GameObject.CreatePrimitive(kind);
            temporary.SetActive(false);
            var mesh = temporary.GetComponent<MeshFilter>().sharedMesh;
            primitives[kind] = mesh;
            if (Application.isPlaying) Destroy(temporary); else DestroyImmediate(temporary);
            return mesh;
        }
        internal Material Mat(string key, string hex, string source = null, float metal = 0, float gloss = .35f, float light = 0, float alpha = 1, float repeat = 1)
        {
            if (materials.TryGetValue(key, out var known)) return known;
            var shader = GraphicsSettings.currentRenderPipeline ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
            if (!shader) shader = Shader.Find("Standard");
            var m = new Material(shader) { name = key, enableInstancing = true };
            var c = C(hex); c.a = alpha;
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (source != null)
            {
                var image = Resources.Load<Texture2D>("Districts/" + source);
                m.mainTexture = image;
                m.mainTextureScale = new Vector2(repeat, repeat);
                if (m.HasProperty("_BaseMap")) { m.SetTexture("_BaseMap", image); m.SetTextureScale("_BaseMap", m.mainTextureScale); }
            }
            m.SetFloat("_Metallic",metal);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness",gloss);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",gloss);
            if (light > 0)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", c * light);
                if (source != null && m.HasProperty("_EmissionMap")) m.SetTexture("_EmissionMap",m.mainTexture);
            }
            if (alpha < 1 || key == "环带")
            {
                m.SetOverrideTag("RenderType", "Transparent");
                if (m.HasProperty("_Mode")) m.SetFloat("_Mode", 3);
                if (m.HasProperty("_Surface")) m.SetFloat("_Surface",1);
                m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite",0);
                m.EnableKeyword("_ALPHABLEND_ON");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = 3000;
            }
            materials[key] = m; owned.Add(m);
            return m;
        }
        internal Material Steel => Mat("拉丝钢","AAB7B7",null,.68f,.52f);
        internal Material Dark => Mat("石墨框","33454B",null,.45f,.35f);
        internal Material Hull => Mat("舱壁","E7DED0","hull",.17f,.41f);
        internal Material Panels => Mat("检修面板","DDD7CE","pressure_case",.32f,.49f);
        internal Material Gold => Mat("工作黄","ECA65B",null,.35f,.42f);
        internal Material Light => Mat("灯管","FFE6B6",null,0,.24f,1.3f);
        internal Material Aqua => Mat("状态灯","78EFE1",null,0,.47f,.77f);
        internal Material Glass => Mat("透光舱盖","9CDFEE",null,.05f,.89f,0,.20f);
        internal Material Console => Mat("仪器表盘","D6FFFC","console",.16f,.51f,.19f);
        internal Material Oak => Mat("暖木","ECE2C5","PorchOak_BaseColor",.04f,.37f);
        internal void Add(Mesh mesh, Vector3 p, Vector3 scale, Material material, Quaternion q)
        {
            if (!batches.TryGetValue(material, out var group)) batches[material] = group = new List<CombineInstance>();
            group.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(p, q, scale) });
        }
        internal void Box(Vector3 p, Vector3 size, Material mat, bool solid = false, Vector3 angle = default)
        {
            var q = Quaternion.Euler(angle);
            Add(Primitive(PrimitiveType.Cube), p, size, mat,q);
            if (solid) Block(p,size,q);
        }
        internal void Block(Vector3 p, Vector3 size, Quaternion rotation)
        {
            var t = Group("通行边界"); t.localPosition = p; t.localRotation = rotation;
            t.gameObject.AddComponent<BoxCollider>().size = size;
        }
        internal void Ball(Vector3 p, Vector3 size, Material mat)
        {
            Add(Primitive(PrimitiveType.Sphere),p,size,mat,Quaternion.identity);
        }
        internal void Rod(Vector3 a, Vector3 b, float r, Material mat)
        {
            var v = b-a;
            Add(Primitive(PrimitiveType.Cylinder), (a+b)*.5f,V(r*2,v.magnitude*.5f,r*2),mat,Quaternion.FromToRotation(Vector3.up,v));
        }
        internal Texture2D Own(Texture2D texture) { owned.Add(texture); return texture; }
        internal Mesh Own(Mesh mesh) { owned.Add(mesh); return mesh; }
        internal Transform Visual(string name, Mesh mesh, Material mat, Transform parent, Vector3 p, Vector3 scale, Quaternion rotation)
        {
            var t = Group(name,parent); t.localPosition=p; t.localRotation=rotation; t.localScale=scale;
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=t.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial=mat;
            if (mat.renderQueue >= 3000) { renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false; }
            return t;
        }
        internal Transform Limb(Transform parent, Vector3 a, Vector3 b, float r, Material mat)
        {
            var v=b-a;
            return Visual("铰接件",Primitive(PrimitiveType.Cylinder),mat,parent,(a+b)*.5f,V(2*r,v.magnitude*.5f,2*r),Quaternion.FromToRotation(Vector3.up,v));
        }
        internal Transform Model(string file, Material mat, Transform parent, Vector3 p, Vector3 scale, float yaw = 0)
        {
            var prefab = Resources.Load<GameObject>("Districts/"+file);
            if (!prefab) return null;
            var t=Instantiate(prefab,parent).transform; t.localPosition=p; t.localScale=scale; t.localRotation=Quaternion.Euler(0,yaw,0);
            foreach(var r in t.GetComponentsInChildren<MeshRenderer>()) { var ms=r.sharedMaterials; for(int i=0;i<ms.Length;i++) ms[i]=mat; r.sharedMaterials=ms; }
            return t;
        }
        internal void Lamp(Vector3 p, Color color, float intensity, float range)
        {
            var t=Group("工作灯"); t.localPosition=p; var l=t.gameObject.AddComponent<Light>(); l.type=LightType.Point; l.range=range; l.intensity=intensity; l.color=color; l.shadows=LightShadows.None;
        }
        internal void Sun(Vector3 dir, Color tint, float intensity, bool soft = false)
        {
            var t=Group("舷外日照"); t.rotation=Quaternion.LookRotation(dir); var l=t.gameObject.AddComponent<Light>(); l.type=LightType.Directional; l.color=tint; l.intensity=intensity; l.shadows=soft?LightShadows.Soft:LightShadows.None; l.shadowStrength=.62f; l.shadowBias=.045f;
        }
        internal void Mark(Vector3 p,float radius,string label,string speech,Action action=null) { Site.Mark(transform.TransformPoint(p),radius,label,speech,action); }
        internal void Finish()
        {
            foreach(var pair in batches)
            {
                var chunk=new List<CombineInstance>(); int vertices=0;
                foreach(var piece in pair.Value)
                {
                    if (vertices+piece.mesh.vertexCount>58000 && chunk.Count>0) { Flush(pair.Key,chunk); chunk.Clear(); vertices=0; }
                    vertices+=piece.mesh.vertexCount; chunk.Add(piece);
                }
                if (chunk.Count>0) Flush(pair.Key,chunk);
            }
            batches.Clear();
        }
        void Flush(Material m,List<CombineInstance> parts)
        {
            var mesh=Own(new Mesh { name=m.name }); mesh.CombineMeshes(parts.ToArray(),true,true); mesh.RecalculateBounds();
            Visual(m.name,mesh,m,transform,Vector3.zero,Vector3.one,Quaternion.identity);
        }
        void OnDestroy() { foreach(var o in owned) if(o) { if(Application.isPlaying) Destroy(o); else DestroyImmediate(o); } }
    }
}
