using UnityEngine;
using System.IO;
namespace ArkWalk {
internal static class ArkMarsArtTrial {
    internal static void Capture(Camera eye, System.Action<string> frame, System.Action<bool,string> check) {
        var data=Resources.Load<TextAsset>("Districts/mars_trial_mesh");
        var atlas=Resources.Load<Texture2D>("Districts/mars_trial_atlas");
        var normal=Resources.Load<Texture2D>("Districts/mars_trial_normal");
        var retained=Resources.Load<Material>("Districts/mars_trial_lit");
        bool ready=retained && data && data.bytes.Length==83520 && atlas && normal && atlas.width==2048 && atlas.height==1536 && normal.width==2048 && normal.height==1536;
        check(ready,"NASA Mars source and full atlases loaded");
        if(!ready)return;
        Mesh mesh;
            if(true) {
                const int count=2034, indexCount=9216;
                var pos=new Vector3[count];var norm=new Vector3[count];var uv=new Vector2[count];var indices=new int[indexCount];
                using(var rd=new BinaryReader(new MemoryStream(data.bytes))) {
                    // Reflect X to Unity handedness; keep linked vertex normals and authored atlas coordinates.
                    for(int i=0;i<count;i++)pos[i]=new Vector3(-rd.ReadSingle(),rd.ReadSingle(),rd.ReadSingle())/500f;
                    for(int i=0;i<count;i++)norm[i]=new Vector3(-rd.ReadSingle(),rd.ReadSingle(),rd.ReadSingle());
                    for(int i=0;i<count;i++)uv[i]=new Vector2(rd.ReadSingle(),1f-rd.ReadSingle());
                    for(int i=0;i<indexCount;i+=3){indices[i]=rd.ReadUInt16();indices[i+2]=rd.ReadUInt16();indices[i+1]=rd.ReadUInt16();}
                }
                mesh=new Mesh{name="NASA VTAD Mars Trial"};mesh.vertices=pos;mesh.normals=norm;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateBounds();mesh.RecalculateTangents();
                Debug.Log($"MARS VTAD {mesh.vertexCount} vertices / {indexCount/3} triangles; atlas {atlas.width}x{atlas.height}");
            }

        var trial=new GameObject("QA only / Mars orbit");
        var at=new Vector3(0,14,110); trial.transform.position=at;trial.transform.localScale=Vector3.one*35;trial.transform.rotation=Quaternion.Euler(0,18,0);
        var mat=new Material(retained){mainTexture=atlas};
        mat.SetFloat("_Metallic",0);mat.SetFloat("_Glossiness",.10f);mat.SetTexture("_BumpMap",normal);mat.EnableKeyword("_NORMALMAP");
        trial.AddComponent<MeshFilter>().sharedMesh=mesh;trial.AddComponent<MeshRenderer>().sharedMaterial=mat;
        var ground=GameObject.Find("Martian canyon");var groundMesh=ground?ground.GetComponent<Renderer>():null;
        var pose=eye.transform;var orig=pose.position;var rot=pose.rotation;
        try {
            if(groundMesh)groundMesh.enabled=false;
            var axis=new Vector3(-.40f,.23f,-1f).normalized;
            System.Action<float> turn = distance => {
                pose.position=at+axis*distance;pose.rotation=Quaternion.LookRotation(-axis,Vector3.up);
            };
            turn(105);frame("10-Mars-OrbitTrial");
            turn(40);frame("11-Mars-CloseNormal");
            mat.DisableKeyword("_NORMALMAP");frame("11-Mars-CloseColorOnly");
            pose.rotation=Quaternion.LookRotation(Vector3.Lerp(-axis,Vector3.right,.65f).normalized,Vector3.up);
            mat.EnableKeyword("_NORMALMAP");frame("12-Mars-CloseLimb");
        } finally {
            pose.SetPositionAndRotation(orig,rot);
            if(groundMesh)groundMesh.enabled=true;
            trial.SetActive(false);
            Object.Destroy(trial);Object.Destroy(mesh);Object.Destroy(mat);
        }
    }
}}
