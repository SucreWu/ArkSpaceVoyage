using UnityEngine;
using static ArkWalk.DistrictSet;
namespace ArkWalk {
internal static class ArkInnerWorlds {
    // Artistic distances and relief, not physical scale. No runtime downloads.
    internal static void Earth(DistrictSet d) {
        var center=V(-12,10,114);var size=Vector3.one*69;
        var globe=d.Visual("Ocean and continents",ArkEarthGlobe.Map(d,Primitive(PrimitiveType.Sphere)),d.Mat("Earth albedo","FFFFFF","earth_nasa_bmng_8k",0,.19f),d.transform,center,size,Quaternion.Euler(12,74,0));
        globe.gameObject.AddComponent<SphereCollider>().radius=.5f;
        ArkEarthLimb.Build(d,globe);
        ArkEarthOceanTrial.Build(globe);
        ArkEarthCloudHistoric.Build(d,globe);
        // Fictional service telescope, a truss with two photovoltaic wings.
        var p=V(-7,3,24);var cell=d.Mat("Cobalt photovoltaic cells","243975",null,.38f,.48f);
        d.Rod(p+V(0,0,-2),p+V(0,0,2),1.05f,d.Panels);
        d.Rod(p+V(0,0,-2.17f),p+V(0,0,-2.05f),.85f,d.Dark);
        d.Rod(p+V(-6,0,0),p+V(6,0,0),.1f,d.Steel);
        for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++){
            var c=p+V(side*(1.8f+i*1.12f),0,0); d.Box(c,V(1.05f,.12f,3.2f),cell,true);
            for(int j=-2;j<=2;j++)d.Box(c+V(0,.08f,.58f*j),V(1.03f,.035f,.032f),d.Gold);
        }
        var core=d.Group("Observatory hull collision");core.localPosition=p;core.gameObject.AddComponent<BoxCollider>().size=V(2.15f,2.15f,4.3f);
        d.Mark(p+V(0,0,-2.5f),2.6f,"轨道望远镜","这是一座虚构的轨道观测站。绕过太阳翼，从镜筒旁看地球。");
        d.Mark(V(-12,10,77),9,"蓝色地球","蓝色的海洋和白色的云层覆盖着地球。这里的距离为漫游作了压缩。");
    }
    static float Canyon(float x,float z) {
        float course=3.7f*Mathf.Sin(z*.07f)+.065f*z;
        float edge=Mathf.Abs(x-course);float ledge=Mathf.SmoothStep(0,1,(edge-5.2f)/9f);
        float detail=Mathf.PerlinNoise(x*.105f+42,z*.105f+55);
        return -7.5f+ledge*(11.8f+4*Mathf.Sin(z*.11f))+.9f*detail+.65f*Mathf.Sin(edge*2.3f)*ledge;
    }
    internal static void Mars(DistrictSet d) {
        const int w=91,h=141;var pos=new Vector3[w*h];var uv=new Vector2[pos.Length];
        var indices=new System.Collections.Generic.List<int>[] {new System.Collections.Generic.List<int>(),new System.Collections.Generic.List<int>(),new System.Collections.Generic.List<int>()};
        for(int j=0;j<h;j++)for(int i=0;i<w;i++){
            int k=i+w*j;float x=(i-45)*1.45f,z=(j-20)*1.45f;float y=Canyon(x,z);
            pos[k]=V(x,y,z);uv[k]=new Vector2(x*.035f,z*.035f);
            if(i==w-1||j==h-1)continue;int band=y< -5?0:(Mathf.FloorToInt(y*.85f+Mathf.Sin(z*.12f))%3==0?1:2);
            indices[band].AddRange(new[]{k,k+w,k+1,k+1,k+w,k+w+1});
        }
        var mesh=d.Own(new Mesh{name="Layered rift terrain"});mesh.vertices=pos;mesh.uv=uv;mesh.subMeshCount=3;
        for(int n=0;n<3;n++)mesh.SetTriangles(indices[n],n);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var dust=d.Mat("Oxide dust","C0794D",null,0,.13f);
        var r=d.Visual("Martian canyon",mesh,dust,d.transform,Vector3.zero,Vector3.one,Quaternion.identity);
        r.GetComponent<MeshRenderer>().sharedMaterials=new[]{dust,d.Mat("Ochre ledges","DF9A65"),d.Mat("Rust strata","9F5838")};r.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh;
        ArkMarsStrataComparison.Apply(d,r);
        d.Mark(V(5,-3,32),3,"峡谷谷底","火星的红色来自含铁尘土。这条可飞越的峡谷是艺术化地形，不是真实地形测绘。");
        var a=V(-18,Canyon(-18,57)+1,57);d.Mark(a,3,"峡壁岩层","沿谷壁向上飞，再回头看低处的谷底。");
    }
}
}

