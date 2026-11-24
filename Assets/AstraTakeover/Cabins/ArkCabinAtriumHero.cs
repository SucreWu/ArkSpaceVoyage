using UnityEngine;
using System.Collections.Generic;

public sealed partial class ArkCabinWing {
    const string HStone="AAA391", HWarm="C1A685", HBrass="A07A4B", HWood="6E4931", HGreen="45633D", HGlass="!9FBAC6", HLamp="!FFD097";
    readonly List<Mesh> heroMeshes = new List<Mesh>();
    Material heroWater;
    Renderer lanternGlobe;
    Transform lanternHandle;
    Light lanternLight;
    bool lanternLit;
    void ReleaseAtriumHero(){foreach(var m in heroMeshes)if(m)Destroy(m);if(heroWater)Destroy(heroWater);foreach(var m in heroLeafColors.Values)if(m)Destroy(m);}
    float Brook(float z)=>-3.1f+.42f*Mathf.Sin(z*.18f)-.018f*z;
    float HalfBrook(float z)=>1.4f+.22f*Mathf.Sin(z*.41f);
    void HeroBeam(string label, Vector3 a,Vector3 b,float w,float d,string col,bool hit=false){
        var t=Part(label,PrimitiveType.Cube,(a+b)*.5f,V(w,(b-a).magnitude,d),col,hit);
        t.localRotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
    }
    void HeroQuad(string n,Vector3 a,Vector3 b,Vector3 c,Vector3 d,string col,bool collider=false,Material custom=null){
        var mesh=new Mesh{name=n};mesh.vertices=new[]{a,b,c,d};
        mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};
        mesh.triangles=new[]{0,1,2,0,2,3,2,1,0,3,2,0};var normal=Vector3.Cross(c-a,b-a).normalized;if(normal.y<0)normal=-normal;mesh.normals=new[]{normal,normal,normal,normal};mesh.RecalculateBounds();
        var g=Group(n,Vector3.zero).gameObject;
        g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=custom?custom:Paint(col);
        if(collider)g.AddComponent<MeshCollider>().sharedMesh=mesh;heroMeshes.Add(mesh);
    }
    void BuildAtriumHero(){
        Box("South wall port",-5.5f,3,0,7,6,.3f,HWarm,true);
        Box("South wall starboard",5.5f,3,0,7,6,.3f,HWarm,true);
        Box("South roof end above vestibule",0,8.9f,-.05f,18,11.4f,.25f,"788283",true);
        Box("North apartment/recycling bulkhead",0,7.05f,24,18,14.1f,.32f,"506068",true);
        foreach(float x in new[]{-9f,9f}){
            Box("Lower envelope",x,2.6f,4.85f,.25f,5.2f,9.7f,HStone,true);
            Box("Lower envelope",x,2.6f,19.15f,.25f,5.2f,9.7f,HStone,true);
            Box("Transfer shoulder lintel",x,4.7f,12,.34f,2.9f,6.1f,HBrass,true);
        }
        Box("Entry crossing deck",0,-.18f,1.14f,18,.36f,2.28f,HStone,true);
        Box("Mid basin crossing deck",0,-.18f,12,18,.36f,4,HStone,true);
        Box("Upper basin crossing deck",0,-.18f,23.05f,18,.36f,1.9f,HStone,true);
        var watershader=Resources.Load<Shader>("Astra/AtriumMovingBrook");
        if(watershader)heroWater=new Material(watershader);
        foreach(var interval in new[]{new Vector2(2.28f,10), new Vector2(14,22.10f)}){
            for(int i=0;i<10;i++){
                float za=Mathf.Lerp(interval.x,interval.y,i/10f),zb=Mathf.Lerp(interval.x,interval.y,(i+1)/10f);
                float a=Brook(za),b=Brook(zb),r=HalfBrook(za),s=HalfBrook(zb);
                var stone=i%4==0?"9E9380":HStone;
                HeroQuad("Port flagstone",V(-9,0,za),V(a-r,0,za),V(b-s,0,zb),V(-9,0,zb),stone,true);
                HeroQuad("Starboard flagstone",V(a+r,0,za),V(9,0,za),V(9,0,zb),V(b+s,0,zb),stone,true);
                HeroQuad("Live brook",V(a-r,-.29f,za),V(a+r,-.29f,za),V(b+s,-.29f,zb),V(b-s,-.29f,zb),"476E75",false,heroWater);
                foreach(int sign in new[]{-1,1}){
                    var u=V(a+r*sign,.49f,za);var v=V(b+s*sign,.49f,zb);
                    HeroBeam("Full brook safety barrier",u,v,.10f,.98f,HWood,true);
                    // Dark thin coping and reed heads silhouette the manufactured retaining wall.
                    HeroBeam("Hand polished mahogany coping",u+V(0,.54f,0),v+V(0,.54f,0),.16f,.10f,HBrass);
                }
            }
        }
        // Basins are separated for the original deck-height crossing; the far cascade is a three dimensional fall.
        for(int i=0;i<4;i++){
            float x=Brook(21.75f)+(i-1.5f)*.61f;
            var rock=Egg("Terraced schist ledge",V(x,.39f+i*.08f,21.82f),V(1.15f,1.61f+i*.34f,1.08f),i%2==0?"546050":"74756B");
            rock.localRotation=Quaternion.Euler(14+i*8,i*43,10);
            HeroQuad("Aerated spill sheet",V(x-.21f,-.27f,21.35f),V(x+.22f,-.27f,21.35f),V(x+.19f,1.12f+i*.13f,21.55f),V(x-.15f,1.12f+i*.13f,21.55f),"!A3BDC3",false,heroWater);
        }
        // Roof is a full asymmetrical envelope, open in color rather than screen-black.
        Vector3[] arc={V(-9,5,0),V(-8.1f,8.65f,0),V(-5.4f,11.7f,0),V(-1.4f,13.5f,0),V(3.45f,13.9f,0),V(6.65f,12.9f,0),V(8.62f,10.6f,0),V(9,7,0),V(9,4,0)};
        for(int j=0;j<arc.Length-1;j++){
            for(int bay=0;bay<8;bay++){
                Vector3 a=arc[j]+V(0,0,bay*3),b=arc[j+1]+V(0,0,bay*3);
                HeroQuad("Laminated luminous glazing",a,b,b+V(0,0,3.005f),a+V(0,0,3.005f),j<2||j>5?"778284":(bay%3==1?"!ADC4CE":HGlass));
                HeroBeam("Vault transverse casting",a,b,.13f,.33f,"526368");
            }
            HeroBeam("Curved vault long seam",arc[j],arc[j]+V(0,0,24),.12f,.13f,"748388");
        }
        // Four residential fabric layers to camera-right. Each row is broken by a different step profile.
        for(int floor=0;floor<4;floor++){
            float y=3.1f+floor*2.03f;
            for(int i=0;i<7;i++){
                float z=1.71f+i*3.4f,shift=.31f*Mathf.Sin(i*1.3f+floor*1.8f);
                float front=7.07f+floor*.23f+shift;
                Box("Deep residential shell",8.61f,y+.88f,z,1.2f,1.76f,3.38f,floor%2==0?"627073":"596569");
                Box("Stone cantilever band",(front+9)*.5f,y,z,9-front,.25f,3.46f,HStone);
                Box("Window recess",7.985f,y+.90f,z,.10f,1.21f,2.26f,"4C5553");
                for(int p=0;p<3;p++){
                    float at=z+(p-1)*.682f;
                    Box("Individual home glass",7.921f,y+.88f,at,.045f,1.04f,.612f,(floor*7+i+p)%5==0?"!B99F77":((i+p)%3==0?"819A9A":"567376"));
                    Box("Bronze glazing post",7.882f,y+.92f,at+.32f,.05f,1.24f,.042f,HBrass);
                }
                Box("Balcony handrail",front,y+.71f,z,.095f,.13f,3.32f,HBrass);
                if(floor==0 && i%2==0){
                    Box("Hand-tiled planter",front+.24f,y+.23f,z+.36f,.43f,.42f,1.22f,"496D65");
                    Box("Cypress greenhouse fin",front+.45f,y+1.05f,z-1.26f,.11f,1.88f,.37f,"85A294");
                }else if(floor==1 && i%3!=0){
                    var sun=Box("Dining gallery shade",front+.45f,y+1.47f,z+.22f,1.14f,.09f,1.88f,"9C675A");
                    sun.localRotation=Quaternion.Euler(0,0,-11);
                    Box("Dining gallery cypress seat",front+.34f,y+.26f,z+.29f,.45f,.30f,.97f,HWood);
                }else if(floor==2 && i%3==1){
                    Box("Shared hall nameplate",front+.12f,y+.97f,z+.82f,.10f,.40f,1.22f,"B39760");
                    for(int slat=0;slat<5;slat++)Box("Shared hall cedar privacy slat",front+.27f,y+1.06f,z-.84f+slat*.14f,.13f,1.82f,.074f,"79583E");
                }else if(floor==3 && i%2==0){
                    Box("Roof irrigation manifold",front+.34f,y+.28f,z,.23f,.21f,2.53f,"557779");
                }
                if((floor+i)%3!=0) HeroBoughs(V(front+.15f,y+.07f,z-.83f),floor%2==0?.66f:.85f,92+i*45+floor*337,true);
            }
        }
        // Two west lofts staggered at either end: not a copy of the east streetwall.
        Box("South west greenhouse bay",-8.23f,4.0f,4,1.5f,2.9f,7.7f,"606A5F");
        Box("Port apartment wall behind ramp",-8.79f,5.47f,19.7f,.36f,4.6f,8.25f,HStone);
        for(int i=0;i<5;i++)Box("Nearby amber windows",-7.44f,3.86f,1.1f+i*1.36f,.10f,1.2f,1.07f,HLamp);
        // Overhead crossing, offset and angled; accessible via port ramp alongside the bank.
        Vector3 start=V(-7.65f,2.9f,22.7f),end=V(6.95f,2.9f,21.2f);
        Box("Port turn landing",-7.28f,2.81f,22.54f,2.28f,.18f,2.77f,HWarm,true);
        Box("Port landing north guard",-7.28f,3.43f,23.86f,2.28f,1.06f,.10f,HBrass,true);
        Box("Port landing outside guard",-8.40f,3.43f,22.51f,.09f,1.06f,2.70f,HBrass,true);
        // The pad, not the bridge rail, owns the shared perimeter here.
        Box("Port pad south rim outside ramp mouth",-6.52f,3.43f,21.16f,.76f,1.06f,.10f,HBrass,true);
        Box("Port pad southeast return",-6.14f,3.43f,21.36f,.10f,1.06f,.40f,HBrass,true);
        HeroWalk("High diagonal garden bridge",start,end,1.96f,-6.14f);
        HeroWalk("West promenade ramp",V(-7.63f,0,12.90f),V(-7.65f,2.9f,21.20f),1.48f);
        Box("Upper view landing",7.08f,2.81f,21.9f,2.18f,.18f,2.83f,HStone,true);
        Box("Landing end safety guard",7.1f,3.43f,23.22f,2.19f,1.06f,.12f,HBrass,true);
        Box("Landing south safety guard",7.1f,3.43f,20.51f,2.19f,1.06f,.12f,HBrass,true);
        Box("Landing outer safety guard",8.15f,3.43f,21.9f,.12f,1.06f,2.83f,HBrass,true);
        // Read three silhouettes instead of a duplicated prefab: canvas produce stall, lantern pergola, supper kiosks.
        HeroStall(6.38f,4.4f,2.5f,3.7f,0);
        HeroStall(6.62f,8,2.18f,2.82f,1);
        BuildFestivalBench();
        HeroStall(6.77f,15.74f,2.24f,2.85f,2);
        HeroStall(6.64f,19.25f,2.52f,3.45f,0);
        HeroBoughs(V(-6.08f,.12f,4.2f),4.6f,111,false);
        HeroBoughs(V(-6.14f,.09f,16.45f),4.2f,634,false);
        HeroBoughs(V(-.97f,.10f,20.2f),3.7f,310,false);
        HeroBoughs(V(4.66f,.08f,6.38f),2.55f,994,false);
        Lamp(V(-.8f,8.7f,7),new Color(.74f,.88f,1),22,2.0f);
        Lamp(V(2.2f,7.5f,18.1f),new Color(.74f,.88f,1),16,1.5f);
        Lamp(V(5.35f,2.4f,6.2f),new Color(1,.69f,.37f),8,2.45f);
        Lamp(V(4.3f,2.6f,18.6f),new Color(1,.70f,.43f),7.5f,1.75f);
    }
    void HeroWalk(string label,Vector3 a,Vector3 b,float width,float southRailBeginsAtX=float.NaN){
        Vector3 lateral=Vector3.Cross(Vector3.up,b-a).normalized * width*.5f;
        HeroQuad(label,a+lateral,a-lateral,b-lateral,b+lateral,HWarm,true);
        foreach(int side in new[]{-1,1}){
            var u=a+lateral*side;var v=b+lateral*side;
            if(side==1 && !float.IsNaN(southRailBeginsAtX))
                u=Vector3.Lerp(u,v,Mathf.InverseLerp(u.x,v.x,southRailBeginsAtX));
            var level=V(0,.53f,0);
            // Continuous blocker with thin top rail, side posts and visible gaps.
            var g=Group(label+" collision rail",(u+v)*.5f+level);
            g.localRotation=Quaternion.LookRotation((v-u).normalized,Vector3.up);
            var box=g.gameObject.AddComponent<BoxCollider>();box.size=V(.11f,1.1f,Vector3.Distance(u,v));
            HeroBeam("Hewn rail",u+V(0,1.02f,0),v+V(0,1.02f,0),.10f,.11f,HBrass);
            HeroBeam("Walk support",u-V(0,.14f,0),v-V(0,.14f,0),.13f,.11f,HStone);
            int count=Mathf.CeilToInt(Vector3.Distance(u,v)/.94f);
            for(int i=0;i<=count;i++){
                var p=Vector3.Lerp(u,v,i/(float)count);
                HeroBeam("Bridge baluster",p,p+V(0,1,0),.065f,.065f,HWood);
            }
        }
    }
    void HeroStall(float x,float z,float w,float d,int kind){
        var palette=new[]{"B07E54","D8B788","934D4E"};string canopyCol=palette[kind];
        float h=kind==1?2.65f:2.35f;
        var a=V(x-w*.5f,h,z-d*.5f);var b=V(x+w*.5f,h+.2f,z-d*.5f);
        var c=V(x+w*.5f,h+.34f,z+d*.5f);var e=V(x-w*.5f,h+.08f,z+d*.5f);
        foreach(Vector3 p in new[]{a,b,c,e})HeroBeam("Market timber post",p,V(p.x,0,p.z),.095f,.095f,HWood,true);
        for(int bay=0;bay<8;bay++){
            float t=bay/8f,u=(bay+1)/8f;
            var p=Vector3.Lerp(a,b,t);var q=Vector3.Lerp(a,b,u);var r=Vector3.Lerp(e,c,u);var s=Vector3.Lerp(e,c,t);
            if(kind!=1||bay%2==0){
                var center=(p+q+r+s)*.25f-V(0,.20f,0);
                HeroQuad("Tensioned awning",p,q,center,center,canopyCol);
                HeroQuad("Tensioned awning",q,r,center,center,canopyCol);
                HeroQuad("Tensioned awning",r,s,center,center,canopyCol);
                HeroQuad("Tensioned awning",s,p,center,center,canopyCol);
            }
        }
        for(int i=0;i<5;i++){
            float j=z+(i-2)*d*.18f;
            var bulb=Egg("Cloth festival lantern",V(x-.45f*w,h-.26f-(i%3)*.1f,j),V(.25f,.36f,.25f),HLamp);
            bulb.localRotation=Quaternion.Euler(0,i*21,0);
        }
        if(kind==0){Box("Produce trestle",x,.44f,z,w*.7f,.88f,d*.76f,HWood,true);
            for(int i=0;i<16;i++)Egg("Baskets and citrus",V(x+.21f*Mathf.Sin(i*4),.96f,z+(i-7.5f)*d*.046f),V(.19f,.14f,.19f),i%3==0?"B96146":"C9994C");
        }else{
            for(int i=0;i<2;i++){float j=z+(i-.5f)*1.18f;Box("Dining pedestal",x,.35f,j,.16f,.7f,.16f,HWood,true);Box("Tea table",x,.75f,j,1.04f,.10f,.76f,HWarm,true);
                for(int k=-1;k<=1;k+=2)Box("Cafe bench",x+k*.62f,.23f,j,.32f,.46f,.77f,HWood,true);}
        }
    }
    void HeroBoughs(Vector3 p,float h,int seed,bool trailing){
        var rng=new System.Random(seed);
        var mesh=new Mesh{name="Folded living canopy "+seed}; var verts=new List<Vector3>();var idx=new List<int>();
        Vector3 fork=p+V(h*.14f,trailing?-h*.34f:h*.61f,h*.06f);
        if(!trailing)HeroBeam("Gnarled cedar trunk",p,fork,h*.12f,h*.10f,"534435",true);
        int count=trailing?4:9, leaves=trailing?32:150;
        for(int b=0;b<count;b++){
            float a=(b*2.39996f)+(seed%13)*.17f;var axis=V(Mathf.Sin(a),0,Mathf.Cos(a));
            float len=h*(trailing?.27f:(.22f+b%4*.085f));
            Vector3 begin=Vector3.Lerp(p,fork,.57f+(b%4)*.12f);
            Vector3 tip=fork+axis*len+V(0,(trailing?-.44f:.20f+(b%3)*.12f)*h,0);
            HeroBeam("Branch",begin,tip,h*.028f,h*.033f,"74614A");
            for(int l=0;l<leaves;l++){
                var center=Vector3.Lerp(begin,tip,.6f+(float)rng.NextDouble()*.56f)+V((float)rng.NextDouble()-.5f,((float)rng.NextDouble()-.5f)*.36f,(float)rng.NextDouble()-.5f)*len;
                Vector3 t=Quaternion.Euler((float)rng.NextDouble()*70, (float)rng.NextDouble()*360,0)*V(.052f*h,0,0);
                Vector3 k=Vector3.Cross(t,Vector3.up).normalized*.027f*h;
                int n=verts.Count;verts.Add(center-t);verts.Add(center+k);verts.Add(center+t);verts.Add(center-k);verts.Add(center+V(0,h*.018f,0));
                for(int j=0;j<4;j++){int u=n+j,v=n+(j+1)%4;idx.Add(u);idx.Add(v);idx.Add(n+4);idx.Add(v);idx.Add(u);idx.Add(n+4);}
            }
        }
        mesh.SetVertices(verts);mesh.SetTriangles(idx,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var g=Group(mesh.name,Vector3.zero).gameObject;
        g.AddComponent<MeshFilter>().sharedMesh=mesh;
        var rr=g.AddComponent<MeshRenderer>();rr.sharedMaterial=HeroFoliage(seed);rr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        heroMeshes.Add(mesh);
    }
    readonly Dictionary<int,Material> heroLeafColors=new Dictionary<int,Material>();
    Material HeroFoliage(int seed) {
        int variant=seed%4;
        if(heroLeafColors.TryGetValue(variant,out var cached))return cached;
        var shader=Resources.Load<Shader>("Astra/AtriumLeafTransmission");
        if(!shader)return Paint("697A44");
        var material=new Material(shader);
        material.color=new[]{new Color(.36f,.43f,.18f),new Color(.31f,.40f,.15f),new Color(.41f,.46f,.23f),new Color(.31f,.40f,.21f)}[variant];
        heroLeafColors[variant]=material;return material;
    }
    void BuildFestivalBench(){
        Box("Lantern brass jig",5.92f,.59f,8.0f,.68f,1.18f,.96f,HWood,true);
        Box("Lantern work surface",5.92f,1.17f,8.0f,.72f,.10f,1.02f,"9A6849");
        Box("Brass lamp foot",5.78f,1.26f,8.10f,.28f,.07f,.26f,HBrass);
        lanternGlobe=Egg("Working festival lantern",V(5.78f,1.48f,8.10f),V(.29f,.38f,.29f),"AE9572").GetComponent<Renderer>();
        for(int i=0;i<6;i++){
            float angle=Mathf.PI*i/3f;
            Vector3 p=V(5.78f+.136f*Mathf.Cos(angle),1.48f,8.10f+.136f*Mathf.Sin(angle));
            HeroBeam("Paper lantern hoop rib",p-V(0,.135f,0),p+V(0,.135f,0),.017f,.017f,HBrass);
        }
        lanternHandle=Group("Lantern dimmer lever",V(5.66f,1.23f,7.69f));
        Part("Ebony and bronze control",PrimitiveType.Cube,V(0,.10f,0),V(.044f,.20f,.07f),HBrass,false,lanternHandle);
        Box("Folded repair papers",6.08f,1.241f,7.97f,.21f,.022f,.34f,"B79F7E");
        lanternLight=Group("Lantern hand-lit glow",V(5.67f,1.62f,8.02f)).gameObject.AddComponent<Light>();
        lanternLight.type=LightType.Point;lanternLight.color=new Color(1,.64f,.29f);lanternLight.range=3.7f;lanternLight.intensity=1.75f;lanternLight.enabled=false;
    }
    public bool ToggleFestivalLantern(){
        if(!lanternGlobe)return false;
        lanternLit=!lanternLit;
        lanternGlobe.sharedMaterial=Paint(lanternLit?"!FFB34D":"AE9572");
        lanternHandle.localRotation=Quaternion.Euler(lanternLit?-38:38,0,0);
        lanternLight.enabled=lanternLit;
        return lanternLit;
    }
}
