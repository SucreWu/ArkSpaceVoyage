using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArkWalk
{
    internal static class ResidentialKit
    {
        private sealed class Part
        {
            internal string Label;
            internal Mesh Mesh;
            internal Material Material;
            internal bool Transparent;
        }

        private static readonly Dictionary<string, Part[]> models = new Dictionary<string, Part[]>();
        private static readonly Dictionary<string, Material> paints = new Dictionary<string, Material>();
        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private const string Root = "Residence/";

        internal static Transform Group(Transform parent, string name, Vector3 at)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            return go.transform;
        }

        internal static Transform Place(Transform parent, string key, Vector3 at, float yaw = 0f, float size = 1f)
        {
            var root = Group(parent, key, at);
            root.localRotation = Quaternion.Euler(0f,yaw,0f);
            root.localScale = Vector3.one * size;
            if (!models.TryGetValue(key, out var parts))
            {
                var bytes = Resources.Load<TextAsset>(Root + "Models/" + key);
                if (!bytes) throw new InvalidOperationException("缺少住宅网格 " + key);
                parts = ReadModel(bytes.bytes, key);
                models.Add(key, parts);
                Resources.UnloadAsset(bytes);
            }
            foreach (var part in parts)
            {
                var node = Group(root, part.Label, Vector3.zero);
                node.gameObject.AddComponent<MeshFilter>().sharedMesh = part.Mesh;
                var renderer = node.gameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = part.Material;
                if (part.Transparent) renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            return root;
        }

        private static string ReadString(BinaryReader reader)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadUInt16()));
        }

        private static Part[] ReadModel(byte[] data, string key)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "AWM2") throw new InvalidDataException("住宅网格数据无效");
                var parts = new Part[reader.ReadInt32()];
                for (int i = 0; i < parts.Length; i++)
                {
                    string label = ReadString(reader);
                    string image = ReadString(reader);
                    string normal = ReadString(reader);
                    var tint = new Color(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                    float metal = reader.ReadSingle(), smooth = reader.ReadSingle(), detail = reader.ReadSingle();
                    var glow = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    bool doubleSided = reader.ReadByte() != 0, glass = reader.ReadByte() != 0;
                    int count = reader.ReadInt32(), indexCount = reader.ReadInt32();
                    var verts = ReadVector3(reader,count);
                    var normals = ReadVector3(reader,count);
                    float[] mapped = ReadFloats(reader, count * 2);
                    var uv = new Vector2[count];
                    for (int j = 0; j < count; j++) uv[j] = new Vector2(mapped[j*2], mapped[j*2+1]);
                    byte[] colors = reader.ReadBytes(count*4);
                    var painted = new Color32[count];
                    for (int j = 0; j < count; j++) painted[j] = new Color32(colors[j*4],colors[j*4+1],colors[j*4+2],colors[j*4+3]);
                    var indices = new int[indexCount];
                    Buffer.BlockCopy(reader.ReadBytes(indexCount*4),0,indices,0,indexCount*4);
                    var mesh = new Mesh {name = key + "_" + label, indexFormat = count >= 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16};
                    mesh.vertices = verts;
                    mesh.normals = normals;
                    mesh.uv = uv;
                    mesh.colors32 = painted;
                    mesh.triangles = indices;
                    if (!string.IsNullOrEmpty(normal)) mesh.RecalculateTangents();
                    mesh.RecalculateBounds();
                    mesh.UploadMeshData(true);
                    var mat = MaterialFor(key + "/" + label,tint,image,normal,metal,smooth,detail,doubleSided);
                    if (mat.HasProperty("_CourtyardBounce"))
                    {
                        if (key=="terraces" || key=="court" || key=="atrium") mat.SetFloat("_CourtyardBounce",1f);
                        if (key.StartsWith("tree",StringComparison.Ordinal) && label!="Crown wood") mat.SetFloat("_LeafTransmission",1f);
                        if (key=="walkway" && !string.IsNullOrEmpty(normal)) mat.SetFloat("_Smoothness",.44f);
                    }
                    mat.SetColor("_EmissionColor",glow);
                    if (mat.HasProperty("_VertexColor")) mat.SetFloat("_VertexColor",1f);
                    if (glass)
                    {
                        mat.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite",0);
                        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface",1f);
                        mat.SetOverrideTag("RenderType","Transparent");
                        mat.renderQueue=(int)RenderQueue.Transparent;
                    }
                    parts[i] = new Part { Label=label, Mesh=mesh, Material=mat, Transparent=glass };
                }
                return parts;
            }
        }

        private static float[] ReadFloats(BinaryReader reader,int count)
        {
            var a = new float[count];
            Buffer.BlockCopy(reader.ReadBytes(count*4),0,a,0,count*4);
            return a;
        }

        private static Vector3[] ReadVector3(BinaryReader reader,int count)
        {
            float[] a=ReadFloats(reader,count*3);
            var values=new Vector3[count];
            for(int i=0;i<count;i++) values[i]=new Vector3(a[i*3],a[i*3+1],a[i*3+2]);
            return values;
        }

        internal static Texture2D Texture(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (!textures.TryGetValue(key,out var texture))
            {
                texture = Resources.Load<Texture2D>(Root+"Paint/"+key);
                if (!texture) throw new InvalidOperationException("缺少住宅贴图 "+key);
                textures[key]=texture;
            }
            return texture;
        }

        internal static Material MaterialFor(string key,Color tint,string image = "",string normal = "",float metal = 0f,float smooth = .22f,float bump = 1f,bool twoSided=false)
        {
            if (paints.TryGetValue(key,out var stored)) return stored;
            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline) shader=Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) shader = Resources.Load<Shader>(Root+"Surface");
            if (!shader) shader = Shader.Find("Standard");
            var mat = new Material(shader) {name=key};
            if(mat.HasProperty("_Color")) mat.SetColor("_Color",tint);
            if(mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor",tint);
            if (!string.IsNullOrEmpty(image))
            {
                var tex=Texture(image);
                if(mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex",tex);
                if(mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap",tex);
            }
            if (!string.IsNullOrEmpty(normal))
            {
                mat.SetTexture("_BumpMap",Texture(normal));
                mat.SetFloat("_BumpScale",bump);
                mat.EnableKeyword("_NORMALMAP");
            }
            mat.SetFloat("_Smoothness",smooth);
            mat.SetFloat("_Metallic",metal);
            mat.SetFloat("_Cull",twoSided?0:2);
            mat.enableInstancing = true;
            paints[key]=mat;
            return mat;
        }

        internal static BoxCollider Solid(Transform parent,string name,Vector3 at,Vector3 size,float yaw=0f)
        {
            var n=Group(parent,name,at);
            n.localRotation=Quaternion.Euler(0f,yaw,0f);
            var box=n.gameObject.AddComponent<BoxCollider>();
            box.size=size;
            return box;
        }
    }
}
