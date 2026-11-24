using System.IO;
using UnityEngine;

namespace ArkWalk {
    internal static class ArkJupiter {
        internal static void Build(DistrictSet d, Mesh fallback, string title) {
            var data=Resources.Load<TextAsset>("Districts/jupiter_nasa_vtad_mesh");
            var atlas=Resources.Load<Texture2D>("Districts/jupiter_nasa_vtad_atlas");
            Mesh mesh=fallback;
            bool useModel=data && atlas && data.bytes.Length==71200;
            if(useModel) {
                const int count=1649, indexCount=9216;
                var pos=new Vector3[count];var norm=new Vector3[count];var uv=new Vector2[count];var indices=new int[indexCount];
                using(var rd=new BinaryReader(new MemoryStream(data.bytes))) {
                    // Reflect X to Unity handedness; keep linked vertex normals and authored atlas coordinates.
                    for(int i=0;i<count;i++)pos[i]=new Vector3(-rd.ReadSingle(),rd.ReadSingle(),rd.ReadSingle())/500f;
                    for(int i=0;i<count;i++)norm[i]=new Vector3(-rd.ReadSingle(),rd.ReadSingle(),rd.ReadSingle());
                    for(int i=0;i<count;i++)uv[i]=new Vector2(rd.ReadSingle(),1f-rd.ReadSingle());
                    for(int i=0;i<indexCount;i+=3){indices[i]=rd.ReadUInt16();indices[i+2]=rd.ReadUInt16();indices[i+1]=rd.ReadUInt16();}
                }
                mesh=d.Own(new Mesh{name="NASA VTAD Jupiter"});mesh.vertices=pos;mesh.normals=norm;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateBounds();
                Debug.Log($"JUPITER VTAD {mesh.vertexCount} vertices / {indexCount/3} triangles; atlas {atlas.width}x{atlas.height}");
            }
            var mat=d.Mat(title,"FFFFFF",useModel?"jupiter_nasa_vtad_atlas":"jupiter",0,.14f);
            var t=d.Visual(title,mesh,mat,d.transform,new Vector3(13,8,120),Vector3.one*35,Quaternion.Euler(0,18,0));
            t.gameObject.AddComponent<SphereCollider>().radius=1;
        }
    }
}
