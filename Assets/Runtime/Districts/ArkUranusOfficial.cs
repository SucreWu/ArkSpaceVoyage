using UnityEngine;
using System.IO;
namespace ArkWalk {
    internal sealed class ArkUranusOfficial : MonoBehaviour {
        internal Mesh OriginalMesh {get;private set;}
        internal Material OriginalMat {get;private set;}
        internal static void Apply(DistrictSet d,Transform planet) {
            var data=Resources.Load<TextAsset>("Districts/uranus_vtad_mesh");
            var tex=Resources.Load<Texture2D>("Districts/uranus_vtad_atlas");
            if(!data||!tex||data.bytes.Length!=140480)return;
            const int count=3256,indexCount=18144;
            var pos=new Vector3[count];var normals=new Vector3[count];var uv=new Vector2[count];var idx=new int[indexCount];
            using(var r=new BinaryReader(new MemoryStream(data.bytes))) {
                // Apply the GLB node's +90 deg rotation about X, reflect X for Unity;
                // unscale the retained collider's local Y scale for the rendered mesh only.
                for(int i=0;i<count;i++){float x=r.ReadSingle(),y=r.ReadSingle(),z=r.ReadSingle();pos[i]=new Vector3(-x,-z*(32f/31f),y)/500f;}
                for(int i=0;i<count;i++){float x=r.ReadSingle(),y=r.ReadSingle(),z=r.ReadSingle();normals[i]=new Vector3(-x,-z*(31f/32f),y).normalized;}
                for(int i=0;i<count;i++)uv[i]=new Vector2(r.ReadSingle(),1-r.ReadSingle());
                for(int i=0;i<indexCount;i+=3){idx[i]=r.ReadUInt16();idx[i+2]=r.ReadUInt16();idx[i+1]=r.ReadUInt16();}
            }
            var mf=planet.GetComponent<MeshFilter>();var renderer=planet.GetComponent<MeshRenderer>();
            var q=planet.gameObject.AddComponent<ArkUranusOfficial>();q.OriginalMesh=mf.sharedMesh;q.OriginalMat=renderer.sharedMaterial;
            var mesh=d.Own(new Mesh{name="NASA VTAD Uranus oblate"});mesh.vertices=pos;mesh.normals=normals;mesh.uv=uv;mesh.triangles=idx;mesh.RecalculateBounds();
            Debug.Log($"URANUS VTAD {mesh.vertexCount} vertices / {indexCount/3} triangles / {tex.width}x{tex.height}; equator/pole ratio {mesh.bounds.extents.y*31/(mesh.bounds.extents.x*32):F5}");
            var material=d.Mat("NASA Uranus visual atmosphere","FFFFFF",null,0,.14f,.12f);
            material.mainTexture=tex;mf.sharedMesh=mesh;renderer.sharedMaterial=material;
        }
    }
}
