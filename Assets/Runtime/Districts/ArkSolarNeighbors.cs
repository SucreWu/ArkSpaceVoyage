using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk {
internal static class ArkSolarNeighbors {
    // A stylized orbital atlas: distances, terrain relief, cloud colors and time are compressed.
    static readonly Vector3[] Impacts={
        V(-.22f,.18f,-1).normalized,V(.4f,-.27f,-1).normalized,V(.48f,.48f,-1).normalized,
        V(-.65f,-.37f,-1).normalized,V(-.78f,.37f,-1).normalized,V(.9f,.08f,-1).normalized,
        V(-.3f,-.65f,-1).normalized,V(.21f,.73f,-1).normalized,V(.62f,-.63f,-1).normalized
    };
    static Vector3 Dir(float u,float v){
        float a=u*Mathf.PI*2,b=(v-.5f)*Mathf.PI,c=Mathf.Cos(b);
        return V(Mathf.Cos(a)*c,Mathf.Sin(b),Mathf.Sin(a)*c);
    }
    static float Noise(Vector3 p,float f) {
        p*=f;
        return (Mathf.PerlinNoise(p.x+42,p.y+57)+Mathf.PerlinNoise(p.y+67,p.z+36)+Mathf.PerlinNoise(p.z+85,p.x+29))/3;
    }
    static float Relief(Vector3 n){
        float h=(Noise(n,17)-.5f)*.018f;
        for(int i=0;i<Impacts.Length;i++){
            float t=(n-Impacts[i]).magnitude/(i==0?.27f:.12f+i%3*.034f);
            if(t<1.38f){float r=(t-1)*5;h+=-.037f*Mathf.Max(0,1-t*t)+.021f*Mathf.Exp(-r*r);}
        }
        return h;
    }
    const int W=768,H=384;
    static Color32[] SurfacePixels(bool venus,bool quietVenus,bool quietMercury){
        const int w=W,h=H;var pixels=new Color32[w*h];
        var a=C(quietVenus?"D3C8B1":venus?"9D6A46":"655E58");var b=C(quietVenus?"F4EDDB":venus?"FFF3CE":"C6B49F");
        long perfLoop=ArkRoutePerf.Tick();
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){
            var n=Dir((x+.5f)/w,(y+.5f)/h);
            float v;
            if(quietVenus){
                // Authored opaque broadband visible-light appearance, not a UV image.
                // 3D noise keeps both longitude wrap and poles continuous.
                float flow=Noise(V(n.x,n.y*3.7f,n.z),5);
                float shear=Noise(V(n.x+(flow-.5f)*.20f,n.y*2.5f,n.z),12);
                float circulation=Mathf.Sin(12*n.y+4*(flow-.5f));
                float cap=Mathf.SmoothStep(0,1,(Mathf.Abs(n.y)-.61f)/.32f);
                v=.53f+.36f*(flow-.5f)+.27f*(shear-.5f)+.067f*circulation+.06f*cap;
            } else if(venus){
                float angle=Mathf.Atan2(n.z,n.x);
                float broad=Noise(n,4), thin=Noise(V(n.x,n.y*4,n.z),29);
                float streak=Mathf.Sin(42*n.y+4*Mathf.Sin(2*angle)+5*broad);
                float polar=Mathf.SmoothStep(0,1,(Mathf.Abs(n.y)-.6f)/.35f);
                v=.52f+.26f*streak+(thin-.5f)*.42f+polar*.21f;
            } else {
                var q=n-Impacts[0];float r=q.magnitude;
                float rays=(.5f+.5f*Mathf.Sin(Mathf.Atan2(q.y,q.x)*19+Noise(n,14)*2))*.21f*Mathf.Max(0,1-r/.78f);
                v=.48f+Relief(n)*4+(Noise(n,38)-.5f)*(quietMercury?.75f:1.8f)+rays;
            }
            pixels[x+y*w]=Color.Lerp(a,b,Mathf.Clamp01(v));
        }
        ArkRoutePerf.Stop("atlas_loop",perfLoop);
        return pixels;
    }
    static string BakedName(bool venus)=>"Districts/"+(venus?"venus":"mercury")+"_surface_accepted_rgba32";
    internal static byte[] BakeAcceptedSurface(bool venus) {
        var colors=SurfacePixels(venus,venus,!venus);
        var rgba=new byte[W*H*4];
        for(int i=0,j=0;i<colors.Length;i++,j+=4){
            var c=colors[i];rgba[j]=c.r;rgba[j+1]=c.g;rgba[j+2]=c.b;rgba[j+3]=c.a;
        }
        return rgba;
    }
    internal static bool CheckBakedSurface(bool venus) {
        var raw=Resources.Load<TextAsset>(BakedName(venus));
        if(!raw)return false;
        var actual=raw.bytes;var expected=BakeAcceptedSurface(venus);
        if(actual.Length!=expected.Length)return false;
        for(int i=0;i<actual.Length;i++)if(actual[i]!=expected[i])return false;
        return true;
    }
    static Material Surface(DistrictSet d,bool venus,bool quietVenus=false,bool quietMercury=false){
        const int w=W,h=H;
        byte[] rgba=null;
        if((venus&&quietVenus)||(!venus&&quietMercury)){
            long perfLoad=ArkRoutePerf.Tick();
            var baked=Resources.Load<TextAsset>(BakedName(venus));
            if(baked)rgba=baked.bytes;
            ArkRoutePerf.Stop("atlas_load",perfLoad);
        }
        bool exact=rgba!=null&&rgba.Length==w*h*4;
        Color32[] pixels=exact?null:SurfacePixels(venus,quietVenus,quietMercury);
        long perfUpload=ArkRoutePerf.Tick();
        var tex=d.Own(new Texture2D(w,h,TextureFormat.RGBA32,true){name=venus?"Procedural sulfuric cloud atlas":"Procedural mercurian crater atlas",wrapModeU=TextureWrapMode.Repeat,wrapModeV=TextureWrapMode.Clamp,anisoLevel=4});
        if(exact)tex.SetPixelData(rgba,0);else tex.SetPixels32(pixels);
        tex.Apply(true,true);
        ArkRoutePerf.Stop("atlas_upload",perfUpload);
        var mat=d.Mat(quietVenus?"Venus subdued visible cloud deck":venus?"Cloud top scattering":quietMercury?"Regolith lower fine gain":"Regolith and ejecta","FFFFFF",null,0,quietVenus?.11f:venus?.19f:.12f,venus?.18f:0);
        mat.mainTexture=tex;return mat;
    }
    static Mesh ShapeMercury(Mesh m){
        m.name="Impact-basin relief";var v=m.vertices;
        for(int i=0;i<v.Length;i++)v[i]*=1+Relief(v[i]);m.vertices=v;m.RecalculateNormals();m.RecalculateBounds();
        return m;
    }
    internal static void Mercury(DistrictSet d,Mesh sphere){
        var p=V(5,6,105);
        long perfRelief=ArkRoutePerf.Tick();
        var m=ShapeMercury(DistrictSpace.Round(d,192,96));
        ArkRoutePerf.Stop("relief_mesh",perfRelief);
        var globe=d.Visual("Mercury: impact-basin relief",m,Surface(d,false,false,true),d.transform,p,Vector3.one*32,Quaternion.identity);
        globe.gameObject.AddComponent<SphereCollider>().radius=1.03f;
        var args=System.Environment.GetCommandLineArgs();
        if(System.Array.IndexOf(args,"-arkVerify")>=0){
            if(System.Array.IndexOf(args,"-arkMercuryAB")>=0)
                globe.gameObject.AddComponent<ArkMercuryMaterialComparison>().Previous=Surface(d,false);
            if(System.Array.IndexOf(args,"-arkMercuryReliefAB")>=0)
                globe.gameObject.AddComponent<ArkMercuryMaterialComparison>().PreviousMesh=ShapeMercury(d.Own(Object.Instantiate(sphere)));
        }
        d.Visual("Near Sun (artistic scale)",sphere,d.Mat("Solar photosphere","FFD18C","star",0,.1f,2.6f),d.transform,V(-82,30,161),Vector3.one*26,Quaternion.identity).GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        d.Mark(V(-2,11,69),5,"水星撞击盆地","水星的岩石表面布满撞击坑；大盆地和放射状亮纹为艺术化重建，非实测地形。");
        d.Mark(V(-28,12,58),4,"近日观测点","水星是最靠近太阳的行星。这里的太阳大小、距离和盆地起伏为漫游而调整。");
    }
    internal static void Venus(DistrictSet d,Mesh sphere){
        var p=V(10,6,111);
        var globe=d.Visual("Venus: layered cloud-top atlas",sphere,Surface(d,true,true),d.transform,p,Vector3.one*39,Quaternion.Euler(0,0,-13));
        globe.gameObject.AddComponent<SphereCollider>().radius=1;
        var args=System.Environment.GetCommandLineArgs();
        if(System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkVenusAB")>=0)
            globe.gameObject.AddComponent<ArkVenusComparison>().Previous=Surface(d,true);
        // A small fictional probe silhouettes against the cloud deck. Open space on both sides.
        var q=V(-6,4,26);
        d.Ball(q,V(2.45f,1.55f,2.1f),d.Panels);d.Ball(q+V(0,-.34f,0),V(2.2f,.92f,1.98f),d.Dark);
        for(int i=0;i<3;i++){float a=i*2.0944f;var arm=V(Mathf.Cos(a),0,Mathf.Sin(a));d.Rod(q,q+arm*2.15f,.09f,d.Gold);d.Ball(q+arm*2.15f,Vector3.one*.26f,d.Aqua);}
        d.Rod(q,q+V(0,2,0),.055f,d.Steel);
        var body=d.Group("Venus probe hull");body.localPosition=q;body.gameObject.AddComponent<SphereCollider>().radius=1.2f;
        d.Mark(q+V(0,0,-1.9f),2,"云顶观测探针","从虚构的观测探针旁绕飞，观察金星浓密的硫酸云。云纹与尺度经过艺术化处理。");
        d.Mark(V(10,7,68),6,"金星云顶","金星厚重的大气完全遮住了地表；这里是云顶近轨观测，不进入大气。");
    }
}
}
