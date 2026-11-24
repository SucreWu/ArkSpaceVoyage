using UnityEngine;
using System.Collections.Generic;
namespace ArkWalk.Art {
public static class SaturnCrystalRibbon {
    // Coordinates are in the flight world's space. x=0 stays free for the pilot.
    public static Transform Build(Transform parent, Vector3 sunlightTowards) {
        var shader=Resources.Load<Shader>("SaturnCrystalRibbon");
        if(!shader||!shader.isSupported){Debug.LogError("Crystal ribbon shader unavailable");return null;}
        var root=new GameObject("Saturn crystal ribbon").transform;root.SetParent(parent,false);
        var own=root.gameObject.AddComponent<SaturnCrystalLifetime>();
        var mat=new Material(shader);own.Material=mat;mat.SetVector("_Sun",sunlightTowards.normalized);
        var nearMat=new Material(mat){name="Saturn near ice frost coat"};
        own.NearMaterial=nearMat;
        nearMat.SetFloat("_FrostCoat",1);
        var random=new System.Random(73109);
        float R()=> (float)random.NextDouble();
        void Crystal(Vector3 at,Vector3 size,int seed) {
            var p=new List<Vector3>();var indices=new List<int>();
            const int n=10, layers=8;
            var rings=new Vector3[layers,n];
            var rotation=Quaternion.Euler(-19f+R()*38f,R()*180f,R()*65f);
            for(int x=0;x<n;x++){
                float a=x*2*Mathf.PI/n, skew=.77f+R()*.43f;
                for(int y=0;y<layers;y++) {
                    float t=(float)y/(layers-1),r=(.21f+.79f*Mathf.Pow(Mathf.Sin(Mathf.PI*t),.51f))*(.86f+.27f*R())*skew;
                    rings[y,x]=rotation*Vector3.Scale(new Vector3(Mathf.Cos(a)*r+(t-.5f)*.34f,(t-.5f)*2+(.5f-R())*.18f,Mathf.Sin(a)*r),size);
                }
            }
            void Tri(Vector3 a,Vector3 b,Vector3 c) {
                if(Vector3.Dot(Vector3.Cross(b-a,c-a),a+b+c)<0){var z=b;b=c;c=z;}
                int i=p.Count;p.Add(a);p.Add(b);p.Add(c);indices.Add(i);indices.Add(i+1);indices.Add(i+2);
            }
            for(int i=0;i<n;i++) {
                int k=(i+1)%n;
                for(int j=0;j<layers-1;j++){
                    Tri(rings[j,i],rings[j+1,i],rings[j+1,k]);
                    Tri(rings[j,i],rings[j+1,k],rings[j,k]);
                }
                Tri(rings[0,i],rings[0,k],rotation*Vector3.down*size.y);
                Tri(rings[layers-1,i],rings[layers-1,k],rotation*Vector3.up*size.y);
            }
            var mesh=new Mesh{name=seed==0?"Keel Shard":"Lamellar ice chip"};mesh.SetVertices(p);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();own.Meshes.Add(mesh);
            var g=new GameObject(mesh.name);g.transform.SetParent(root,false);g.transform.localPosition=at;
            g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=nearMat;
        }
        Crystal(new Vector3(5.8f,-.32f,20.3f),new Vector3(2.30f,.69f,1.27f),0);
        for(int cluster=0;cluster<9;cluster++){
            float x=(cluster%3==0?-1f:1f)*(4.2f+R()*5),z=8.5f+cluster*7.36f;
            for(int i=0;i<3;i++) {
                float s=.15f+R()*.43f;
                Crystal(new Vector3(x+(.5f-R())*1.8f,-.25f-R()*1.39f,z+R()*2.7f),new Vector3(s*1.63f,s*.62f,s),1);
            }
        }
        MidBand(parent,mat,own);
        return root;
    }
    static void MidBand(Transform parent,Material mat,SaturnCrystalLifetime own) {
        // Render-only dispersed particle volume, not a card or collision surface.
        var field=new GameObject("Saturn intermediate ring fragments").transform;field.SetParent(parent,false);
        var random=new System.Random(681025);
        float R()=> (float)random.NextDouble();
        float Range(float a,float b)=>Mathf.Lerp(a,b,R());
        for(int slice=0;slice<4;slice++)for(int side=-1;side<=1;side+=2){
            var verts=new List<Vector3>();var tris=new List<int>();
            for(int j=0;j<104;j++){
                float z=Range(32+slice*10,43+slice*10);
                float t=(z-32)/41f;
                float sweep=5.9f+11f*t;
                float spread=(R()+R()+R()-1.5f)*(12+10*t);
                float x=side*Mathf.Max(4.8f+Range(0,2.6f),sweep+spread);
                float y=Mathf.Lerp(-1.25f,2.15f,t)+.073f*x+(R()+R()-1)*3.1f;
                float size=.065f+.47f*Mathf.Pow(R(),3.4f)+(R()<.045f?.25f:0);
                var origin=new Vector3(x,y,z);
                var rot=Quaternion.Euler(Range(-30,30),Range(0,360),Range(-35,35));
                var q=new Vector3[6];
                q[0]=new Vector3(Range(-.34f,.34f),Range(.52f,.92f),Range(-.3f,.3f));
                q[5]=new Vector3(Range(-.31f,.31f),-Range(.36f,.81f),Range(-.25f,.25f));
                for(int k=1;k<=4;k++){
                    float a=(k-1)*Mathf.PI/2+Range(-.30f,.30f);
                    q[k]=new Vector3(Mathf.Cos(a)*Range(.65f,1.30f),Range(-.28f,.27f),Mathf.Sin(a)*Range(.62f,1.16f));
                }
                for(int k=0;k<6;k++)q[k]=rot*Vector3.Scale(q[k],new Vector3(size*1.23f,size*.45f,size));
                void Tri(Vector3 a,Vector3 b,Vector3 c){
                    if(Vector3.Dot(Vector3.Cross(b-a,c-a),a+b+c)<0){var swap=b;b=c;c=swap;}
                    int at=verts.Count;verts.Add(origin+a);verts.Add(origin+b);verts.Add(origin+c);
                    tris.Add(at);tris.Add(at+1);tris.Add(at+2);
                }
                for(int k=1;k<=4;k++){int next=k%4+1;Tri(q[0],q[k],q[next]);Tri(q[5],q[next],q[k]);}
            }
            var mesh=new Mesh{name="Saturn varied middle-depth fragments"};
            mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();own.Meshes.Add(mesh);
            var piece=new GameObject("Ring fragment volume");piece.transform.SetParent(field,false);
            piece.AddComponent<MeshFilter>().sharedMesh=mesh;piece.AddComponent<MeshRenderer>().sharedMaterial=mat;
        }
    }
}
public sealed class SaturnCrystalLifetime:MonoBehaviour {
    public Material Material,NearMaterial;
    public readonly List<Mesh> Meshes=new List<Mesh>();
    void OnDestroy(){foreach(var m in Meshes)if(m)Destroy(m);if(Material)Destroy(Material);if(NearMaterial)Destroy(NearMaterial);}
}
}
