using UnityEngine;
using static ArkWalk.DistrictSet;

namespace ArkWalk {
internal static class ArkIceGiants {
    const int w=768,h=384;
    const string NeptuneRaw="Districts/neptune_storm_accepted_rgba32";
    static Color32[] CloudPixels(bool neptune,bool irregularStorm){
        var px=new Color32[w*h];
        Color low=C(neptune?"526B88":"86B2B0"),high=C(neptune?"B1CED8":"C8DFD7");
        // Parse immutable storm colors once, rather than for each atlas texel.
        Color stormInk=neptune?C("263C57"):default,stormCloud=neptune?C("F4FCFF"):default;
        long perfLoop=ArkRoutePerf.Tick();
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){
            float u=(x+.5f)/w,v=(y+.5f)/h,a=u*2*Mathf.PI;
            float f=Mathf.PerlinNoise(24+Mathf.Cos(a)*5,29+v*32+Mathf.Sin(a)*5);
            float belt=.5f+.15f*Mathf.Sin(v*57+f*2)+.15f*(f-.5f);
            Color c=Color.Lerp(low,high,belt);
            if(neptune&&irregularStorm){
                // Stylized sheared dark storm and faint trailing cloud, not an observation.
                float du=Mathf.DeltaAngle(u*360,273)/360f/.056f, dv=(v-.55f)/.064f;
                float uu=du+.15f*dv+.073f*Mathf.Sin(2.3f*dv);
                float vv=dv-.18f*du-.094f*Mathf.Sin(2f*du);
                float r=Mathf.Sqrt(uu*uu+vv*vv+.18f*uu*vv);
                float oval=(1-Mathf.SmoothStep(0,1,(r-.43f)/.64f))*.67f;
                float tail=Mathf.SmoothStep(0,1,(du-.52f)/.67f)*(1-Mathf.SmoothStep(0,1,(du-2.05f)/.93f));
                float axis=.49f-.105f*du+.14f*Mathf.Sin(1.6f*du)+(f-.5f)*.32f;
                float wakeOffset=(dv-axis)*Mathf.Lerp(4.9f,2.2f,Mathf.Clamp01(du*.31f));
                float wake=tail*Mathf.Exp(-wakeOffset*wakeOffset)*(.105f+.105f*f);
                c=Color.Lerp(c,stormInk,oval);
                c=Color.Lerp(c,stormCloud,wake);
            } else if(neptune){
                float du=Mathf.DeltaAngle(u*360,273)/360f/.056f, dv=(v-.55f)/.064f;
                float r=Mathf.Sqrt(du*du+dv*dv+.35f*du*dv);
                float oval=(1-Mathf.SmoothStep(0,1,(r-.54f)/.44f))*.82f;
                float ribbon=Mathf.Exp(-Mathf.Pow((r-1.23f)*15,2)) * Mathf.Clamp01(.4f+.6f*dv);
                float wake=Mathf.Exp(-Mathf.Pow((v-.63f-.012f*Mathf.Sin(du*1.8f))*103,2)) * Mathf.Exp(-Mathf.Pow((du-1.4f)*.57f,2)) * (.45f+.55f*f);
                c=Color.Lerp(c,stormInk,oval);
                c=Color.Lerp(c,stormCloud,Mathf.Clamp01(ribbon*.73f+wake*.52f));
            }
            px[y*w+x]=c;
        }
        ArkRoutePerf.Stop("atlas_loop",perfLoop);
        return px;
    }
    internal static byte[] BakeAcceptedNeptune(){
        var pixels=CloudPixels(true,true);
        var raw=new byte[w*h*4];
        for(int i=0,j=0;i<pixels.Length;i++,j+=4){
            var p=pixels[i];raw[j]=p.r;raw[j+1]=p.g;raw[j+2]=p.b;raw[j+3]=p.a;
        }
        return raw;
    }
    internal static bool CheckBakedNeptune(){
        var baked=Resources.Load<TextAsset>(NeptuneRaw);
        if(!baked)return false;
        var raw=baked.bytes;var generated=BakeAcceptedNeptune();
        if(raw.Length!=generated.Length)return false;
        for(int i=0;i<raw.Length;i++)if(raw[i]!=generated[i])return false;
        return true;
    }
    static Texture2D CloudTexture(DistrictSet d,bool neptune,bool irregularStorm,bool forceGenerate=false){
        byte[] raw=null;
        if(neptune&&irregularStorm&&!forceGenerate){
            long perfLoad=ArkRoutePerf.Tick();
            var baked=Resources.Load<TextAsset>(NeptuneRaw);
            if(baked)raw=baked.bytes;
            ArkRoutePerf.Stop("atlas_load",perfLoad);
        }
        bool ready=raw!=null&&raw.Length==w*h*4;
        var px=ready?null:CloudPixels(neptune,irregularStorm);
        long perfUpload=ArkRoutePerf.Tick();
        var tex=d.Own(new Texture2D(w,h,TextureFormat.RGBA32,true){name=neptune?"Storm and companion cloud atlas":"Quiet polar bands",wrapModeU=TextureWrapMode.Repeat,wrapModeV=TextureWrapMode.Clamp,anisoLevel=4});
        if(ready)tex.SetPixelData(raw,0);else tex.SetPixels32(px);
        tex.Apply(true,true);
        ArkRoutePerf.Stop("atlas_upload",perfUpload);
        return tex;
    }
    static Material Clouds(DistrictSet d,bool neptune,bool irregularStorm=false){
        var tex=CloudTexture(d,neptune,irregularStorm);
        var m=d.Mat(neptune?(irregularStorm?"Neptune sheared dark storm":"Neptune dynamic storm study"):"Uranus calm atmosphere","FFFFFF",null,0,.14f,.12f);
        m.mainTexture=tex;return m;
    }
    static Mesh Loop(DistrictSet d,float r,float w){
        const int n=160;var v=new Vector3[(n+1)*2];var tris=new int[n*12];
        for(int i=0;i<=n;i++){
            float a=i*(2*Mathf.PI/n);var radial=V(Mathf.Cos(a),0,Mathf.Sin(a));
            v[2*i]=radial*(r-w*.5f);v[2*i+1]=radial*(r+w*.5f);
            if(i==n)break;int k=i*2,t=i*12;
            tris[t]=k;tris[t+1]=k+2;tris[t+2]=k+1;tris[t+3]=k+1;tris[t+4]=k+2;tris[t+5]=k+3;
            // Opposite side for fly-through view of this thin ribbon.
            tris[t+6]=k;tris[t+7]=k+1;tris[t+8]=k+2;tris[t+9]=k+1;tris[t+10]=k+3;tris[t+11]=k+2;
        }
        var m=d.Own(new Mesh{name="Narrow ring ribbon"});m.vertices=v;m.triangles=tris;
        var no=new Vector3[v.Length];for(int i=0;i<no.Length;i++)no[i]=Vector3.up;m.normals=no;m.RecalculateBounds();return m;
    }
    internal static void Uranus(DistrictSet d,Mesh sphere){
        var p=V(1,7,116);var rotation=Quaternion.Euler(22,0,97.77f);
        var globe=d.Visual("Uranus side-on poles",sphere,Clouds(d,false),d.transform,p,V(32,31,32),rotation);
        globe.gameObject.AddComponent<SphereCollider>().radius=1;
        var args=System.Environment.GetCommandLineArgs();
        if(System.Array.IndexOf(args,"-arkVerify")>=0 && System.Array.IndexOf(args,"-arkUranusAB")>=0)ArkUranusOfficial.Apply(d,globe);
        ArkUranusRingComparison ringAB=null;
        if(System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkUranusRingAB")>=0)
            ringAB=d.gameObject.AddComponent<ArkUranusRingComparison>();
        var ringShader=Resources.Load<Shader>("ArkUranusRingTransmission");
        float[] r={35,38,40,42,44,48,53,60};
        for(int i=0;i<r.Length;i++){
            string tint=i==7?"7196A6":i==6?"886B60":"777876";
            float opacity=i==5?.63f:i>=6?.38f:.47f;
            var previous=d.Mat("Gossamer ring "+i,tint,null,0,.1f,.31f);
            var m=previous;
            if(ringShader&&ringShader.isSupported){
                m=d.Mat("Gossamer ring transmitted "+i,tint);
                m.shader=ringShader;var c=C(tint);c.a=opacity;m.color=c;
                m.renderQueue=3000;m.SetVector("_Sun",new Vector4(-.9f,.34f,-.46f,0));
            }
            var mesh=Loop(d,r[i],i==5?.9f:.42f);
            var visual=d.Visual("Uranus equatorial ribbon "+i,mesh,m,d.transform,p,Vector3.one,rotation);
            var renderer=visual.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows=true;
            if(ringAB)ringAB.Previous[renderer]=previous;
        }
        // Offset navigation mast, never in the common arrival/return lane.
        var q=V(18,8,43);
        d.Rod(q+V(0,-2,0),q+V(0,2,0),.13f,d.Steel);
        d.Rod(q+V(-2,0,0),q+V(2,0,0),.095f,d.Steel);
        d.Ball(q,Vector3.one*.55f,d.Gold);
        d.Mark(q,3,"侧滚视角","天王星自转轴倾角约九十八度，看起来像侧着滚动；相对行星赤道的暗环也随之倾斜。环的宽度与距离为漫游夸张。");
        d.Mark(V(-9,29,83),6,"冰巨星极区","甲烷吸收红光，使天王星呈淡蓝绿色。这里没有可降落的固体表面。");
    }
    internal static void Neptune(DistrictSet d,Mesh sphere){
        var p=V(16,9,116);
        var existing=Clouds(d,true,true);
        var material=existing;
        var detail=Resources.Load<Shader>("ArkNeptuneCloudDetail");
        if(detail&&detail.isSupported){
            material=d.Mat("Neptune quiet sheared cloud detail","FFFFFF",null,0,.14f,.12f);
            material.mainTexture=existing.mainTexture;
            material.shader=detail;
        }
        var globe=d.Visual("Neptune dark vortex and bright companions",sphere,material,d.transform,p,Vector3.one*33,Quaternion.identity);
        globe.gameObject.AddComponent<SphereCollider>().radius=1;
        var args=System.Environment.GetCommandLineArgs();
        bool verify=System.Array.IndexOf(args,"-arkVerify")>=0;
        if(verify&&(System.Array.IndexOf(args,"-arkNeptuneDetailAB")>=0||System.Array.IndexOf(args,"-arkNeptuneRawAB")>=0)){
            var witness=globe.gameObject.AddComponent<ArkNeptuneStormComparison>();
            witness.PriorDetail=existing;
            if(System.Array.IndexOf(args,"-arkNeptuneRawAB")>=0)witness.Generated=CloudTexture(d,true,true,true);
        }
        if(System.Array.IndexOf(args,"-arkVerify")>=0&&System.Array.IndexOf(args,"-arkNeptuneAB")>=0)
            globe.gameObject.AddComponent<ArkNeptuneStormComparison>().Previous=Clouds(d,true);
        var q=V(-18,5,39);
        var moon=d.Own(Object.Instantiate(sphere));moon.name="Triton mottled relief";var v=moon.vertices;
        for(int i=0;i<v.Length;i++){
            var n=v[i];float f=Mathf.PerlinNoise(26+n.x*11,31+n.z*11+n.y*9);
            v[i]*=.98f+(f-.5f)*.055f;
        }
        moon.vertices=v;moon.RecalculateNormals();moon.RecalculateBounds();
        d.Visual("Triton foreground flyby",moon,d.Mat("Triton ice and frost","D4C2BB","moon",0,.27f),d.transform,q,Vector3.one*6.4f,Quaternion.Euler(18,32,0)).gameObject.AddComponent<SphereCollider>().radius=1.04f;
        d.Mark(q+V(0,0,-6.8f),3,"逆行的海卫一","海卫一的公转方向和海王星自转方向相反，冰质卫星从侧前方掠过。此处地貌和运动尺度均作艺术化处理。");
        d.Mark(V(15,14,80),7,"风暴与伴生亮云","深色的大气涡旋会生成、变化和消散；这里演绎的是一种风暴结构，不是永久斑纹，也不是实况。");
    }
}
}
