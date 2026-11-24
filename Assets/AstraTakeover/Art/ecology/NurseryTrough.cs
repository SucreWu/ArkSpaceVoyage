using UnityEngine;
using System.Collections.Generic;

namespace ArkWalk.Art {
public static class NurseryTrough {
    public static void Build(Transform parent, Vector3 center, float rotation=0f) {
        var g=new GameObject("Nursery / split ceramic germination trough");var t=g.transform;t.SetParent(parent,false);t.localPosition=center;t.localRotation=Quaternion.Euler(0,rotation,0);
        var ceramic=Mat("Fired mineral ceramic",new Color(.56f,.49f,.38f),.18f);
        var fibre=Mat("Compression-moulded flax",new Color(.22f,.20f,.15f),.09f);
        var soil=Mat("Damp coir and mineral substrate",new Color(.25f,.19f,.135f),.045f);
        var olive=Mat("Nursling mature leaves",new Color(.26f,.38f,.20f),.22f);
        var tip=Mat("Nursling new leaves",new Color(.37f,.44f,.22f),.21f);
        ceramic.mainTexture=Noise(8,false);fibre.mainTexture=Noise(19,true);soil.mainTexture=Noise(44,false);
        Bowl(t,"Pressed flax lifting frame",fibre,.58f,.72f,.10f,.31f,1.02f,.35f);
        Bowl(t,"Bevelled fired body and rolled rim",ceramic,.69f,.77f,.28f,.67f,1.27f,.46f);
        Bowl(t,"Inset coir surface",soil,.56f,.62f,.55f,.68f,1.15f,.676f);
        var c=g.AddComponent<BoxCollider>();c.center=new Vector3(0,.33f,0);c.size=new Vector3(2.54f,.66f,1.51f);
        var random=new System.Random(591);
        float Rand()=> (float)random.NextDouble();
        var vp=new List<Vector3>();var fp=new List<int>();var young=new List<int>();
        for(int i=0;i<13;i++){
            float a=i*2.39996f,r=Mathf.Sqrt((i+.2f)/13),x=Mathf.Cos(a)*r*1.04f,z=Mathf.Sin(a)*r*.49f;
            for(int j=0;j<5;j++){
                var indices=j==0?young:fp;int k=vp.Count;float dir=j*1.2566f+Rand(),len=.13f+.26f*Rand();
                for(int n=0;n<=6;n++){
                    float u=n/6f,b=(.001f+.062f*Mathf.Sin(Mathf.PI*u))*Mathf.Min(1,u*2f);
                    float dist=len*u*u,h=.692f+len*(1.37f*u-.65f*u*u);
                    for(int s=-1;s<=1;s++)vp.Add(new Vector3(x+Mathf.Cos(dir)*dist-Mathf.Sin(dir)*b*s,h-.34f*b*Mathf.Abs(s),z+Mathf.Sin(dir)*dist+Mathf.Cos(dir)*b*s));
                    if(n>0)for(int m=0;m<2;m++){int q=k+(n-1)*3+m; indices.AddRange(new[]{q,q+1,q+4,q,q+4,q+3,q+4,q+1,q,q+3,q+4,q});}
                }
            }
        }
        var blades=new Mesh{name="Folded seedling leaves, front and back"};
        // Opposite wound faces need independent normals.
        var positions=new List<Vector3>();var front=new List<int>();var fresh=new List<int>();
        for(int s=0;s<2;s++)foreach(int id in s==0?fp:young){(s==0?front:fresh).Add(positions.Count);positions.Add(vp[id]);}
        blades.SetVertices(positions);blades.subMeshCount=2;blades.SetTriangles(front,0);blades.SetTriangles(fresh,1);blades.RecalculateNormals();Attach(t,"Leaf colonies",blades,new[]{olive,tip});
    }
    public static void AddLocalRim(Transform parent,Vector3 position,Vector3 aim) {
        var g=new GameObject("Nursery neutral-white rim fill");g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.LookAt(parent.TransformPoint(aim));
        var l=g.AddComponent<Light>();l.type=LightType.Spot;l.color=new Color(1f,.955f,.875f);l.range=4f;l.spotAngle=55f;l.innerSpotAngle=36f;l.intensity=.34f;l.shadows=LightShadows.None;
    }
    static Texture2D Noise(int seed,bool fibres) {
        var rng=new System.Random(seed);var tex=new Texture2D(64,64,TextureFormat.RGB24,true);tex.name=fibres?"Fine cross-laid flax":"Tiled mineral flecks";
        var colors=new Color[4096];
        for(int y=0;y<64;y++)for(int x=0;x<64;x++){float n=(float)rng.NextDouble();float v=.77f+.20f*n;
            if(fibres)v+=(x%13==0||y%17==0)?-.09f:0;
            else if(n>.975f)v=.55f;
            colors[x+64*y]=new Color(v,v,v);}
        tex.SetPixels(colors);tex.Apply(true,true);return tex;
    }
    static Material Mat(string name,Color tint,float smooth) {
        var m=new Material(Shader.Find("Standard")){name=name,color=tint};m.SetFloat("_Glossiness",smooth);m.SetFloat("_Metallic",0);return m;
    }
    static void Bowl(Transform t,string name,Material m,float zb,float zt,float bottom,float top,float x,float inner) {
        var v=new List<Vector3>();var uv=new List<Vector2>();var tr=new List<int>();
        float[] xx={x*.77f,x*.86f,x*.98f,x,x*.96f,x*.88f,x*.84f,0};
        float[] zz={zb*.91f,zb,zt*.97f,zt,zt*.98f,zt*.89f,zt*.87f,0};
        float[] yy={bottom,bottom+.045f,top-.08f,top-.028f,top,top-.009f,inner,inner};
        for(int j=0;j<yy.Length;j++)for(int k=0;k<=40;k++){
            float a=k*Mathf.PI/20,c=Mathf.Cos(a),s=Mathf.Sin(a);
            v.Add(new Vector3(xx[j]*Mathf.Sign(c)*Mathf.Pow(Mathf.Abs(c),.70f),yy[j],zz[j]*Mathf.Sign(s)*Mathf.Pow(Mathf.Abs(s),.65f)));
            uv.Add(new Vector2(k/5f,j*.54f));
            if(j>0&&k>0){int p=(j-1)*41+k-1;tr.AddRange(new[]{p,p+41,p+1,p+1,p+41,p+42});}
        }
        var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();Attach(t,name,mesh,new[]{m});
    }
    static void Attach(Transform t,string name,Mesh mesh,Material[] materials){
        var g=new GameObject(name);g.transform.SetParent(t,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterials=materials;
    }
}
}

